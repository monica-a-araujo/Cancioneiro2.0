using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Cancioneiro2._0.Services.Database;

// Interface do SqlDatabaseService
public interface ISqlDatabaseService
{
    /// <summary>
    /// Inicializa e testa a conexão à base de dados
    /// </summary>
    /// <returns>String vazia se sucesso, mensagem de erro caso contrário</returns>
    Task<string> InitializeAsync();
    
    /// <summary>
    /// Executa SELECT sem encriptação
    /// </summary>
    Task<object> SelectRequestAsync<T>(string sqlQuery, bool isMany) where T : SqlEntity;
    
    /// <summary>
    /// Executa SELECT com desencriptação (SYMMETRIC KEY)
    /// </summary>
    Task<object> SelectRequestWithDecryptionAsync<T>(string sqlQuery, bool isMany, string? keyName, string? certificateName) where T : SqlEntity;
   
    /// <summary>
    /// Executa INSERT, UPDATE ou DELETE
    /// </summary>
    Task InsertRequestAsync(string sqlQuery);
}

public class SqlDatabaseService : ISqlDatabaseService
{
    private readonly string _connectionString;
    
    public SqlDatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AzureSqlDatabase") 
                            ?? throw new InvalidOperationException("Connection string 'AzureSqlDatabase' não encontrada no appsettings.json");
    }


    public async Task<string> InitializeAsync()
    {
        string ret = "";

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await conn.CloseAsync();

        }
        catch (SqlException e)
        {
            ret = "SQL Error:" + e.Message;
        }
        catch (InvalidOperationException o)
        {
            ret = "Invalid Operation:" + o.Message;
        }
        catch (Exception ex)
        {
            ret =  "Error:" + ex.Message;
        }

        return ret; // if empty -> sucess
    }

    // versão sem decriptação
    public Task<object> SelectRequestAsync<T>(string sqlQuery, bool isMany) where T : SqlEntity
    {
        return SelectRequestWithDecryptionAsync<T>(sqlQuery, isMany, null, null);
    }

    // versao com descriptação e async
    public async Task<object> SelectRequestWithDecryptionAsync<T>(string sqlQuery, bool isMany, string? keyName, string? certificateName) where T : SqlEntity
    {
        object? ret = null;
        
        if (!sqlQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Query deve começar com SELECT");
        }

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var command = new SqlCommand { Connection = conn };

            // Adiciona decryption se necessário
            if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(certificateName))
            {
                command.CommandText = $"OPEN SYMMETRIC KEY {keyName} DECRYPTION BY CERTIFICATE {certificateName}; ";
                command.CommandText += sqlQuery;
                command.CommandText += $"; CLOSE SYMMETRIC KEY {keyName};";
            }
            else
            {
                command.CommandText = sqlQuery;
            }

            using var reader = await command.ExecuteReaderAsync();

            if (isMany)
            {
                ret = SqlRepository.GetElements<T>(reader);
            }
            else
            {
                ret = SqlRepository.GetElement<T>(reader);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SelectRequest: {ex.Message}");
            throw;
        }

        return ret;    
    }

    public async Task InsertRequestAsync(string sqlQuery)
    {
        var upperQuery = sqlQuery.TrimStart().ToUpperInvariant();
        if (!upperQuery.StartsWith("INSERT") && 
            !upperQuery.StartsWith("UPDATE") && 
            !upperQuery.StartsWith("DELETE"))
        {
            throw new ArgumentException("Query deve começar com INSERT, UPDATE ou DELETE");
        }
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var command = new SqlCommand(sqlQuery, conn);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in InsertRequest: {ex.Message}");
            throw;
        }    
    }
}