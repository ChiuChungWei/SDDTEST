using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Serilog;

namespace ContractReviewScheduler.Services
{
    /// <summary>
    /// LDAP 認證服務 - 整合 Active Directory
    /// </summary>
    public interface ILdapService
    {
        /// <summary>
        /// 驗證使用者 AD 帳號和密碼
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> AuthenticateAsync(string adAccount, string password);

        /// <summary>
        /// 取得 AD 使用者資訊
        /// </summary>
        Task<(bool Found, string? Name, string? Email)> GetUserInfoAsync(string adAccount);

        /// <summary>
        /// 搜尋 AD 中的使用者
        /// </summary>
        Task<List<(string AdAccount, string Name, string Email)>> SearchUsersAsync(string searchTerm);

        /// <summary>
        /// 檢查使用者是否為審查人員 (檢查 AD 群組)
        /// </summary>
        Task<bool> IsReviewerAsync(string adAccount);
    }

    public class LdapService : ILdapService
    {
        private readonly ILogger<LdapService> _logger;
        private readonly string _ldapPath;
        private readonly string _ldapDomain;
        private readonly string _reviewerGroupName;

        public LdapService(ILogger<LdapService> logger, IConfiguration configuration)
        {
            _logger = logger;
            
            // 從配置讀取 LDAP 設定
            _ldapPath = configuration["Ldap:Path"] ?? "LDAP://company.local";
            _ldapDomain = configuration["Ldap:Domain"] ?? "company.local";
            _reviewerGroupName = configuration["Ldap:ReviewerGroup"] ?? "ReviewersGroup";

            _logger.LogInformation("LDAP 服務初始化: Path={LdapPath}, Domain={Domain}", _ldapPath, _ldapDomain);
        }

        public async Task<(bool IsValid, string? ErrorMessage)> AuthenticateAsync(string adAccount, string password)
        {
            if (string.IsNullOrWhiteSpace(adAccount) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "帳號或密碼不能為空");
            }

            try
            {
                using (var principalContext = new PrincipalContext(ContextType.Domain, _ldapDomain))
                {
                    var isValid = principalContext.ValidateCredentials(adAccount, password);
                    
                    if (isValid)
                    {
                        _logger.LogInformation("使用者 {AdAccount} 認證成功", adAccount);
                        return (true, null);
                    }
                    else
                    {
                        _logger.LogWarning("使用者 {AdAccount} 認證失敗 - 密碼不正確", adAccount);
                        return (false, "帳號或密碼不正確");
                    }
                }
            }
            catch (PasswordException ex)
            {
                _logger.LogError(ex, "LDAP 密碼驗證異常: {AdAccount}", adAccount);
                return (false, "認證服務暫時不可用");
            }
            catch (PrincipalServerDownException ex)
            {
                _logger.LogError(ex, "LDAP 伺服器離線");
                return (false, "認證伺服器暫時不可用");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LDAP 認證發生異常: {AdAccount}", adAccount);
                return (false, "認證發生異常");
            }
        }

        public async Task<(bool Found, string? Name, string? Email)> GetUserInfoAsync(string adAccount)
        {
            if (string.IsNullOrWhiteSpace(adAccount))
            {
                return (false, null, null);
            }

            try
            {
                using (var principalContext = new PrincipalContext(ContextType.Domain, _ldapDomain))
                {
                    var userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, adAccount);
                    
                    if (userPrincipal != null)
                    {
                        var name = userPrincipal.DisplayName ?? userPrincipal.Name ?? adAccount;
                        var email = userPrincipal.EmailAddress ?? $"{adAccount}@{_ldapDomain}";
                        
                        _logger.LogInformation("找到 AD 使用者: {AdAccount}, Name={Name}, Email={Email}", 
                            adAccount, name, email);
                        
                        return (true, name, email);
                    }
                    else
                    {
                        _logger.LogWarning("AD 中未找到使用者: {AdAccount}", adAccount);
                        return (false, null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋 AD 使用者異常: {AdAccount}", adAccount);
                return (false, null, null);
            }
        }

        public async Task<List<(string AdAccount, string Name, string Email)>> SearchUsersAsync(string searchTerm)
        {
            var results = new List<(string, string, string)>();

            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            {
                return results;
            }

            try
            {
                using (var principalContext = new PrincipalContext(ContextType.Domain, _ldapDomain))
                {
                    using (var searcher = new PrincipalSearcher())
                    {
                        var userPrincipal = new UserPrincipal(principalContext)
                        {
                            DisplayName = $"*{searchTerm}*"
                        };

                        searcher.QueryFilter = userPrincipal;
                        var principalEnumerator = searcher.FindAll();

                        int count = 0;
                        const int maxResults = 20;

                        foreach (var principal in principalEnumerator)
                        {
                            if (count >= maxResults) break;

                            if (principal is UserPrincipal up)
                            {
                                var adAccount = up.SamAccountName;
                                var name = up.DisplayName ?? up.Name ?? adAccount;
                                var email = up.EmailAddress ?? $"{adAccount}@{_ldapDomain}";

                                results.Add((adAccount, name, email));
                                count++;
                            }
                        }

                        _logger.LogInformation("AD 使用者搜尋完成: 搜尋詞={SearchTerm}, 找到={Count}", 
                            searchTerm, count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋 AD 使用者異常: {SearchTerm}", searchTerm);
            }

            return results;
        }

        public async Task<bool> IsReviewerAsync(string adAccount)
        {
            if (string.IsNullOrWhiteSpace(adAccount))
            {
                return false;
            }

            try
            {
                using (var principalContext = new PrincipalContext(ContextType.Domain, _ldapDomain))
                {
                    var userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, adAccount);
                    
                    if (userPrincipal != null)
                    {
                        var groups = userPrincipal.GetGroups();
                        var isReviewer = groups.Any(g => g.Name.Equals(_reviewerGroupName, StringComparison.OrdinalIgnoreCase));
                        
                        _logger.LogInformation("審查人員檢查: {AdAccount}, IsReviewer={IsReviewer}", 
                            adAccount, isReviewer);
                        
                        return isReviewer;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查審查人員狀態異常: {AdAccount}", adAccount);
            }

            return false;
        }
    }
}
