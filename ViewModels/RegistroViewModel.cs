using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PencaMaui.Services;

namespace PencaMaui.ViewModels;

public partial class RegistroViewModel : ObservableObject
{
    private readonly AuthService _auth;

    [ObservableProperty] string nombre = string.Empty;
    [ObservableProperty] string email = string.Empty;
    [ObservableProperty] string password = string.Empty;
    [ObservableProperty] string codigoInvitacion = string.Empty;
    [ObservableProperty] bool isBusy;
    [ObservableProperty] string errorMessage = string.Empty;

    public RegistroViewModel(AuthService auth)
    {
        _auth = auth;
    }

    [RelayCommand]
    async Task RegistrarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Completá todos los campos obligatorios";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        var (success, error) = await _auth.RegistrarAsync(
            Nombre, Email, Password,
            string.IsNullOrWhiteSpace(CodigoInvitacion) ? null : CodigoInvitacion);

        IsBusy = false;

        if (success)
            await Shell.Current.GoToAsync("//home");
        else
            ErrorMessage = error;
    }

    [RelayCommand]
    async Task VolverAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

}
