using Microsoft.Data.SqlClient;

namespace App.Core.Database
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public bool TestConnection()
        {
            try { using var c = GetConnection(); c.Open(); return true; }
            catch { return false; }
        }

        public int ExecuteNonQuery(string sql, Action<SqlCommand>? p = null)
        {
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            p?.Invoke(cmd);
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string sql, Action<SqlCommand>? p = null)
        {
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            p?.Invoke(cmd);
            return cmd.ExecuteScalar();
        }

        public List<T> ExecuteQuery<T>(string sql, Func<SqlDataReader, T> mapper, Action<SqlCommand>? p = null)
        {
            var list = new List<T>();
            using var conn = GetConnection(); conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            p?.Invoke(cmd);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(mapper(r));
            return list;
        }

        // Async version for bonus mark
        public async Task<List<T>> ExecuteQueryAsync<T>(string sql, Func<SqlDataReader, T> mapper, Action<SqlCommand>? p = null)
        {
            var list = new List<T>();
            await using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            p?.Invoke(cmd);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(mapper(r));
            return list;
        }
    }
}
