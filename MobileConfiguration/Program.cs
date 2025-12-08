using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MobileConfiguration.Database;
using MobileConfiguration.Repository;
using NLog;
using NLog.Extensions.Logging;
using Shared.EntityFramework;
using Shared.Extensions;
using Shared.General;
using Shared.Logger;
using Shared.Logger.TennantContext;
using Shared.Middleware;
using System.Reflection;
using System.Runtime.CompilerServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = NLog.Logger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
    .AddJsonFile($"/home/txnproc/config/appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables().Build();

ConfigurationReader.Initialise(configuration);

builder.Host.UseWindowsService();

String path = Assembly.GetExecutingAssembly().Location;
path = Path.GetDirectoryName(path);
builder.Configuration.SetBasePath(path)
       .AddJsonFile("hosting.json", optional: true)
       .AddJsonFile("hosting.development.json", optional: true)
       .AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<TenantContext>();
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
LogLevel middlewareLogLevel = ConfigurationReaderExtensions.GetValueOrDefault("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

RequestResponseMiddlewareLoggingConfig config = new(middlewareLogLevel, logRequests, logResponses);

builder.Services.AddSingleton(config);


var app = builder.Build();
String contentRoot = Directory.GetCurrentDirectory();

String nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

app.UseMiddleware<TenantMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

LogManager.Setup(b =>
{
    b.SetupLogFactory(setup =>
    {
        setup.AddCallSiteHiddenAssembly(typeof(NlogLogger).Assembly);
        setup.AddCallSiteHiddenAssembly(typeof(Shared.Logger.Logger).Assembly);
        setup.AddCallSiteHiddenAssembly(typeof(TenantMiddleware).Assembly);
    });
    b.LoadConfigurationFromFile(nlogConfigPath);
});

Logger logger = LogManager.LogFactory.GetLogger("MobileConfiguration");

Shared.Logger.Logger.Initialise(logger as ILogger);


// Configure the HTTP request pipeline.
app.UseAuthorization();

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