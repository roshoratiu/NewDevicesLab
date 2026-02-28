using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewDevicesLab.Application.Administration;
using NewDevicesLab.Application.Devices;
using NewDevicesLab.Application.Projects;
using NewDevicesLab.Application.Security;
using NewDevicesLab.Persistance.Data;
using NewDevicesLab.Persistance.Repositories;

namespace NewDevicesLab.Persistance;

public static class DependencyInjection
{
    private const string ProductionPlaceholderPassword = "__SET_IN_MONSTERASP_OR_ENV__";

    public static IServiceCollection AddPersistance(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configuredConnection = configuration.GetConnectionString("DefaultConnection");
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Your_strong_password_123!";
        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"];

        var connectionString = configuredConnection;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = $"Server=127.0.0.1,1434;Database=NewDevicesLabDb;User Id=sa;Password={password};TrustServerCertificate=True;Encrypt=False";
        }
        else if (connectionString.Contains(ProductionPlaceholderPassword, StringComparison.Ordinal))
        {
            if (string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The production connection string still contains the placeholder password. " +
                    "Set ConnectionStrings__DefaultConnection in the hosting environment.");
            }

            connectionString = connectionString.Replace(ProductionPlaceholderPassword, password, StringComparison.Ordinal);
        }

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IAdministrationRepository, AdministrationRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        return services;
    }
}
