using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace MyTrace
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Add logging
            builder.Logging.AddDebug();

            // Register services and pages with DI
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<BarcodeScanner>();
            builder.Services.AddTransient<ILogger<SimilarProductViewModel>, Logger<SimilarProductViewModel>>();

            // Build and return the app
            return builder.Build();
        }
    }
}
