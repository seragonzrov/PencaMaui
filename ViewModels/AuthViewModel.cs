using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PencaMaui.Services;

namespace PencaMaui.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly AuthService _auth;
    private static bool _yaHuboSesion = false;

    // Login
    [ObservableProperty] string saludo = "Bienvenido";
    [ObservableProperty] string email = string.Empty;
    [ObservableProperty] string password = string.Empty;
    [ObservableProperty] string errorMessage = string.Empty;

    // Registro
    [ObservableProperty] string nombre = string.Empty;
    [ObservableProperty] string emailRegistro = string.Empty;
    [ObservableProperty] string passwordRegistro = string.Empty;
    [ObservableProperty] string codigoInvitacion = string.Empty;
    [ObservableProperty] string errorRegistro = string.Empty;

    [ObservableProperty] bool isBusy;

    public event Action? MostrarRegistro;
    public event Action? MostrarLogin;

    public AuthViewModel(AuthService auth)
    {
        _auth = auth;
        Saludo = _yaHuboSesion ? "Bienvenido de nuevo" : "Bienvenido";
        _yaHuboSesion = true;
    }

    [RelayCommand]
    async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Ingresá tu email y contraseña";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        var (success, error) = await _auth.LoginAsync(Email, Password);

        IsBusy = false;

        if (success)
        {
            await Shell.Current.GoToAsync("//home");
            await Task.Delay(100);
            (Shell.Current as AppShell)?.ResetearTabs();
        }
        else
            ErrorMessage = error;
    }

    [RelayCommand]
    async Task LoginGoogleAsync()
    {
        try
        {
            IsBusy = true;
            var authUrl = new Uri("https://tupenca-api-production-ed1d.up.railway.app/api/Auth/google");
            var callbackUrl = new Uri("tupenca://callback");

            var result = await WebAuthenticator.Default.AuthenticateAsync(authUrl, callbackUrl);

            if (result.Properties.TryGetValue("token", out var idToken))
            {
                var (success, error) = await _auth.LoginFirebaseAsync(idToken);
                if (success)
                {
                    await Shell.Current.GoToAsync("//home");
                    await Task.Delay(100);
                    (Shell.Current as AppShell)?.ResetearTabs();
                }
                else
                    ErrorMessage = error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al iniciar sesión con Google";
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    void IrRegistro()
    {
        ErrorMessage = string.Empty;
        MostrarRegistro?.Invoke();
    }

    [RelayCommand]
    void VolverLogin()
    {
        ErrorRegistro = string.Empty;
        MostrarLogin?.Invoke();
    }

    [RelayCommand]
    async Task RegistrarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre) ||
            string.IsNullOrWhiteSpace(EmailRegistro) ||
            string.IsNullOrWhiteSpace(PasswordRegistro))
        {
            ErrorRegistro = "Completá todos los campos obligatorios";
            return;
        }

        IsBusy = true;
        ErrorRegistro = string.Empty;

        var (success, error) = await _auth.RegistrarAsync(
            Nombre, EmailRegistro, PasswordRegistro,
            string.IsNullOrWhiteSpace(CodigoInvitacion) ? null : CodigoInvitacion);

        IsBusy = false;

        if (success)
            await Shell.Current.GoToAsync("//home");
        else
            ErrorRegistro = error;
    }
}