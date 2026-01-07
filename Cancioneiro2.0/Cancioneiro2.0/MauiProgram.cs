using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Cancioneiro2._0;

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
        
        // CONFIGURAÇÃO - Carregar appsettings.json
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Cancioneiro2._0.appsettings.json");
            
        if (stream == null)
        {
            throw new Exception("appsettings.json não foi encontrado como EmbeddedResource!");
        }
            
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Configuration.AddConfiguration(config);

        // ========================================
        // REGISTAR SERVIÇOS (vais adicionar aqui)
        // ========================================
        // builder.Services.AddSingleton<ISqlDatabaseService, SqlDatabaseService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}