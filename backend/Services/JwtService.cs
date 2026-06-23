using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NetworkInventory.Api.Models;

namespace NetworkInventory.Api.Services;

/// <summary>
/// Сервис генерации JWT-токенов.
/// Токен подписывается симметричным ключом (HMAC-SHA256) — целостность и подлинность гарантируются криптографически.
/// </summary>
public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("sub", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>Константы ролей для RBAC — централизованное определение предотвращает опечатки.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

/// <summary>Имя HttpOnly cookie, в которой хранится JWT (защита от XSS).</summary>
public static class AuthCookie
{
    public const string Name = "access_token";
}
