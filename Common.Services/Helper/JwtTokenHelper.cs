using Common.Services.ViewModels;

using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Common.Services.Helper
{
    public class JwtTokenHelper
    {
        public string GenerateToken(
             string userId,
             string email,
             string? organizationId,
              string role,
             JwtSettings jwtSettings)
        {
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
                {
                    new Claim("user_id", userId),
                    new Claim("email", email),
                    new Claim(ClaimTypes.Role, role)
                };

            // Only tenant users have organization
            if (!string.Equals(role, "Super Admin", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim("organization_id", organizationId!));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                 new SymmetricSecurityKey(key),
                 SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
