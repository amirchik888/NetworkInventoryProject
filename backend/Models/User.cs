namespace NetworkInventory.Api.Models;

/// <summary>
/// Модель пользователя системы.
/// Используется для аутентификации (JWT) и RBAC — разграничения прав доступа.
/// Пароль хранится только в виде хэша (BCrypt), что защищает от утечки при компрометации БД.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>Уникальное имя для входа в систему.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Хэш пароля (BCrypt). Исходный пароль никогда не сохраняется.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя для RBAC: "Admin" — полный доступ, "User" — только чтение.
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>Устройства, последнее изменение которых выполнил данный пользователь (аудит).</summary>
    public ICollection<Device> ModifiedDevices { get; set; } = new List<Device>();
}
