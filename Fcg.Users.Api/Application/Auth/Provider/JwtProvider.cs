using Application.Auth.Provider;
using Domain.Entidades;
using Domain.Enum;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public sealed class JwtProvider(IConfiguration cfg) : IJwtProvider
{
    public (string token, DateTime expires) Create(Usuario user, TimeSpan? ttl = null)
    {
        var keyBytes = Encoding.UTF8.GetBytes(cfg["Jwt:Key"] ?? "dev-secret-key-change");
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var exp = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(60));

        var role = user.NivelAcesso == NivelAcessoEnum.Administrador ? "Admin" : "User";

        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.Address),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

        var token = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"],
            audience: cfg["Jwt:Audience"],
            claims: claims,
            expires: exp,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), exp);
    }
}