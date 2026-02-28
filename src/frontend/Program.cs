using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using NewDevicesLab.Application.Administration;
using NewDevicesLab.Application.Devices;
using NewDevicesLab.Application.Security;
using NewDevicesLab.Frontend.Security;
using NewDevicesLab.Infrastructure.Security;
using NewDevicesLab.Persistance;
using NewDevicesLab.Persistance.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddPersistance(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
	dbContext.Database.EnsureCreated();
	await AppDbSeeder.SeedAsync(dbContext, passwordHasher, CancellationToken.None);
}

app.UseDefaultFiles();
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

app.Run();
