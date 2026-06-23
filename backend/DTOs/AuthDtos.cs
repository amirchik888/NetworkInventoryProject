using System.ComponentModel.DataAnnotations;

namespace NetworkInventory.Api.DTOs;

/// <summary>
/// DTO для входа в систему.
/// Data Annotations ограничивают ввод и защищают от передачи лишних полей (Over-Posting).
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Ответ при успешной аутентификации. JWT НЕ возвращается в теле — только в HttpOnly cookie.</summary>
public class LoginResponseDto
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = "Успешный вход. Токен установлен в защищённую cookie.";
}

/// <summary>Информация о текущем авторизованном пользователе.</summary>
public class UserInfoDto
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
