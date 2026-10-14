using EnergyGuard.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnergyGuard.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<EnergyReading> EnergyReadings => Set<EnergyReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<EnergyReport> Reports => Set<EnergyReport>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Device>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SerialNumber).IsUnique();
            e.Property(x => x.PowerLimitKwh).HasPrecision(18, 4);
        });

        builder.Entity<EnergyReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.ReadingDate });
            e.Property(x => x.KilowattHour).HasPrecision(18, 4);
            e.HasOne(x => x.Device)
             .WithMany(d => d.Readings)
             .HasForeignKey(x => x.DeviceId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Alert>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.IsResolved });
        });

        builder.Entity<EnergyReport>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotalKwh).HasPrecision(18, 4);
            e.Property(x => x.AverageKwh).HasPrecision(18, 4);
            e.Property(x => x.PeakKwh).HasPrecision(18, 4);
            e.Property(x => x.CarbonFootprintKg).HasPrecision(18, 4);
        });
    }
}
