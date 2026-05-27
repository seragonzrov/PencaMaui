using PencaMaui.Services;

namespace PencaMaui;

public partial class App : Application
{
    private readonly AuthService _auth;

    public App(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Si ya hay sesión guardada, ir directo al home
        var logueado = await _auth.CargarSesionAsync();
        if (logueado)
            await Shell.Current.GoToAsync("//home");
        else
            await Shell.Current.GoToAsync("//login");
    }
}
