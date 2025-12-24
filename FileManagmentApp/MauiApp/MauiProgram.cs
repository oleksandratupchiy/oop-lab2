using Microsoft.Extensions.Logging;
using MauiApp24.Parsers; // Додайте, якщо у вас тут використовуються ваші класи
using Saver;

namespace MauiApp24
{
    public static class MauiProgram
    {
        // 1. Тут повертаємо Microsoft.Maui.Hosting.MauiApp
        public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
        {
            // 2. І тут створюємо через повне ім'я
            var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    // Ваш шрифт
                    fonts.AddFont("Montserrat-VariableFont_wght.ttf", "MyCustomFont");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}