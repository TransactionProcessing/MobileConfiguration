using Microsoft.EntityFrameworkCore;
using MobileConfiguration.Database;
using MobileConfiguration.Repository;
using NLog;
using NLog.Extensions.Logging;
using Sentry.Extensibility;
using Shared.EntityFramework;
using Shared.Extensions;
using Shared.General;
using Shared.Logger;
using Shared.Logger.TennantContext;
using Shared.Middleware;
using Shared.Serialisation;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
    .AddJsonFile($"/home/txnproc/config/appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables().Build();

ConfigurationReader.Initialise(configuration);

// Configure Sentry on the webBuilder using the config snapshot.
var sentrySection = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "Dsn", "N/A");
if (sentrySection != "N/A")
{
    // Replace the condition below if you intended to only enable Sentry in certain environments.
    if (builder.Environment.IsDevelopment() == false)
    {
        builder.WebHost.UseSentry(o =>
        {
            o.Dsn = sentrySection;
            o.SendDefaultPii = true;
            o.MaxRequestBodySize = RequestSize.Always;
            o.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
            o.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
            o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        });
    }
}


String contentRoot = Directory.GetCurrentDirectory();

String nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

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

ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddNLog(); // bridges Microsoft ILogger to NLog
});
ILogger logger = loggerFactory.CreateLogger("MobileConfiguration");
Shared.Logger.Logger.Initialise(logger);


builder.Host.UseWindowsService();

String path = Assembly.GetExecutingAssembly().Location;
path = Path.GetDirectoryName(path);
builder.Configuration.SetBasePath(path)
       .AddJsonFile("hosting.json", optional: true)
       .AddJsonFile("hosting.development.json", optional: true)
       .AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
// Use minimal APIs and handler pattern instead of MVC controllers
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
    SqlServerRetryOptions retryOptions;
    try
    {
        retryOptions = ConfigurationReader.GetSection<SqlServerRetryOptions>("AppSettings:SqlServerRetry");
    }
    catch (KeyNotFoundException)
    {
        retryOptions = null;
    }

    if (retryOptions != null)
    {
        builder.Services.AddDbContext<ConfigurationContext>(options => options.UseSharedSqlServer<ConfigurationContext>(ConfigurationReader.GetConnectionString("ConfigurationDatabase"), retry => {
            retry.AdditionalTransientErrorNumbers = retryOptions.AdditionalTransientErrorNumbers;
            retry.MaxRetryCount = retryOptions.MaxRetryCount;
            retry.MaxRetryDelay = retryOptions.MaxRetryDelay;
        }));
    }
    else
    {
        builder.Services.AddDbContext<ConfigurationContext>(options => options.UseSqlServer(ConfigurationReader.GetConnectionString("ConfigurationDatabase"), retry => {
            retry.EnableRetryOnFailure();
        }));
    }
}
bool logRequests = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
bool logResponses = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
LogLevel middlewareLogLevel = ConfigurationReader.GetValueOrDefault("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

RequestResponseMiddlewareLoggingConfig config = new(middlewareLogLevel, logRequests, logResponses);

builder.Services.AddSingleton(config);

builder.Services.AddSingleton<IStringSerialiser, SystemTextJsonSerializer>();
builder.Services.AddSingleton<Func<Object, String>>(_ => obj => StringSerialiser.Serialise(obj));
builder.Services.AddSingleton<Func<String, Type, Object>>(_ => (str, type) => StringSerialiser.DeserializeObject<Object>(str, type));
builder.Services.AddSingleton(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions());
builder.Services.ConfigureHttpJsonOptions(options => {
    JsonSerializerConfiguration.ConfigureMinimalApi(options.SerializerOptions);
});
var app = builder.Build();

var serialiser = app.Services.GetRequiredService<IStringSerialiser>();
StringSerialiser.Initialise(serialiser);

app.UseMiddleware<TenantMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
app.UseAuthorization();

app.AddRequestResponseLogging();
app.AddExceptionHandler();


// Minimal API endpoints (handler pattern)
app.MapPost("/api/TransactionMobileConfiguration", MobileConfiguration.Handlers.TransactionMobileConfigurationHandler.PostConfiguration);
app.MapGet("/api/TransactionMobileConfiguration/{id}", MobileConfiguration.Handlers.TransactionMobileConfigurationHandler.GetConfiguration);
app.MapPut("/api/TransactionMobileConfiguration/{id}", MobileConfiguration.Handlers.TransactionMobileConfigurationHandler.PutConfiguration);

app.MapPost("/api/TransactionMobileLogging", MobileConfiguration.Handlers.TransactionMobileLoggingHandler.PostLogging);

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


public static class JsonSerializerConfiguration
{
    public static void ConfigureMinimalApi(JsonSerializerOptions serializerOptions)
    {
        var defaultOptions = SystemTextJsonSerializer.GetDefaultJsonSerializerOptions();
        serializerOptions.PropertyNamingPolicy = defaultOptions.PropertyNamingPolicy;
        serializerOptions.DictionaryKeyPolicy = defaultOptions.DictionaryKeyPolicy;
        serializerOptions.ReferenceHandler = defaultOptions.ReferenceHandler;
        serializerOptions.WriteIndented = defaultOptions.WriteIndented;
        serializerOptions.Converters.Add(new DateTimeSpaceConverter());
    }
}