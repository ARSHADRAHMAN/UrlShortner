using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace UrlShortner.Configuration;

/// <summary>
/// Serilog logging configuration
/// Configures structured logging with correlation ID support
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configure Serilog with console, file, and SQL Server sinks
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The logger</returns>
    public static Serilog.ILogger ConfigureSerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "UrlShortener")
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(environment.ContentRootPath, "Logs", "app-.log"),
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 104857600, // 100 MB
                rollOnFileSizeLimit: true)
            .WriteTo.File(
                formatter: new CompactJsonFormatter(),
                path: Path.Combine(environment.ContentRootPath, "Logs", "app-json-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 104857600)
            .CreateLogger();

        Log.Logger = logger;

        return logger;
    }

    /// <summary>
    /// Add Serilog to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });

        return services;
    }
}
