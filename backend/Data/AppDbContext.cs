using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NetworkInventory.Api.Models;

namespace NetworkInventory.Api.Data;

/// <summary>
/// Контекст Entity Framework Core для SQLite.
/// Все запросы к БД выполняются через LINQ — параметризованные запросы EF защищают от SQL-инъекций (SQLi).
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<NetworkInterface> NetworkInterfaces => Set<NetworkInterface>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50);
            entity.Property(u => u.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.Property(d => d.Name).HasMaxLength(100);
            entity.Property(d => d.Type).HasMaxLength(50);
            entity.Property(d => d.HardwareModel).HasMaxLength(100);
            entity.Property(d => d.NetworkRole).HasMaxLength(200);

            entity.HasOne(d => d.LastModifiedBy)
                .WithMany(u => u.ModifiedDevices)
                .HasForeignKey(d => d.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NetworkInterface>(entity =>
        {
            entity.Property(n => n.PortName).HasMaxLength(20);
            entity.Property(n => n.IpAddress).HasMaxLength(15);
            entity.Property(n => n.MacAddress).HasMaxLength(17);

            entity.HasOne(n => n.Device)
                .WithMany(d => d.NetworkInterfaces)
                .HasForeignKey(n => n.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Переопределение SaveChangesAsync для автоматического аудита изменений устройств.
    /// Реализует принцип Accountability (подотчётность) из CIA-триады расширенной модели безопасности:
    /// каждое изменение Device фиксирует WHO (кто) и WHEN (когда).
    /// ID пользователя берётся из JWT-claims текущего HTTP-запроса через IHttpContextAccessor.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Device>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.LastModifiedDate = now;

                if (userId.HasValue)
                {
                    entry.Entity.LastModifiedByUserId = userId.Value;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Извлекает ID пользователя из claim "sub" JWT-токена текущего запроса.</summary>
    private int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor?.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor?.HttpContext?.User.FindFirst("sub")?.Value;

        return int.TryParse(userIdClaim, out var id) ? id : null;
    }
}
