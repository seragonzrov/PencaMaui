using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PencaMaui.Services;
using Plugin.Firebase.Auth.Google;

namespace PencaMaui.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _auth;
    private static bool _yaHuboSesion = false;

    [ObservableProperty] string saludo = "Bienvenido";
    [ObservableProperty] string email = string.Empty;
    [ObservableProperty] string password = string.Empty;
    [ObservableProperty] bool isBusy;
    [ObservableProperty] string errorMessage = string.Empty;

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;
        Saludo = _yaHuboSesion ? "Bienvenido de nuevo" : "Bienvenido";
        _yaHuboSesion = true;
        ErrorMessage = string.Empty;
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
            //await Task.Delay(100);
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
            ErrorMessage = string.Empty;

    #if ANDROID
            // SignInWithGoogleAsync hace el login nativo de Google Y el intercambio
            // de credencial con Firebase en un solo paso, devolviendo el usuario de Firebase
            // (el backend valida tokens de Firebase, igual que en la versión web)
            var firebaseUser = await CrossFirebaseAuthGoogle.Current.SignInWithGoogleAsync();
            var tokenResult = await firebaseUser.GetIdTokenResultAsync(false);
            var firebaseIdToken = tokenResult.Token;

            var (success, error) = await _auth.LoginFirebaseAsync(firebaseIdToken);
            if (success)
            {
                await Shell.Current.GoToAsync("//home");
                await Task.Delay(100);
                (Shell.Current as AppShell)?.ResetearTabs();
            }
            else
                ErrorMessage = error;
    #endif
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Console.WriteLine($"[Google] {ex.GetType().Name}: {ex.Message}");
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
