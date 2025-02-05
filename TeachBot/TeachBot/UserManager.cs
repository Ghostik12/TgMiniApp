using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;

namespace TeachBot
{
    internal class UserManager
    {
        private static string _connectionString = "Data Source=bot.db";

        public static List<User> GetUsers()
        {
            var users = new List<User>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            ChatId = reader.GetInt64(1),
                            Username = reader.IsDBNull(2) ? null : reader.GetString(2),
                            SelectedLanguage = reader.GetString(3)
                        });
                    }
                }
            }
            return users;
        }

        internal static User GetUserByChatId(long chatId)
        {
            var user = new User();
            using(var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users WHERE ChatId = $chatId";
                command.Parameters.AddWithValue($"chatId", chatId);
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        user.Id = reader.GetInt32(0);
                        user.ChatId = reader.GetInt64(1);
                        user.Username = reader.GetString(2);
                        user.SelectedLanguage = reader.GetString(3);
                    }
                }
            }
            return user;
        }

        internal static User AddUser(long chatId)
        {
            var user = new User();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users WHERE ChatId = $chatId";
                command.Parameters.AddWithValue($"chatId", chatId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user.Id = reader.GetInt32(0);
                        user.ChatId = reader.GetInt64(1);
                        user.Username = reader.GetString(2);
                        user.SelectedLanguage = reader.GetString(3);
                    }
                }
            }
            return user;
        }
    }
}
