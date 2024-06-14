using System.Text.Json.Serialization;
using Rx.IB2.Hubs;
using Rx.IB2.Services;
using Rx.IB2.Services.IbApiHandlers;
using Rx.IB2.Services.IbApiSenders;
using Rx.IB2.Utils;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy
            .AllowCredentials()
            .AllowAnyHeader()
            .WithOrigins("http://localhost:3100");
    });
});
builder.Services
    .AddSignalR()
    // Serialize Enum as string
    .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<IbApiOptionDefinitionsManager>();
builder.Services.AddSingleton<IbApiContractDetailsManager>();
builder.Services.AddSingleton<IbApiHistoryPxRequestManager>();
builder.Services.AddSingleton<IbApiOneTimePxRequestManager>();
builder.Services.AddSingleton<IbApiRequestManager>();
builder.Services.AddSingleton<IbApiHandler>();
builder.Services.AddHostedService<IbApiReceiver>();
builder.Services.AddSingleton<IbApiSender>();
builder.Services.AddSingleton<LoggingHelper>();
builder.Services.AddControllers();

var app = builder.Build();

app.Services.GetRequiredService<LoggingHelper>().Initialize();

app.UseCors();
app.MapHub<SignalRHub>("/signalr");
app.MapControllers();

app.Services.GetRequiredService<IbApiSender>().Connect();

app.Lifetime.ApplicationStarted.Register(() => Log.Information("Rx.IB2 Started"));
app.Lifetime.ApplicationStopping.Register(() => app.Services.GetRequiredService<IbApiSender>().Disconnect());

await app.RunAsync();