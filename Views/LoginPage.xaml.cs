using PencaMaui.ViewModels;

namespace PencaMaui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel vm)
        {
            vm.ErrorMessage = string.Empty;
            if (vm.SitioSeleccionado == null)
                _ = vm.CargarSitiosAsync();
        }
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
        if (BindingContext is LoginViewModel vm)
            vm.ErrorMessage = string.Empty;
    }
}