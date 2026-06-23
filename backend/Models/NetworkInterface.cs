namespace NetworkInventory.Api.Models;

/// <summary>
/// Сетевой интерфейс устройства (порт).
/// Связан с Device отношением «один-ко-многим» — одно устройство может иметь несколько интерфейсов.
/// </summary>
public class NetworkInterface
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    /// <summary>Имя порта, например: eth0, Gi0/1, Fa0/24.</summary>
    public string PortName { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string MacAddress { get; set; } = string.Empty;

    /// <summary>Активен ли интерфейс в данный момент.</summary>
    public bool IsActive { get; set; }

    public Device Device { get; set; } = null!;
}
