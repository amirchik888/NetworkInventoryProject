using System.ComponentModel.DataAnnotations;

namespace NetworkInventory.Api.DTOs;

/// <summary>
/// DTO для создания устройства.
/// Принимаем только разрешённые поля — клиент не может передать Id, LastModifiedDate и т.д. (защита от Over-Posting).
/// </summary>
public class DeviceCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string HardwareModel { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string NetworkRole { get; set; } = string.Empty;

    public List<NetworkInterfaceDto> NetworkInterfaces { get; set; } = new();
}

/// <summary>DTO для обновления устройства — те же ограничения, что и при создании.</summary>
public class DeviceUpdateDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string HardwareModel { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string NetworkRole { get; set; } = string.Empty;

    public List<NetworkInterfaceDto> NetworkInterfaces { get; set; } = new();
}

/// <summary>
/// DTO сетевого интерфейса для приёма данных от клиента.
/// Валидация IP/MAC снижает риск некорректных и вредоносных данных.
/// </summary>
public class NetworkInterfaceDto
{
    [Required]
    [StringLength(20)]
    public string PortName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "Некорректный формат IP-адреса")]
    public string IpAddress { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$", ErrorMessage = "Некорректный формат MAC-адреса")]
    public string MacAddress { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>DTO для отдачи устройства клиенту (включая блок аудита).</summary>
public class DeviceResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string HardwareModel { get; set; } = string.Empty;
    public string NetworkRole { get; set; } = string.Empty;
    public DateTime LastModifiedDate { get; set; }
    public string LastModifiedByUsername { get; set; } = string.Empty;
    public List<NetworkInterfaceResponseDto> NetworkInterfaces { get; set; } = new();
}

public class NetworkInterfaceResponseDto
{
    public int Id { get; set; }
    public string PortName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>Краткая информация для списка на Dashboard.</summary>
public class DeviceListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string NetworkRole { get; set; } = string.Empty;
    public DateTime LastModifiedDate { get; set; }
    public string LastModifiedByUsername { get; set; } = string.Empty;
    public int InterfaceCount { get; set; }
}
