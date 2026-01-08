using Cancioneiro2._0.Services.Database;
namespace Cancioneiro2._0;

public partial class MainPage : ContentPage
{
    int count = 0;
    private readonly ISqlDatabaseService _database;

    public MainPage(ISqlDatabaseService database)
    {
        InitializeComponent();
        _database = database;
        
        // Testar conexão ao carregar a página
        //TestConnectionAsync();
    }

    private async void OnCounterClicked(object? sender, EventArgs e)
    {
        await this.DisplayAlert("Título", "Mensagem", "OK");
    }
}