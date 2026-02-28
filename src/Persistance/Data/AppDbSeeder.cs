using Microsoft.EntityFrameworkCore;
using NewDevicesLab.Application.Security;
using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Persistance.Data;

public static class AppDbSeeder
{
    private static readonly PermissionDefinition[] PermissionDefinitions =
    [
        new("devices.read", "Read devices", "Allows viewing the device catalog."),
        new("devices.create", "Create devices", "Allows registering new devices."),
        new("devices.update", "Update devices", "Allows editing existing devices."),
        new("devices.delete", "Delete devices", "Allows removing devices."),
        new("admin.access", "Access admin console", "Allows opening the protected admin interface."),
        new("admin.users.manage", "Manage users", "Allows creating users and editing memberships."),
        new("admin.groups.manage", "Manage groups", "Allows editing groups and their permissions."),
        new("projects.create", "Create projects", "Allows starting a new student project."),
        new("projects.view.own", "View own projects", "Allows viewing projects the user created or joined."),
        new("projects.view.group", "View group projects", "Allows viewing projects across student groups."),
        new("projects.status.manage", "Manage project statuses", "Allows updating project lifecycle states."),
        new("ordersheets.create", "Create order sheets", "Allows opening an order sheet."),
        new("ordersheets.view.own", "View own order sheets", "Allows viewing order sheets created by the user."),
        new("ordersheets.view.group", "View group order sheets", "Allows reviewing order sheets across groups."),
        new("ordersheets.submit", "Submit order sheets", "Allows submitting an order sheet for review."),
        new("system.full_access", "Full access", "Grants unrestricted access to the whole application.")
    ];

    private static readonly GroupDefinition[] GroupDefinitions =
    [
        new(
            "Student",
            "Can create and monitor personal project work.",
            "devices.read",
            "projects.create",
            "projects.view.own",
            "ordersheets.create",
            "ordersheets.view.own",
            "ordersheets.submit"),
        new(
            "Teaching Assistant",
            "Can monitor projects and review order sheets.",
            "devices.read",
            "devices.create",
            "devices.update",
            "projects.view.group",
            "projects.status.manage",
            "ordersheets.view.group"),
        new(
            "Teacher",
            "Can supervise projects and manage lab inventory.",
            "devices.read",
            "devices.create",
            "devices.update",
            "devices.delete",
            "projects.view.group",
            "projects.status.manage",
            "ordersheets.view.group"),
        new(
            "Order",
            "Can review submitted order sheets and supporting details.",
            "devices.read",
            "ordersheets.view.group"),
        new(
            "Administrator",
            "Full access over users, groups, permissions, projects, and order sheets.",
            "devices.read",
            "devices.create",
            "devices.update",
            "devices.delete",
            "admin.access",
            "admin.users.manage",
            "admin.groups.manage",
            "projects.create",
            "projects.view.own",
            "projects.view.group",
            "projects.status.manage",
            "ordersheets.create",
            "ordersheets.view.own",
            "ordersheets.view.group",
            "ordersheets.submit",
            "system.full_access")
    ];

    public static async Task SeedAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        foreach (var definition in PermissionDefinitions)
        {
            var permission = await dbContext.Permissions
                .SingleOrDefaultAsync(item => item.Code == definition.Code, cancellationToken);

            if (permission is null)
            {
                dbContext.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = definition.Code,
                    Name = definition.Name,
                    Description = definition.Description,
                    IsSystem = true,
                    CreatedAtUtc = now
                });
            }
            else
            {
                permission.Name = definition.Name;
                permission.Description = definition.Description;
                permission.IsSystem = true;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var definition in GroupDefinitions)
        {
            var group = await dbContext.Groups
                .SingleOrDefaultAsync(item => item.Name == definition.Name, cancellationToken);

            if (group is null)
            {
                dbContext.Groups.Add(new Group
                {
                    Id = Guid.NewGuid(),
                    Name = definition.Name,
                    Description = definition.Description,
                    IsSystem = true,
                    CreatedAtUtc = now
                });
            }
            else
            {
                group.Description = definition.Description;
                group.IsSystem = true;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var permissionMap = await dbContext.Permissions
            .ToDictionaryAsync(item => item.Code, item => item, cancellationToken);

        var groupMap = await dbContext.Groups
            .Include(item => item.GroupPermissions)
            .ToDictionaryAsync(item => item.Name, item => item, cancellationToken);

        foreach (var definition in GroupDefinitions)
        {
            var group = groupMap[definition.Name];
            var existingPermissionIds = group.GroupPermissions
                .Select(item => item.PermissionId)
                .ToHashSet();

            foreach (var permissionCode in definition.PermissionCodes)
            {
                var permission = permissionMap[permissionCode];
                if (existingPermissionIds.Contains(permission.Id))
                {
                    continue;
                }

                dbContext.GroupPermissions.Add(new GroupPermission
                {
                    GroupId = group.Id,
                    PermissionId = permission.Id,
                    GrantedAtUtc = now
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var adminUser = await dbContext.Users
            .SingleOrDefaultAsync(item => item.Username == "admin", cancellationToken);

        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                RadboudEmail = "admin@ru.nl",
                Username = "admin",
                PasswordHash = passwordHasher.Hash("ChangeMeNow!2026"),
                StudentNumber = "ADMIN-0001",
                EnrollmentDate = new DateOnly(2026, 2, 1),
                IsActive = true,
                CreatedAtUtc = now
            };

            dbContext.Users.Add(adminUser);
        }
        else
        {
            adminUser.RadboudEmail = "admin@ru.nl";
            adminUser.IsActive = true;
            if (string.IsNullOrWhiteSpace(adminUser.PasswordHash))
            {
                adminUser.PasswordHash = passwordHasher.Hash("ChangeMeNow!2026");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var administratorGroupId = groupMap["Administrator"].Id;
        var adminMembershipExists = await dbContext.UserGroups.AnyAsync(
            item => item.UserId == adminUser.Id && item.GroupId == administratorGroupId,
            cancellationToken);

        if (!adminMembershipExists)
        {
            dbContext.UserGroups.Add(new UserGroup
            {
                UserId = adminUser.Id,
                GroupId = administratorGroupId,
                JoinedAtUtc = now
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed record PermissionDefinition(string Code, string Name, string Description);

    private sealed record GroupDefinition(
        string Name,
        string Description,
        params string[] PermissionCodes);
}
