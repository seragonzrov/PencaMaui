using PencaMaui.ViewModels;

namespace PencaMaui.Views;

public partial class HomePencasPage : ContentPage
{
    public HomePencasPage(HomePencasViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HomePencasViewModel vm)
            vm.CargarCommand.Execute(null);
    }
}

public partial class PosicionesPage : ContentPage
{
    public PosicionesPage(PosicionesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PosicionesViewModel vm)
            vm.CargarCommand.Execute(null);
    }
}

public partial class PrediccionPage : ContentPage
{
    public PrediccionPage(PrediccionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PrediccionViewModel vm)
            vm.CargarCommand.Execute(null);
    }
}

public partial class PerfilPage : ContentPage
{
    public PerfilPage(PerfilViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PerfilViewModel vm)
            vm.CargarCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is PerfilViewModel vm)
            vm.CancelarCierreCommand.Execute(null);
    }
}
