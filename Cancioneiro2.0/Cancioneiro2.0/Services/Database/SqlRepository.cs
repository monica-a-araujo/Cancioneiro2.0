using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Cancioneiro2._0.Services.Database;

public class SqlRepository
{
    public static List<T> GetElements<T>(SqlDataReader reader) where T : SqlEntity
    {
        List<T> ret = new List<T>();

        while (reader.Read())
        {
            T? element = (T)Activator.CreateInstance(typeof(T)) as T;
            
            if (element != null)
            {
                element.ConvertSQLtoObject(reader);
                ret.Add(element);
            }
        }

        return ret;
    }

    public static T? GetElement<T>(SqlDataReader reader) where T : SqlEntity
    {
        T? ret = default(T);

        if (reader.Read())
        {
            ret = (T)Activator.CreateInstance(typeof(T));
            if (ret != null)
            {
                ret.ConvertSQLtoObject(reader);
            }        }

        return ret;
    }
}