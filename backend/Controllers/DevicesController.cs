using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkInventory.Api.Data;
using NetworkInventory.Api.DTOs;
using NetworkInventory.Api.Models;
using NetworkInventory.Api.Services;

namespace NetworkInventory.Api.Controllers;

/// <summary>
/// CRUD-операции над сетевым оборудованием.
/// RBAC: Admin — полный доступ; User — только чтение.
/// Все запросы к БД — через LINQ/EF Core (защита от SQL-инъекций).
/// Входные данные принимаются только через DTO (защита от Over-Posting).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(AppDbContext context, ILogger<DevicesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Список всех устройств для Dashboard.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceListItemDto>>> GetAll()
    {
        var devices = await _context.Devices
            .Include(d => d.LastModifiedBy)
            .Include(d => d.NetworkInterfaces)
            .OrderBy(d => d.Name)
            .Select(d => new DeviceListItemDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type,
                NetworkRole = d.NetworkRole,
                LastModifiedDate = d.LastModifiedDate,
                LastModifiedByUsername = d.LastModifiedBy.Username,
                InterfaceCount = d.NetworkInterfaces.Count
            })
            .ToListAsync();

        return Ok(devices);
    }

    /// <summary>Детальная карточка устройства с интерфейсами и блоком аудита.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviceResponseDto>> GetById(int id)
    {
        var device = await _context.Devices
            .Include(d => d.LastModifiedBy)
            .Include(d => d.NetworkInterfaces)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device is null)
            return NotFound(new { message = $"Устройство с ID {id} не найдено" });

        return Ok(MapToResponse(device));
    }

    /// <summary>Создание устройства — только Admin (RBAC).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DeviceResponseDto>> Create([FromBody] DeviceCreateDto dto)
    {
        var device = new Device
        {
            Name = dto.Name,
            Type = dto.Type,
            HardwareModel = dto.HardwareModel,
            NetworkRole = dto.NetworkRole,
            NetworkInterfaces = dto.NetworkInterfaces.Select(i => new NetworkInterface
            {
                PortName = i.PortName,
                IpAddress = i.IpAddress,
                MacAddress = i.MacAddress,
                IsActive = i.IsActive
            }).ToList()
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        await _context.Entry(device).Reference(d => d.LastModifiedBy).LoadAsync();
        await _context.Entry(device).Collection(d => d.NetworkInterfaces).LoadAsync();

        _logger.LogInformation("Создано устройство {DeviceName} (ID={Id}) пользователем {User}",
            device.Name, device.Id, User.Identity?.Name);

        return CreatedAtAction(nameof(GetById), new { id = device.Id }, MapToResponse(device));
    }

    /// <summary>Обновление устройства — только Admin (RBAC).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DeviceResponseDto>> Update(int id, [FromBody] DeviceUpdateDto dto)
    {
        var device = await _context.Devices
            .Include(d => d.NetworkInterfaces)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device is null)
            return NotFound(new { message = $"Устройство с ID {id} не найдено" });

        device.Name = dto.Name;
        device.Type = dto.Type;
        device.HardwareModel = dto.HardwareModel;
        device.NetworkRole = dto.NetworkRole;

        // Обновление интерфейсов: удаляем старые, добавляем новые из DTO
        _context.NetworkInterfaces.RemoveRange(device.NetworkInterfaces);
        device.NetworkInterfaces = dto.NetworkInterfaces.Select(i => new NetworkInterface
        {
            PortName = i.PortName,
            IpAddress = i.IpAddress,
            MacAddress = i.MacAddress,
            IsActive = i.IsActive
        }).ToList();

        await _context.SaveChangesAsync();

        await _context.Entry(device).Reference(d => d.LastModifiedBy).LoadAsync();

        _logger.LogInformation("Обновлено устройство {DeviceName} (ID={Id}) пользователем {User}",
            device.Name, device.Id, User.Identity?.Name);

        return Ok(MapToResponse(device));
    }

    /// <summary>Удаление устройства — только Admin (RBAC).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device is null)
            return NotFound(new { message = $"Устройство с ID {id} не найдено" });

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Удалено устройство {DeviceName} (ID={Id}) пользователем {User}",
            device.Name, device.Id, User.Identity?.Name);

        return NoContent();
    }

    private static DeviceResponseDto MapToResponse(Device device) => new()
    {
        Id = device.Id,
        Name = device.Name,
        Type = device.Type,
        HardwareModel = device.HardwareModel,
        NetworkRole = device.NetworkRole,
        LastModifiedDate = device.LastModifiedDate,
        LastModifiedByUsername = device.LastModifiedBy?.Username ?? "—",
        NetworkInterfaces = device.NetworkInterfaces.Select(i => new NetworkInterfaceResponseDto
        {
            Id = i.Id,
            PortName = i.PortName,
            IpAddress = i.IpAddress,
            MacAddress = i.MacAddress,
            IsActive = i.IsActive
        }).ToList()
    };
}
