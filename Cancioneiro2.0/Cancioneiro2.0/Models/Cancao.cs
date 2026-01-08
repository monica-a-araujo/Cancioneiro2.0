using Cancioneiro2._0.Services.Database;
using Microsoft.Data.SqlClient;

namespace Cancioneiro2._0.Models;

/// <summary>
/// Representa uma canção da base de dados
/// Esta classe MAPEIA a tabela 'cancao' do SQL Server
///
/// models = entidades/ dados
/// </summary>
public class Cancao : SqlEntity
{
    public required string Titulo { get; set; }
    public int? Bpm { get; set; } // int? -> pode ser null
    
    public Cancao() { }

    
    public override void ConvertSQLtoObject(SqlDataReader reader)
    {
        Id = reader.GetInt32(reader.GetOrdinal("id"));
        
        Titulo = reader["titulo"] as string ?? string.Empty;
        Bpm = reader["bpm"] as byte?;
    }

    // Insert titulo de canção na base de dados
    //TODO: proteger contra SQL Injection
    public override string ConvertObjectToSQLInsertQuery()
    {

        return $"INSERT INTO cancao (titulo, bpm) " +
               $"VALUES ('{Titulo}', {Bpm?.ToString() ?? "NULL"})";
    }

    //update function
    public override string ConvertObjectToSQLUpdateQuery()
    {
        return $"UPDATE cancao " +
               $"SET titulo='{Titulo}', bpm={Bpm?.ToString() ?? "NULL"} " +
               $"WHERE id={Id}";
    }
}
