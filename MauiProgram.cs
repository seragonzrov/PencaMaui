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
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // ─── Services (Singleton) ────────────────────────────────────────────
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<PencaService>();
        builder.Services.AddSingleton<PrediccionService>();
        builder.Services.AddSingleton<UsuarioService>();

        // ─── ViewModels (Transient) ──────────────────────────────────────────
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<HomePencasViewModel>();
        builder.Services.AddTransient<PosicionesViewModel>();
        builder.Services.AddTransient<PrediccionViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        // ─── Pages (Transient) ───────────────────────────────────────────────
        builder.Services.AddTransient<AuthPage>();
        builder.Services.AddTransient<HomePencasPage>();
        builder.Services.AddTransient<PosicionesPage>();
        builder.Services.AddTransient<PrediccionPage>();
        builder.Services.AddTransient<PerfilPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ─── Quitar subrayado nativo de Entry en Android ─────────────────────
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
        #if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        #endif
        });

        return builder.Build();
    }
}
