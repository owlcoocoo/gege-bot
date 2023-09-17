using Microsoft.Data.Sqlite;

namespace GegeBot.Db
{
    public class Sqlite : IDb
    {
        readonly string _connectionString;
        readonly string _tableName;

        public Sqlite(string dataSource, string tableName)
        {
            _connectionString = $"Data Source={dataSource}";
            _tableName = tableName;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @$"
                CREATE TABLE IF NOT EXISTS ""{tableName}"" (
                  ""key"" varchar(512) not null,
                  ""value"" text,
                  ""updated_at"" datetime not null default(datetime(CURRENT_TIMESTAMP, 'localtime')),
                  PRIMARY KEY (""key"")
                );
            ";
            command.ExecuteNonQuery();
        }

        public string GetValue(string key)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @$"
                SELECT value
                FROM {_tableName}
                WHERE key = $key
            ";
            command.Parameters.AddWithValue("$key", key);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var value = reader.GetString(0);
                return value;
            }

            return "";
        }

        public bool SetValue(string key, string value)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @$"
                INSERT OR REPLACE INTO {_tableName}(key, value)
                VALUES($key, $value);
            ";
            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$value", value);

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteKey(string key)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @$"
                DELETE FROM {_tableName}
                WHERE key = $key;
            ";
            command.Parameters.AddWithValue("$key", key);

            return command.ExecuteNonQuery() > 0;
        }
    }
}
