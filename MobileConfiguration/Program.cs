using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MobileConfiguration.Database;
using MobileConfiguration.Repository;
using NLog.Extensions.Logging;
using Shared.EntityFramework;
using Shared.Extensions;
using Shared.General;
using Shared.Logger;
using Shared.Middleware;
using Shared.Repositories;
using System.Reflection;
using ILogger = Microsoft.Extensions.Logging.ILogger;

IConfigurationRoot configuration = new ConfigurationBuilder()
                                   .AddJsonFile("appsettings.json")
                                   .AddEnvironmentVariables()
                                   .Build();

ConfigurationReader.Initialise(configuration);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
String path = Assembly.GetExecutingAssembly().Location;
path = Path.GetDirectoryName(path);
builder.Configuration.SetBasePath(path)
       .AddJsonFile("hosting.json", optional: true)
       .AddJsonFile("hosting.development.json", optional: true)
       .AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddSingleton(typeof(IDbContextResolver<>), typeof(DbContextResolver<>));
Boolean isInMemoryDatabase = Boolean.Parse(ConfigurationReader.GetValue("AppSettings", "InMemoryDatabase"));

if (isInMemoryDatabase) {
    builder.Services.AddDbContext<ConfigurationContext>(builder => builder.UseInMemoryDatabase("ConfigurationDatabaseTest"));
}
else {
    builder.Services.AddDbContext<ConfigurationContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("ConfigurationDatabase")));
}
bool logRequests = ConfigurationReaderExtensions.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
bool logResponses = ConfigurationReaderExtensions.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
LogLevel middlewareLogLevel = ConfigurationReaderExtensions.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

RequestResponseMiddlewareLoggingConfig config =
    new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

builder.Services.AddSingleton(config);


var app = builder.Build();

String nlogConfigFilename = "nlog.config";

if (app.Environment.IsDevelopment())
{
    var developmentNlogConfigFilename = "nlog.development.config";
    if (File.Exists(Path.Combine(app.Environment.ContentRootPath, developmentNlogConfigFilename)))
    {
        nlogConfigFilename = developmentNlogConfigFilename;
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

loggerFactory.ConfigureNLog(Path.Combine(path, nlogConfigFilename));
loggerFactory.AddNLog();

ILogger logger = loggerFactory.CreateLogger("MobileConfiguration");

Logger.Initialise(logger);


// Configure the HTTP request pipeline.
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.AddRequestLogging();
app.AddResponseLogging();
app.AddExceptionHandler();


app.MapControllers();

InitializeDatabase(app).Wait(CancellationToken.None);

app.Run();


async Task InitializeDatabase(IApplicationBuilder app)
{
    using (IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
    {
        ConfigurationContext dbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationContext>();
        
        if (dbContext!= null && dbContext.Database.IsRelational())
        {
            await dbContext.MigrateAsync(CancellationToken.None);
        }
    }
}

public static class ConfigurationReaderExtensions {
    public static T GetValueOrDefault<T>(String sectionName,
                                         String keyName,
                                         T defaultValue) {
        try {
            var value = ConfigurationReader.GetValue(sectionName, keyName);

            if (String.IsNullOrEmpty(value)) {
                return defaultValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (KeyNotFoundException kex) {
            return defaultValue;
        }
    }
}