using Microsoft.Data.SqlClient;
namespace Cancioneiro2._0.Services.Database;

public abstract class SqlEntity
{
    public long Id { get; set; }

    public SqlEntity() { }
        
    public abstract void ConvertSQLtoObject(SqlDataReader reader);
    public abstract string ConvertObjectToSQLInsertQuery();
    public abstract string ConvertObjectToSQLUpdateQuery();
}