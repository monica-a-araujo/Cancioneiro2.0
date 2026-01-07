using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Cancioneiro2._0.Services.Database;

// Interface do SqlDatabaseService
public interface ISqlDatabaseService
{
    Task<string> InitializeAsync();
    Task<object> SelectRequestAsync<T>(string sqlQuery, bool isMany) where T : SqlEntity;
    Task<object> SelectRequestWithDecryptionAsync<T>(string sqlQuery, bool isMany, string keyName, string certificateName) where T : SqlEntity;
    Task InsertRequestAsync(string sqlQuery);
}

public class SqlDatabaseService : ISqlDatabaseService
{
    private readonly string _connectionString;
    
    public SqlDatabaseService(IConfiguration configuration)
    {
        // Lê connection string do appsettings.json
        _connectionString = configuration.GetConnectionString("AzureSqlDatabase");
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
            ret = e.Message;
        }
        catch (InvalidOperationException o)
        {
            ret = o.Message;
        }
        catch (Exception ex)
        {
            ret = ex.Message;
        }

        return ret;
    }

    // versão sem decriptação
    public Task<object> SelectRequestAsync<T>(string sqlQuery, bool isMany) where T : SqlEntity
    {
        return SelectRequestWithDecryptionAsync<T>(sqlQuery, isMany, null, null);
    }

    // versao com descriptação e async
    public async Task<object> SelectRequestWithDecryptionAsync<T>(string sqlQuery, bool isMany, string keyName, string certificateName) where T : SqlEntity
    {
        object ret = null;

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

        return ret;    }

    public async Task InsertRequestAsync(string sqlQuery)
    {
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
        }    }
}