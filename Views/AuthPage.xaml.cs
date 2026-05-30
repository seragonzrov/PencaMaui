using PencaMaui.ViewModels;

namespace PencaMaui.Views;

public partial class AuthPage : ContentPage
{
    public AuthPage(AuthViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        vm.MostrarRegistro += AnimarHaciaRegistro;
        vm.MostrarLogin += AnimarHaciaLogin;
    }

    private async void AnimarHaciaRegistro()
    {
        double ancho = Width;
        PanelRegistro.TranslationX = ancho;
        await Task.WhenAll(
            PanelLogin.TranslateTo(-ancho, 0, 300, Easing.CubicInOut),
            PanelRegistro.TranslateTo(0, 0, 300, Easing.CubicInOut)
        );
    }

    private async void AnimarHaciaLogin()
    {
        double ancho = Width;
        PanelLogin.TranslationX = ancho;
        await Task.WhenAll(
            PanelLogin.TranslateTo(0, 0, 300, Easing.CubicInOut),
            PanelRegistro.TranslateTo(-ancho, 0, 300, Easing.CubicInOut)
        );
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AuthViewModel vm)
            vm.ErrorMessage = string.Empty;
        if (Window != null)
            Window.Resumed += OnResumed;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Window != null)
            Window.Resumed -= OnResumed;
    }

    private void OnResumed(object? sender, EventArgs e)
    {
        if (BindingContext is AuthViewModel vm)
            vm.ErrorMessage = string.Empty;
    }
}