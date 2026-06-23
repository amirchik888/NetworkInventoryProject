using NetworkInventory.Api.Models;

namespace NetworkInventory.Api.Data;

/// <summary>
/// Инициализация базы данных начальными данными (Seed).
/// Создаёт тестовых пользователей и 5 сетевых устройств с интерфейсами для демонстрации системы.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (context.Users.Any())
            return;

        // --- Пользователи (RBAC: Admin и обычный User) ---
        var admin = new User
        {
            Username = "admin",
            // Пароль: Admin123! — хэшируется BCrypt (защита от хранения паролей в открытом виде)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin"
        };

        var user = new User
        {
            Username = "operator",
            // Пароль: Operator123!
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operator123!"),
            Role = "User"
        };

        context.Users.AddRange(admin, user);
        await context.SaveChangesAsync();

        // --- 5 сетевых устройств с интерфейсами ---
        var devices = new List<Device>
        {
            CreateDevice(admin.Id, "GW-CORE-01", "Router", "Cisco ISR 4331",
                "Главный шлюз организации — маршрутизация между VLAN и Internet",
                new[] { ("Gi0/0/0", "10.0.0.1", "00:1A:2B:3C:4D:01", true), ("Gi0/0/1", "192.168.1.1", "00:1A:2B:3C:4D:02", true), ("Gi0/0/2", "172.16.0.1", "00:1A:2B:3C:4D:03", false) }),

            CreateDevice(admin.Id, "SW-FLOOR2-AGG", "Switch", "Cisco Catalyst 2960-X",
                "Агрегация 2 этажа — объединение коммутаторов доступа",
                new[] { ("Gi1/0/1", "10.0.2.10", "00:2B:3C:4D:5E:01", true), ("Gi1/0/2", "10.0.2.11", "00:2B:3C:4D:5E:02", true), ("Gi1/0/3", "10.0.2.12", "00:2B:3C:4D:5E:03", true), ("Gi1/0/4", "10.0.2.13", "00:2B:3C:4D:5E:04", false) }),

            CreateDevice(user.Id, "FW-VPN-01", "Firewall", "FortiGate 60F",
                "Используется для VPN-туннеля с филиалом — IPsec Site-to-Site",
                new[] { ("port1", "203.0.113.10", "00:3C:4D:5E:6F:01", true), ("port2", "10.0.5.1", "00:3C:4D:5E:6F:02", true) }),

            CreateDevice(user.Id, "SW-SALES-01", "Switch", "HP ProCurve 2530",
                "Коммутатор для отдела продаж — VLAN 20",
                new[] { ("1", "10.0.20.2", "00:4D:5E:6F:70:01", true), ("2", "10.0.20.3", "00:4D:5E:6F:70:02", true), ("3", "10.0.20.4", "00:4D:5E:6F:70:03", false) }),

            CreateDevice(admin.Id, "AP-WIFI-HQ", "Access Point", "Ubiquiti UniFi U6-Pro",
                "Точка доступа Wi-Fi в главном офисе — SSID Corp-WiFi",
                new[] { ("eth0", "10.0.30.50", "00:5E:6F:70:81:01", true), ("wlan0", "10.0.30.51", "00:5E:6F:70:81:02", true) })
        };

        context.Devices.AddRange(devices);
        await context.SaveChangesAsync();
    }

    private static Device CreateDevice(
        int modifiedByUserId,
        string name,
        string type,
        string model,
        string networkRole,
        (string port, string ip, string mac, bool active)[] interfaces)
    {
        var device = new Device
        {
            Name = name,
            Type = type,
            HardwareModel = model,
            NetworkRole = networkRole,
            LastModifiedDate = DateTime.UtcNow,
            LastModifiedByUserId = modifiedByUserId,
            NetworkInterfaces = interfaces.Select(i => new NetworkInterface
            {
                PortName = i.port,
                IpAddress = i.ip,
                MacAddress = i.mac,
                IsActive = i.active
            }).ToList()
        };

        return device;
    }
}
