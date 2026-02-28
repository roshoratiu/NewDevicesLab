using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NewDevicesLab.Application.Administration;
using NewDevicesLab.Application.Devices;
using NewDevicesLab.Application.Projects;
using NewDevicesLab.Application.Security;
using NewDevicesLab.Frontend.Security;
using NewDevicesLab.Persistance;
using NewDevicesLab.Persistance.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ndl.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect("/");
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect("/");
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	var title = builder.Configuration["Swagger:Title"] ?? "New Devices Lab API";
	var version = builder.Configuration["Swagger:Version"] ?? "v1";
	var description = builder.Configuration["Swagger:Description"]
		?? "API used for New Devices Lab coursework.";

	options.SwaggerDoc(version, new OpenApiInfo
	{
		Title = title,
		Version = version,
		Description = description,
		Contact = new OpenApiContact
		{
			Name = "New Devices Lab Team"
		}
	});

	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	if (File.Exists(xmlPath))
	{
		options.IncludeXmlComments(xmlPath);
	}
});

builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddPersistance(builder.Configuration);

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await EnsureDatabaseIsReadyAsync(dbContext, passwordHasher, app.Lifetime.ApplicationStopping);
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization warning: {ex.Message}");
    Console.WriteLine("The app will continue running. Some features may not work until the database is reachable.");
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.DocumentTitle = "New Devices Lab - Swagger";
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "New Devices Lab API v1");
	options.DefaultModelsExpandDepth(-1);
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task EnsureDatabaseIsReadyAsync(
	AppDbContext dbContext,
	IPasswordHasher passwordHasher,
	CancellationToken cancellationToken)
{
	const int maxAttempts = 12;

	for (var attempt = 1; attempt <= maxAttempts; attempt++)
	{
		try
		{
			await EnsureDatabaseSchemaAsync(dbContext, cancellationToken);
            await EnsureProjectSchemaAsync(dbContext, cancellationToken);
			await AppDbSeeder.SeedAsync(dbContext, passwordHasher, cancellationToken);
			return;
		}
		catch (SqlException) when (attempt < maxAttempts)
		{
			await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
		}
	}

	await EnsureDatabaseSchemaAsync(dbContext, cancellationToken);
    await EnsureProjectSchemaAsync(dbContext, cancellationToken);
	await AppDbSeeder.SeedAsync(dbContext, passwordHasher, cancellationToken);
}

static async Task EnsureDatabaseSchemaAsync(AppDbContext dbContext, CancellationToken cancellationToken)
{
    var migrations = await dbContext.Database.GetMigrationsAsync(cancellationToken);

    if (migrations.Any())
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        return;
    }

    await dbContext.Database.EnsureCreatedAsync(cancellationToken);
}

static Task EnsureProjectSchemaAsync(AppDbContext dbContext, CancellationToken cancellationToken)
{
    const string sql = """
        IF OBJECT_ID(N'[Projects]', N'U') IS NULL
        BEGIN
            CREATE TABLE [Projects](
                [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                [Name] nvarchar(160) NOT NULL,
                [Idea] nvarchar(240) NOT NULL,
                [Description] nvarchar(2000) NOT NULL,
                [Status] nvarchar(60) NOT NULL,
                [CreatedByUserId] uniqueidentifier NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                CONSTRAINT [FK_Projects_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([Id])
            );
        END;

        IF OBJECT_ID(N'[ProjectMembers]', N'U') IS NULL
        BEGIN
            CREATE TABLE [ProjectMembers](
                [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                [ProjectId] uniqueidentifier NOT NULL,
                [UserId] uniqueidentifier NOT NULL,
                [IsOwner] bit NOT NULL,
                [AddedAtUtc] datetime2 NOT NULL,
                CONSTRAINT [FK_ProjectMembers_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_ProjectMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
            );

            CREATE UNIQUE INDEX [IX_ProjectMembers_ProjectId_UserId] ON [ProjectMembers]([ProjectId], [UserId]);
            CREATE INDEX [IX_ProjectMembers_UserId] ON [ProjectMembers]([UserId]);
        END;

        IF OBJECT_ID(N'[OrderSheets]', N'U') IS NULL
        BEGIN
            CREATE TABLE [OrderSheets](
                [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                [ProjectId] uniqueidentifier NOT NULL,
                [CreatedByUserId] uniqueidentifier NOT NULL,
                [Status] nvarchar(60) NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [SubmittedAtUtc] datetime2 NULL,
                CONSTRAINT [FK_OrderSheets_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_OrderSheets_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([Id])
            );

            CREATE INDEX [IX_OrderSheets_ProjectId] ON [OrderSheets]([ProjectId]);
            CREATE INDEX [IX_OrderSheets_CreatedByUserId] ON [OrderSheets]([CreatedByUserId]);
        END;

        IF OBJECT_ID(N'[OrderSheetItems]', N'U') IS NULL
        BEGIN
            CREATE TABLE [OrderSheetItems](
                [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                [OrderSheetId] uniqueidentifier NOT NULL,
                [SiteName] nvarchar(200) NOT NULL,
                [ComponentName] nvarchar(200) NOT NULL,
                [Brand] nvarchar(120) NOT NULL,
                [Link] nvarchar(1000) NOT NULL,
                [PriceEuro] decimal(10,2) NOT NULL,
                CONSTRAINT [FK_OrderSheetItems_OrderSheets_OrderSheetId] FOREIGN KEY ([OrderSheetId]) REFERENCES [OrderSheets]([Id]) ON DELETE CASCADE
            );

            CREATE INDEX [IX_OrderSheetItems_OrderSheetId] ON [OrderSheetItems]([OrderSheetId]);
        END;
        """;

    return dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
}
