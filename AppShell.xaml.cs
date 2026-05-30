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
    }

    public void ResetearTabs()
    {
        var tabBar = Items.OfType<TabBar>().FirstOrDefault();
        if (tabBar != null)
            tabBar.CurrentItem = tabBar.Items[0]; // primera tab = Pencas
    }
}
