using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkInventory.Api.Data;
using NetworkInventory.Api.DTOs;
using NetworkInventory.Api.Services;

namespace NetworkInventory.Api.Controllers;

/// <summary>
/// Контроллер аутентификации.
/// JWT-токен НЕ возвращается в JSON-ответе — устанавливается в HttpOnly cookie.
/// Это защищает токен от кражи через XSS-атаки: JavaScript на странице не может прочитать HttpOnly cookie.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, JwtService jwtService, ILogger<AuthController> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>Вход в систему. Устанавливает JWT в HttpOnly, Secure, SameSite=Strict cookie.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        // Запрос к БД через LINQ (параметризованный — защита от SQLi)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Неудачная попытка входа для пользователя {Username} с IP {IP}",
                dto.Username, HttpContext.Connection.RemoteIpAddress);

            return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
        }

        var token = _jwtService.GenerateToken(user);

        // КРИТИЧНО: токен в HttpOnly cookie, а НЕ в теле ответа
        // HttpOnly — недоступен для document.cookie (XSS)
        // Secure — передаётся только по HTTPS (в Production)
        // SameSite=Strict — защита от CSRF через сторонние сайты
        Response.Cookies.Append(AuthCookie.Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8),
            Path = "/"
        });

        _logger.LogInformation("Пользователь {Username} ({Role}) успешно вошёл в систему",
            user.Username, user.Role);

        return Ok(new LoginResponseDto
        {
            Username = user.Username,
            Role = user.Role,
            Message = "Успешный вход. Токен установлен в защищённую cookie."
        });
    }

    /// <summary>Выход — удаление JWT cookie.</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookie.Name, new CookieOptions { Path = "/" });

        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("Пользователь {Username} вышел из системы", username);

        return Ok(new { message = "Выход выполнен успешно" });
    }

    /// <summary>Проверка текущей сессии и получение данных пользователя.</summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfoDto> Me()
    {
        return Ok(new UserInfoDto
        {
            Username = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Role = User.FindFirst(ClaimTypes.Role)?.Value ?? ""
        });
    }
}
