namespace NetworkInventory.Api.Models;

/// <summary>
/// Сетевое устройство в инвентаре.
/// Содержит поля аудита (LastModifiedDate, LastModifiedByUserId) для реализации
/// принципа Accountability (подотчётность) — каждое изменение привязано к сотруднику и времени.
/// </summary>
public class Device
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Тип устройства: Router, Switch, Firewall и т.д.</summary>
    public string Type { get; set; } = string.Empty;

    public string HardwareModel { get; set; } = string.Empty;

    /// <summary>
    /// Роль устройства в топологии сети (например: "VPN Router", "Коммутатор отдела продаж").
    /// </summary>
    public string NetworkRole { get; set; } = string.Empty;

    /// <summary>Дата и время последнего изменения записи (автоматически в SaveChangesAsync).</summary>
    public DateTime LastModifiedDate { get; set; }

    /// <summary>ID пользователя, внёсшего последнее изменение.</summary>
    public int LastModifiedByUserId { get; set; }

    /// <summary>Навигационное свойство — сотрудник, изменивший устройство.</summary>
    public User LastModifiedBy { get; set; } = null!;

    /// <summary>Сетевые интерфейсы устройства (связь 1-ко-многим).</summary>
    public ICollection<NetworkInterface> NetworkInterfaces { get; set; } = new List<NetworkInterface>();
}
