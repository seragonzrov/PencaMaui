using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PencaMaui.Converters;
using PencaMaui.Services;
using PencaMaui.ViewModels;
using PencaMaui.Views;

namespace PencaMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ─── Services (Singleton) ────────────────────────────────────────────
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<PencaService>();
        builder.Services.AddSingleton<PrediccionService>();
        builder.Services.AddSingleton<UsuarioService>();

        // ─── ViewModels (Transient) ──────────────────────────────────────────
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomePencasViewModel>();
        builder.Services.AddTransient<PosicionesViewModel>();
        builder.Services.AddTransient<PrediccionViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();
        builder.Services.AddTransient<RegistroViewModel>();

        // ─── Pages (Transient) ───────────────────────────────────────────────
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePencasPage>();
        builder.Services.AddTransient<PosicionesPage>();
        builder.Services.AddTransient<PrediccionPage>();
        builder.Services.AddTransient<PerfilPage>();
        builder.Services.AddTransient<RegistroPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
