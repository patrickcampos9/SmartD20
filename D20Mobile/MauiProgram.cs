using Microsoft.Extensions.Logging;

namespace D20Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if ANDROID
            builder.Services.AddSingleton<Platforms.Android.AndroidBluetoothLowEnergyTransport>();
            builder.Services.AddSingleton<Services.IBluetoothLowEnergyTransport>(services =>
                services.GetRequiredService<Platforms.Android.AndroidBluetoothLowEnergyTransport>());
            builder.Services.AddSingleton<Services.IGattServiceDiscoveryTransport>(services =>
                services.GetRequiredService<Platforms.Android.AndroidBluetoothLowEnergyTransport>());
            builder.Services.AddSingleton<Services.BluetoothWatchConnectionService>();
            builder.Services.AddSingleton<Services.BluetoothGattDiagnosticsService>();
#if DEBUG
            builder.Services.AddSingleton<Services.SimulatedWatchConnectionService>();
            builder.Services.AddSingleton<Services.SimulatedGattDiagnosticsService>();
            builder.Services.AddSingleton<Services.DevelopmentWatchService>();
            builder.Services.AddSingleton<Services.IWatchConnectionService>(services =>
                services.GetRequiredService<Services.DevelopmentWatchService>());
            builder.Services.AddSingleton<Services.IGattDiagnosticsService>(services =>
                services.GetRequiredService<Services.DevelopmentWatchService>());
#else
            builder.Services.AddSingleton<Services.IWatchConnectionService>(services =>
                services.GetRequiredService<Services.BluetoothWatchConnectionService>());
            builder.Services.AddSingleton<Services.IGattDiagnosticsService>(services =>
                services.GetRequiredService<Services.BluetoothGattDiagnosticsService>());
#endif
#else
            builder.Services.AddSingleton<Services.IWatchConnectionService, Services.SimulatedWatchConnectionService>();
            builder.Services.AddSingleton<Services.IGattDiagnosticsService, Services.SimulatedGattDiagnosticsService>();
#endif
            builder.Services.AddSingleton<Controllers.HomeController>();
            builder.Services.AddSingleton<Controllers.GattDiagnosticsController>();
            builder.Services.AddSingleton<Views.MainPage>();
            builder.Services.AddSingleton<Views.GattDiagnosticsPage>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
