using System;
using Microsoft.EntityFrameworkCore;
using ContractReviewScheduler.Data;
using ContractReviewScheduler.Models.Domain;
using Serilog;

namespace ContractReviewScheduler.Services
{
    /// <summary>
    /// 使用者同步服務 - 從 AD 同步使用者到資料庫
    /// </summary>
    public interface IUserSyncService
    {
        /// <summary>
        /// 同步使用者資訊到資料庫
        /// </summary>
        Task<(bool Success, string? Message)> SyncUserAsync(string adAccount, string name, string email, string role);

        /// <summary>
        /// 取得或建立使用者
        /// </summary>
        Task<User?> GetOrCreateUserAsync(string adAccount);

        /// <summary>
        /// 更新使用者最後登入時間
        /// </summary>
        Task<bool> UpdateLastLoginAsync(int userId);

        /// <summary>
        /// 取得所有審查人員
        /// </summary>
        Task<List<User>> GetReviewersAsync();

        /// <summary>
        /// 檢查使用者是否存在
        /// </summary>
        Task<bool> UserExistsAsync(string adAccount);
    }

    public class UserSyncService : IUserSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILdapService _ldapService;
        private readonly ILogger<UserSyncService> _logger;
        private readonly ICacheService _cacheService;

        public UserSyncService(
            ApplicationDbContext context,
            ILdapService ldapService,
            ILogger<UserSyncService> logger,
            ICacheService cacheService)
        {
            _context = context;
            _ldapService = ldapService;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<(bool Success, string? Message)> SyncUserAsync(string adAccount, string name, string email, string role)
        {
            if (string.IsNullOrWhiteSpace(adAccount))
            {
                return (false, "帳號不能為空");
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.AdAccount == adAccount);

                if (user == null)
                {
                    // 新增使用者
                    user = new User
                    {
                        AdAccount = adAccount,
                        Name = name,
                        Email = email,
                        Role = role,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    _logger.LogInformation("新增使用者: {AdAccount}, Role={Role}", adAccount, role);
                }
                else
                {
                    // 更新使用者
                    user.Name = name;
                    user.Email = email;
                    user.Role = role;
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Users.Update(user);
                    _logger.LogInformation("更新使用者: {AdAccount}, Role={Role}", adAccount, role);
                }

                await _context.SaveChangesAsync();

                // 清除快取
                _cacheService.Remove(CacheKeys.GetUserCacheKey(adAccount));
                _cacheService.Remove(CacheKeys.ReviewerListKey);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "使用者同步異常: {AdAccount}", adAccount);
                return (false, ex.Message);
            }
        }

        public async Task<User?> GetOrCreateUserAsync(string adAccount)
        {
            if (string.IsNullOrWhiteSpace(adAccount))
            {
                return null;
            }

            // 從快取讀取
            var cacheKey = CacheKeys.GetUserCacheKey(adAccount);
            if (_cacheService.TryGetValue(cacheKey, out User? cachedUser))
            {
                _logger.LogDebug("從快取取得使用者: {AdAccount}", adAccount);
                return cachedUser;
            }

            try
            {
                // 從資料庫查詢
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.AdAccount == adAccount);

                if (user != null)
                {
                    _logger.LogDebug("從資料庫取得使用者: {AdAccount}", adAccount);
                    _cacheService.GetOrCreate(cacheKey, () => user, TimeSpan.FromHours(1));
                    return user;
                }

                // 從 AD 查詢並建立
                var (found, name, email) = await _ldapService.GetUserInfoAsync(adAccount);
                if (found)
                {
                    var isReviewer = await _ldapService.IsReviewerAsync(adAccount);
                    var role = isReviewer ? "reviewer" : "applicant";

                    var (syncSuccess, _) = await SyncUserAsync(adAccount, name ?? adAccount, email ?? $"{adAccount}@company.local", role);
                    
                    if (syncSuccess)
                    {
                        user = await _context.Users
                            .FirstOrDefaultAsync(u => u.AdAccount == adAccount);

                        if (user != null)
                        {
                            _cacheService.GetOrCreate(cacheKey, () => user, TimeSpan.FromHours(1));
                            _logger.LogInformation("新增使用者 (從 AD): {AdAccount}, Role={Role}", 
                                adAccount, role);
                            return user;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取或建立使用者異常: {AdAccount}", adAccount);
            }

            return null;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    // 清除快取以刷新資訊
                    _cacheService.Remove(CacheKeys.GetUserCacheKey(user.AdAccount));

                    _logger.LogDebug("更新使用者最後登入時間: UserId={UserId}", userId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新使用者登入時間異常: UserId={UserId}", userId);
            }

            return false;
        }

        public async Task<List<User>> GetReviewersAsync()
        {
            try
            {
                // 嘗試從快取取得
                if (_cacheService.TryGetValue(CacheKeys.ReviewerListKey, out List<User>? cachedReviewers))
                {
                    _logger.LogDebug("從快取取得審查人員清單");
                    return cachedReviewers ?? new List<User>();
                }

                // 從資料庫查詢
                var reviewers = await _context.Users
                    .Where(u => u.Role == "reviewer" && u.IsActive)
                    .OrderBy(u => u.Name)
                    .ToListAsync();

                // 存入快取 (TTL: 1 小時)
                _cacheService.GetOrCreate(
                    CacheKeys.ReviewerListKey,
                    () => reviewers,
                    TimeSpan.FromHours(1));

                _logger.LogInformation("取得審查人員清單: 數量={Count}", reviewers.Count);
                return reviewers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得審查人員清單異常");
                return new List<User>();
            }
        }

        public async Task<bool> UserExistsAsync(string adAccount)
        {
            if (string.IsNullOrWhiteSpace(adAccount))
            {
                return false;
            }

            try
            {
                return await _context.Users
                    .AnyAsync(u => u.AdAccount == adAccount && u.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查使用者存在狀態異常: {AdAccount}", adAccount);
                return false;
            }
        }
    }
}
