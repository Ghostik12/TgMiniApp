using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachBot
{
    internal class AdminManager
    {
        private static string _connectionString = "Data Source=bot.db";

        public static void AddAdmin(long chatId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Admins (ChatId) VALUES ($chatId)";
                command.Parameters.AddWithValue("$chatId", chatId);
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveAdmin(long chatId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Admins WHERE ChatId = $chatId";
                command.Parameters.AddWithValue("$chatId", chatId);
                command.ExecuteNonQuery();
            }
        }

        public static bool IsAdmin(long chatId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Admins WHERE ChatId = $chatId";
                command.Parameters.AddWithValue("$chatId", chatId);
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
        }
    }
}
