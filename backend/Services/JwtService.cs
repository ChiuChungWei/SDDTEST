using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace ContractReviewScheduler.Services
{
    /// <summary>
    /// JWT Token 簽發和驗證服務
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 簽發 JWT Token
        /// </summary>
        string GenerateToken(int userId, string adAccount, string role, string email);

        /// <summary>
        /// 驗證和解析 JWT Token
        /// </summary>
        (bool IsValid, ClaimsPrincipal? Principal, string? ErrorMessage) ValidateToken(string token);

        /// <summary>
        /// 從 Token 提取聲明
        /// </summary>
        Dictionary<string, string> ExtractClaims(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly ILogger<JwtService> _logger;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpirationMinutes;

        public JwtService(ILogger<JwtService> logger, IConfiguration configuration)
        {
            _logger = logger;

            // 從配置讀取 JWT 設定
            _jwtKey = configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT Key 未在配置中設定");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? "ContractReviewScheduler";
            _jwtAudience = configuration["Jwt:Audience"] ?? "ContractReviewSchedulerClient";
            _jwtExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

            _logger.LogInformation("JWT 服務初始化: Issuer={Issuer}, Audience={Audience}, ExpireMinutes={Minutes}",
                _jwtIssuer, _jwtAudience, _jwtExpirationMinutes);
        }

        public string GenerateToken(int userId, string adAccount, string role, string email)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtKey);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, adAccount),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("ad_account", adAccount),
                    new Claim("issued_at", DateTime.UtcNow.ToString("O"))
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                    Issuer = _jwtIssuer,
                    Audience = _jwtAudience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var encodedToken = tokenHandler.WriteToken(token);

                _logger.LogInformation("JWT Token 已簽發: UserId={UserId}, Role={Role}, Expire={Expire}",
                    userId, role, tokenDescriptor.Expires);

                return encodedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT Token 簽發異常: UserId={UserId}", userId);
                throw;
            }
        }

        public (bool IsValid, ClaimsPrincipal? Principal, string? ErrorMessage) ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, null, "Token 不能為空");
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtKey);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogDebug("JWT Token 驗證成功: UserId={UserId}", userId);

                return (true, principal, null);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "JWT Token 已過期");
                return (false, null, "Token 已過期");
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "JWT Token 簽名無效");
                return (false, null, "Token 簽名無效");
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT Token 驗證失敗");
                return (false, null, "Token 無效");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT Token 驗證異常");
                return (false, null, "Token 驗證異常");
            }
        }

        public Dictionary<string, string> ExtractClaims(string token)
        {
            var claims = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(token))
            {
                return claims;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken != null)
                {
                    foreach (var claim in jwtToken.Claims)
                    {
                        claims[claim.Type] = claim.Value;
                    }

                    _logger.LogDebug("從 Token 提取聲明: 數量={ClaimCount}", claims.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提取 Token 聲明異常");
            }

            return claims;
        }
    }
}
