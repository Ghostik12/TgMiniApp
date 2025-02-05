using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace TeachBot
{
    internal class WordManager
    {
        private static string _connectionString = "Data Source=bot.db";

        public static void AddWord(string language, string word, string translation)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Words (Language, Word, Translation) VALUES ($language, $word, $translation)";
                command.Parameters.AddWithValue("$language", language);
                command.Parameters.AddWithValue("$word", word);
                command.Parameters.AddWithValue("$translation", translation);
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveWord(int wordId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Words WHERE Id = $wordId";
                command.Parameters.AddWithValue("$wordId", wordId);
                command.ExecuteNonQuery();
            }
        }

        public static List<Word> GetWords(string language)
        {
            var words = new List<Word>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Words WHERE Language = $language";
                command.Parameters.AddWithValue("$language", language);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        words.Add(new Word
                        {
                            Id = reader.GetInt32(0),
                            Language = reader.GetString(1),
                            Words = reader.GetString(2),
                            Translation = reader.GetString(3)
                        });
                    }
                }
            }
            return words;
        }

        internal static int FindWordId(string selectedLanguage, string wordText)
        {
            var word = new Word();
            using(var  connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Words WHERE Word = $wordText";
                command.Parameters.AddWithValue($"wordText", wordText);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        word.Id = reader.GetInt32(0);
                    }
                }
            }
            return word.Id;
        }
    }
}
