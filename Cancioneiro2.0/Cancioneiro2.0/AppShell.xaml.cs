using Cancioneiro2._0.Views;
namespace Cancioneiro2._0;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("CancaoListPage", typeof(CancaoListPage));
    }
} 