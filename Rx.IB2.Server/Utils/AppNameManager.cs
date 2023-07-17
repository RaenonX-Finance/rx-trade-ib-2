﻿using System.Reflection;

namespace Rx.IB2.Utils;

public static class AppNameManager {
    public static string GetAppName(bool isDev, bool isProd) {
        var appName = Assembly.GetEntryAssembly()?.FullName?.Split(',')[0] ?? "(Unmanaged)";

        if (isDev) {
            appName += ".Development";
        }
        else if (isProd) {
            appName += ".Production";
        }

        return appName;
    }

    public static string GetAppName(WebApplication app) {
        var environment = app.Environment;

        return GetAppName(environment.IsDevelopment(), environment.IsProduction());
    }
}