using Microsoft.Data.Sqlite;
using BlazorSignalRApp.Models;

namespace BlazorSignalRApp.Services
{
    public class ChatStorageService
    {
        private readonly string _connectionString = "Data Source=chat.db";

        public ChatStorageService()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Author TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                );
            ";
            tableCmd.ExecuteNonQuery();
        }

        public void SaveMessage(Message message)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
                INSERT INTO Messages (Author, Content)
                VALUES ($author, $content);
            ";
            cmd.Parameters.AddWithValue("$author", message.Author);
            cmd.Parameters.AddWithValue("$content", message.Content);
            cmd.ExecuteNonQuery();
        }

        public List<Message> GetAllMessages()
        {
            var messages = new List<Message>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Author, Content, Timestamp FROM Messages ORDER BY Timestamp";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(new Message
                {
                    Author = reader.GetString(0),
                    Content = reader.GetString(1),
                    Timestamp = reader.GetDateTime(2)
                });
            }

            return messages;
        }
    }
}
