using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GotFired.Api.Features.Auth
{
    public class TokenService
    {
        public const string NameClaimType = "name";
        public const string RoleClaimType = "role";

        private readonly JwtOptions _options;
        private readonly SigningCredentials _credentials;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            _credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)), SecurityAlgorithms.HmacSha256);
        }

        public TimeSpan Lifetime => _options.Lifetime;

        public string CreateToken(string userName, IEnumerable<string> roles)
        {
            var claims = new List<Claim> { new(NameClaimType, userName) };
            claims.AddRange(roles.Select(r => new Claim(RoleClaimType, r)));

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(_options.Lifetime),
                signingCredentials: _credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
