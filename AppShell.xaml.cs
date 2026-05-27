using PencaMaui.Views;

namespace PencaMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rutas de navegación (no tab)
        Routing.RegisterRoute("posiciones", typeof(PosicionesPage));
        Routing.RegisterRoute("prediccion", typeof(PrediccionPage));
        Routing.RegisterRoute("registro", typeof(RegistroPage));
    }
}
