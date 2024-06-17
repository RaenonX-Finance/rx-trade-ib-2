using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.File.Archive;

namespace Rx.IB2.Utils;

internal class UtcTimestampEnricher : ILogEventEnricher {
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propFactory) {
        logEvent.AddPropertyIfAbsent(propFactory.CreateProperty("TimestampUtc", logEvent.Timestamp.UtcDateTime));
    }
}

public class LoggingHelper(IConfiguration config, IHostEnvironment host) {
    private const string OutputTemplate =
        "{TimestampUtc:yyyy-MM-dd HH:mm:ss.fff} [{ProcessId,6}] [{ThreadId,3}] "
        + "{SourceContext,52} [{Level:u1}] {Message:lj}{NewLine}{Exception}";

    private IConfiguration Config { get; } = config;

    private IHostEnvironment Host { get; } = host;

    public void Initialize() {
        var logDir = Config.GetSection("Logging").GetSection("Additional").GetValue<string>("FileDirectory");

        Initialize(Host, logDir);
    }

    private void Initialize(IHostEnvironment host, string? logDir) {
        var appName = AppNameManager.GetAppName(host);

        var loggerConfig = new LoggerConfiguration()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.With(new UtcTimestampEnricher())
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: OutputTemplate);

        if (logDir is not null) {
            loggerConfig = loggerConfig.WriteTo.File(
                Path.Combine(logDir, $"{appName}-.log"),
                rollingInterval: RollingInterval.Hour,
                retainedFileCountLimit: 1,
                outputTemplate: OutputTemplate,
                fileSizeLimitBytes: null,
                hooks: new ArchiveHooks()
            );
        }

        loggerConfig = loggerConfig.ReadFrom.Configuration(Config);

        Log.Logger = loggerConfig.CreateLogger();
    }
}