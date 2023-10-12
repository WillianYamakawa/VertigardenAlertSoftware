using MySqlConnector;
using DotNetEnv;

namespace Project.Lib.Database;

public static class MySQLExtensions{
    public static T? Get<T>(this MySqlDataReader reader, int index, T? defaultValue = default){
        object result = reader[index];
        return Convert.IsDBNull(result) ? defaultValue : (T)result;
    }
}

public interface IDataBaseContext{
    bool ExecuteNonQuery(string query, params KeyValuePair<string, object?>[] parameters);
    MySqlDataReader? Execute(string query, params KeyValuePair<string, object?>[] parameters);

    ulong GetLastInsertedId();
}

public class MySQLContext : IDataBaseContext, IDisposable{
    private MySqlConnection? _connection;

    public MySQLContext()
    {
        _connection = new($"Server={Env.GetString("DB_HOST", string.Empty)};Port={Env.GetString("DB_PORT", string.Empty)};Database={Env.GetString("DB_DATABASE", string.Empty)};Uid={Env.GetString("DB_LOGIN", string.Empty)};Pwd={Env.GetString("DB_PASSWORD", string.Empty)};");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection = null;
    }

    public MySqlDataReader? Execute(string query, params KeyValuePair<string, object?>[] parameters)
    {
        if(_connection == null) return null;
        using MySqlCommand command = new(query, _connection);
        foreach(var e in parameters){
            command.Parameters.AddWithValue(e.Key, e.Value);
        }
        try
        {
            MySqlDataReader reader = command.ExecuteReader();
            return reader;
        }
        catch (Exception e){ Console.WriteLine(e); return null; }
    }

    public bool ExecuteNonQuery(string query, params KeyValuePair<string, object?>[] parameters)
    {
        if(_connection == null) return false;
        using MySqlCommand command = new(query, _connection);
        foreach(var e in parameters){
            command.Parameters.AddWithValue(e.Key, e.Value);
        }
        try { command.ExecuteNonQuery(); }
        catch { return false; }
        return true;
    }

    public ulong GetLastInsertedId()
    {
        var result = Execute("SELECT LAST_INSERT_ID() as id");
        if(result == null) return 0;
        result.Read();
        return result.GetUInt64("id");
    }
}