using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CarbonWise.BuildingBlocks.Domain.Users;

namespace CarbonWise.BuildingBlocks.Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username),
                new Claim("isInInstitution", user.IsInInstitution.ToString()),
                new Claim("isStudent", user.IsStudent.ToString()),
                new Claim("isAcademicPersonal", user.IsAcademicPersonal.ToString()),
                new Claim("isAdministrativeStaff", user.IsAdministrativeStaff.ToString()),
                new Claim("uniqueId", user.UniqueId ?? ""),
                new Claim("sustainabilityPoint", user.SustainabilityPoint?.ToString() ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, 
                    RequireExpirationTime = true,
                    RequireSignedTokens = true
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenException)
            {
                return null; // Token geçersiz
            }
            catch (Exception)
            {
                return null; // Genel hata
            }
        }

        public UserInfo? GetUserFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            try
            {
                var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return null;

                var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, out var role))
                    return null;

                var sustainabilityPointStr = principal.FindFirst("sustainabilityPoint")?.Value;
                int? sustainabilityPoint = null;
                if (!string.IsNullOrEmpty(sustainabilityPointStr) && int.TryParse(sustainabilityPointStr, out var point))
                {
                    sustainabilityPoint = point;
                }

                return new UserInfo
                {
                    UserId = userId,
                    Username = principal.FindFirst("username")?.Value ?? "",
                    Email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? "",
                    Role = role,
                    IsInInstitution = bool.TryParse(principal.FindFirst("isInInstitution")?.Value, out var isInInst) && isInInst,
                    IsStudent = bool.TryParse(principal.FindFirst("isStudent")?.Value, out var isStudent) && isStudent,
                    IsAcademicPersonal = bool.TryParse(principal.FindFirst("isAcademicPersonal")?.Value, out var isAcademic) && isAcademic,
                    IsAdministrativeStaff = bool.TryParse(principal.FindFirst("isAdministrativeStaff")?.Value, out var isAdmin) && isAdmin,
                    SustainabilityPoint = sustainabilityPoint
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsTokenValid(string token)
        {
            return ValidateToken(token) != null;
        }
    }
}
