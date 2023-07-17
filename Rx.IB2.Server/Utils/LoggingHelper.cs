using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Rx.IB2.Utils;


internal class UtcTimestampEnricher : ILogEventEnricher {
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propFactory) {
        logEvent.AddPropertyIfAbsent(propFactory.CreateProperty("TimestampUtc", logEvent.Timestamp.UtcDateTime));
    }
}

public class LoggingHelper {
    private const string OutputTemplate =
        "{TimestampUtc:yyyy-MM-dd HH:mm:ss.fff} [{ProcessId,6}] [{ThreadId,3}] "
        + "{SourceContext,52} [{Level:u1}] {Message:lj}{NewLine}{Exception}";

    private IConfiguration Config { get; }
    
    private IHostEnvironment Host { get; }

    public LoggingHelper(IConfiguration config, IHostEnvironment host) {
        Config = config;
        Host = host;
    }

    public void Initialize() {
        var logDir = Config.GetSection("Logging").GetSection("Additional").GetValue<string>("FileDirectory");
        
        Initialize(logDir, Host.IsDevelopment(), Host.IsProduction());
    }

    private void Initialize(string? logDir, bool isDev, bool isProd) {
        var appName = AppNameManager.GetAppName(isDev, isProd);

        var loggerConfig = new LoggerConfiguration()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.With(new UtcTimestampEnricher())
            .MinimumLevel.Information();

        if (isDev) {
            loggerConfig = loggerConfig.WriteTo.Console(outputTemplate: OutputTemplate);
        }

        if (logDir is not null) {
            loggerConfig = loggerConfig.WriteTo.File(
                Path.Combine(logDir, $"{appName}-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: OutputTemplate,
                shared: true,
                fileSizeLimitBytes: null
            );
        }

        loggerConfig = loggerConfig.ReadFrom.Configuration(Config);

        Log.Logger = loggerConfig.CreateLogger();
    }
}