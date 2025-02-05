using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachBot
{
    internal class FavoriteManager
    {
        private static string _connectionString = "Data Source=bot.db";

        public static void AddFavorite(long userId, int wordId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Favorites (UserId, WordId) VALUES ($userId, $wordId)";
                command.Parameters.AddWithValue("$userId", userId);
                command.Parameters.AddWithValue("$wordId", wordId);
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveFavorite(int userId, int wordId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Favorites WHERE UserId = $userId AND WordId = $wordId";
                command.Parameters.AddWithValue("$userId", userId);
                command.Parameters.AddWithValue("$wordId", wordId);
                command.ExecuteNonQuery();
            }
        }

        public static List<Word> GetFavorites(long userId)
        {
            var favorites = new List<Word>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT w.Id, w.Language, w.Word, w.Translation 
                FROM Favorites f
                JOIN Words w ON f.WordId = w.Id
                WHERE f.UserId = $userId";
                command.Parameters.AddWithValue("$userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        favorites.Add(new Word
                        {
                            Id = reader.GetInt32(0),
                            Language = reader.GetString(1),
                            Words = reader.GetString(2),
                            Translation = reader.GetString(3)
                        });
                    }
                }
            }
            return favorites;
        }
    }
}
