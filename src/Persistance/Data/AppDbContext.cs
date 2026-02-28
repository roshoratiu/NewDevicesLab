using Microsoft.EntityFrameworkCore;
using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Persistance.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices => Set<Device>();

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserGroup> UserGroups => Set<UserGroup>();

    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(item => item.CreatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.RadboudEmail)
                .HasMaxLength(256)
                .IsRequired();
            entity.HasIndex(item => item.RadboudEmail)
                .IsUnique();
            entity.Property(item => item.Username)
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(item => item.Username)
                .IsUnique();
            entity.Property(item => item.PasswordHash)
                .HasMaxLength(512)
                .IsRequired();
            entity.Property(item => item.StudentNumber)
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(item => item.EnrollmentDate)
                .HasColumnType("date")
                .IsRequired();
            entity.Property(item => item.IsActive)
                .IsRequired();
            entity.Property(item => item.CreatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name)
                .HasMaxLength(120)
                .IsRequired();
            entity.HasIndex(item => item.Name)
                .IsUnique();
            entity.Property(item => item.Description)
                .HasMaxLength(500)
                .IsRequired();
            entity.Property(item => item.IsSystem)
                .IsRequired();
            entity.Property(item => item.CreatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code)
                .HasMaxLength(120)
                .IsRequired();
            entity.HasIndex(item => item.Code)
                .IsUnique();
            entity.Property(item => item.Name)
                .HasMaxLength(120)
                .IsRequired();
            entity.Property(item => item.Description)
                .HasMaxLength(500)
                .IsRequired();
            entity.Property(item => item.IsSystem)
                .IsRequired();
            entity.Property(item => item.CreatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(item => new { item.UserId, item.GroupId });
            entity.Property(item => item.JoinedAtUtc)
                .IsRequired();
            entity.HasOne(item => item.User)
                .WithMany(item => item.UserGroups)
                .HasForeignKey(item => item.UserId);
            entity.HasOne(item => item.Group)
                .WithMany(item => item.UserGroups)
                .HasForeignKey(item => item.GroupId);
        });

        modelBuilder.Entity<GroupPermission>(entity =>
        {
            entity.HasKey(item => new { item.GroupId, item.PermissionId });
            entity.Property(item => item.GrantedAtUtc)
                .IsRequired();
            entity.HasOne(item => item.Group)
                .WithMany(item => item.GroupPermissions)
                .HasForeignKey(item => item.GroupId);
            entity.HasOne(item => item.Permission)
                .WithMany(item => item.GroupPermissions)
                .HasForeignKey(item => item.PermissionId);
        });
    }
}
