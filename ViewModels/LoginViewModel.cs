using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PencaMaui.Services;

namespace PencaMaui.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _auth;

    [ObservableProperty] string email = string.Empty;
    [ObservableProperty] string password = string.Empty;
    [ObservableProperty] bool isBusy;
    [ObservableProperty] string errorMessage = string.Empty;

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;
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
        // WebAuthenticator para OAuth con Google via Firebase
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
    async Task IrRegistroAsync()
    {
        await Shell.Current.GoToAsync("registro");
    }
}
