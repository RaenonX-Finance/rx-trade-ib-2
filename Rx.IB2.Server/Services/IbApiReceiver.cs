using IBApi;
using Rx.IB2.Services.IbApiHandlers;
using ILogger = Serilog.ILogger;

namespace Rx.IB2.Services;

public class IbApiReceiver : BackgroundService {
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(IbApiReceiver));

    private EReader Reader { get; }

    private IbApiHandler Handler { get; }

    public IbApiReceiver(IbApiHandler handler) {
        Reader = new EReader(handler.ClientSocket, handler.ReaderSignal);
        Handler = handler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        Log.Information("Starting IB API receiver");
        Reader.Start();

        await Task.Run(
            () => {
                while (Handler.ClientSocket.IsConnected()) {
                    Handler.ReaderSignal.waitForSignal();
                    Reader.processMsgs();
                }

                if (!Handler.ClientSocket.IsConnected()) {
                    Log.Warning("IB API Socket disconnected, terminating IB API receiver");
                }

                Log.Warning(
                    "Either IB API socket is not connected anymore or cancellation requested, " +
                    "terminating IB API receiver"
                );
            },
            stoppingToken
        ).ContinueWith(
            t => {
                // `t.Exception?.Flatten().InnerExceptions` guaranteed not null
                // by `TaskContinuationOptions.OnlyOnFaulted`
                foreach (var exception in t.Exception?.Flatten().InnerExceptions!) {
                    Log.Error(exception, "Error occurred while reading IB API message");
                }
            },
            TaskContinuationOptions.OnlyOnFaulted
        );

        Log.Warning("IB API reader task ended");
    }
}