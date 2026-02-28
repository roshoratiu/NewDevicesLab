using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewDevicesLab.Application.Administration;
using NewDevicesLab.Application.Devices;
using NewDevicesLab.Application.Security;
using NewDevicesLab.Persistance.Data;
using NewDevicesLab.Persistance.Repositories;

namespace NewDevicesLab.Persistance;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistance(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configuredConnection = configuration.GetConnectionString("DefaultConnection");
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Your_strong_password_123!";

        var connectionString = string.IsNullOrWhiteSpace(configuredConnection)
            ? $"Server=localhost,1434;Database=NewDevicesLabDb;User Id=sa;Password={password};TrustServerCertificate=True;Encrypt=False"
            : configuredConnection;

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IAdministrationRepository, AdministrationRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        return services;
    }
}
