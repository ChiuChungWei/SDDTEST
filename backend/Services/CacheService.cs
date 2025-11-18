using System;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace ContractReviewScheduler.Services
{
    /// <summary>
    /// 快取服務 - 使用 IMemoryCache 快取 AD 查詢結果
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 從快取取得值，如果不存在則執行工廠方法
        /// </summary>
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);

        /// <summary>
        /// 非同步版本 - 從快取取得值
        /// </summary>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

        /// <summary>
        /// 移除快取項目
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// 清空所有快取
        /// </summary>
        void Clear();

        /// <summary>
        /// 取得快取中的值（如果存在）
        /// </summary>
        bool TryGetValue<T>(string key, out T? value);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly TimeSpan _defaultExpiration;
        private readonly HashSet<string> _cacheKeys;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _defaultExpiration = TimeSpan.FromHours(1); // 預設 1 小時
            _cacheKeys = new HashSet<string>();
        }

        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("快取鍵不能為空", nameof(key));
            }

            if (_memoryCache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug("快取命中: {CacheKey}", key);
                return cachedValue!;
            }

            try
            {
                var value = factory();
                
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };

                _memoryCache.Set(key, value, cacheOptions);
                _cacheKeys.Add(key);

                _logger.LogDebug("快取設定: {CacheKey}, 過期時間={Expiration}", 
                    key, expiration ?? _defaultExpiration);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "快取操作異常: {CacheKey}", key);
                throw;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("快取鍵不能為空", nameof(key));
            }

            if (_memoryCache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug("快取命中 (非同步): {CacheKey}", key);
                return cachedValue!;
            }

            try
            {
                var value = await factory();
                
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };

                _memoryCache.Set(key, value, cacheOptions);
                _cacheKeys.Add(key);

                _logger.LogDebug("快取設定 (非同步): {CacheKey}, 過期時間={Expiration}", 
                    key, expiration ?? _defaultExpiration);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "非同步快取操作異常: {CacheKey}", key);
                throw;
            }
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            try
            {
                _memoryCache.Remove(key);
                _cacheKeys.Remove(key);
                _logger.LogDebug("快取已移除: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除快取異常: {CacheKey}", key);
            }
        }

        public void Clear()
        {
            try
            {
                foreach (var key in _cacheKeys.ToList())
                {
                    _memoryCache.Remove(key);
                }
                _cacheKeys.Clear();
                _logger.LogInformation("快取已清空");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空快取異常");
            }
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = default;
                return false;
            }

            try
            {
                return _memoryCache.TryGetValue(key, out value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得快取值異常: {CacheKey}", key);
                value = default;
                return false;
            }
        }
    }

    /// <summary>
    /// 快取鍵常數
    /// </summary>
    public static class CacheKeys
    {
        public const string UserPrefix = "user_";
        public const string ReviewerListKey = "reviewers_list";
        public const string AppointmentPrefix = "appointment_";
        public const string LeaveSchedulePrefix = "leave_";

        public static string GetUserCacheKey(string adAccount) => $"{UserPrefix}{adAccount.ToLower()}";
        public static string GetAppointmentCacheKey(int appointmentId) => $"{AppointmentPrefix}{appointmentId}";
        public static string GetLeaveScheduleCacheKey(int reviewerId) => $"{LeaveSchedulePrefix}{reviewerId}";
    }
}
