using System.Reflection;

namespace Rx.IB2.Utils;

public static class AppNameManager {
    public static string GetAppName(IHostEnvironment host) {
        var appName = Assembly.GetEntryAssembly()?.FullName?.Split(',')[0] ?? "(Unmanaged)";

        if (host.IsDevelopment()) {
            appName += ".Development";
        } else if (host.IsProduction()) {
            appName += ".Production";
        }

        return appName;
    }
}