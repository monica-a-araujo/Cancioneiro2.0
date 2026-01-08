using Cancioneiro2._0.Services.Database;
using System.Collections.ObjectModel;
using Cancioneiro2._0.Services.Models;

namespace Cancioneiro2._0;

public partial class CancaoListPage : ContentPage
{
    
    private readonly ISqlDatabaseService _database;
    
 
    private ObservableCollection<Cancao> _cancoes;

    // ==================== CONSTRUTOR ====================
    
    /// <summary>
    /// O MAUI chama este construtor e INJETA automaticamente o serviço
    /// Isto é Dependency Injection (DI)
    /// </summary>
    public CancaoListPage(ISqlDatabaseService database)
    {
        InitializeComponent(); // Carrega o XAML
        
        _database = database;
        
        // Cria a coleção observável (vazia inicialmente)
        _cancoes = new ObservableCollection<Cancao>();
        
        // LIGA a coleção ao CollectionView do XAML
        // Agora quando adicionares items a _cancoes, aparecem na UI!
        CancaoCollection.ItemsSource = _cancoes;
        
        // Carrega as canções assim que a página abre
        LoadCancoesAsync();
    }

    // ==================== MÉTODOS ====================
    
    /// <summary>
    /// Carrega todas as canções da base de dados
    /// </summary>
    private async void LoadCancoesAsync()
    {
        try
        {
            // 1. Atualiza UI - mostra loading
            LblStatus.Text = "🔄 Carregando canções...";
            LblStatus.TextColor = Colors.Orange;
            
            // 2. Limpa a lista atual (se houver)
            _cancoes.Clear();
            
            // 3. FAZ A QUERY SQL
            // SelectRequestAsync<Cancao>: diz que esperas objetos do tipo Cancao
            // isMany: true = espera múltiplos resultados (lista)
            var result = await _database.SelectRequestAsync<Cancao>(
                "SELECT id, titulo, bpm FROM cancao ORDER BY titulo", 
                isMany: true
            );
            
            // 4. PROCESSA O RESULTADO
            // O resultado vem como 'object', precisamos fazer cast para List<Cancao>
            if (result is List<Cancao> cancaoList)
            {
                // 5. ADICIONA À UI
                // Para cada canção retornada, adiciona à ObservableCollection
                // A UI atualiza AUTOMATICAMENTE a cada Add()!
                foreach (var cancao in cancaoList)
                {
                    _cancoes.Add(cancao);
                }
                
                // 6. Atualiza status de sucesso
                LblStatus.Text = $"✅ {cancaoList.Count} canção(ões) carregada(s)";
                LblStatus.TextColor = Colors.Green;
            }
            else
            {
                // Resultado vazio ou formato errado
                LblStatus.Text = "⚠️ Nenhuma canção encontrada";
                LblStatus.TextColor = Colors.Orange;
            }
        }
        catch (Exception ex)
        {
            // 7. TRATAMENTO DE ERROS
            LblStatus.Text = $"❌ Erro ao carregar";
            LblStatus.TextColor = Colors.Red;
            
            // Mostra detalhes do erro ao utilizador
            await DisplayAlert("Erro", 
                $"Falha ao carregar canções:\n\n{ex.Message}", 
                "OK");
        }
    }

    /// <summary>
    /// Chamado quando o botão refresh (🔄) é clicado
    /// </summary>
    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadCancoesAsync();
    }

    /// <summary>
    /// Chamado quando o utilizador clica numa canção da lista
    /// </summary>
    private async void OnCancaoSelected(object sender, SelectionChangedEventArgs e)
    {
        // e.CurrentSelection contém os items selecionados
        // FirstOrDefault() pega o primeiro (ou null se nenhum)
        if (e.CurrentSelection.FirstOrDefault() is Cancao selectedCancao)
        {
            // Mostra detalhes numa popup
            await DisplayAlert(
                "Canção Selecionada",
                $"Título: {selectedCancao.Titulo}\n" +
                $"ID: {selectedCancao.Id}\n" +
                $"BPM: {selectedCancao.Bpm ?? 0}",
                "OK"
            );
            
            // Desmarca a seleção (para poder clicar novamente)
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}