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
            builder.Services.AddSingleton<Services.IBluetoothLowEnergyTransport, Platforms.Android.AndroidBluetoothLowEnergyTransport>();
            builder.Services.AddSingleton<Services.IWatchConnectionService, Services.BluetoothWatchConnectionService>();
#else
            builder.Services.AddSingleton<Services.IWatchConnectionService, Services.SimulatedWatchConnectionService>();
#endif
            builder.Services.AddSingleton<Controllers.HomeController>();
            builder.Services.AddSingleton<Views.MainPage>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
