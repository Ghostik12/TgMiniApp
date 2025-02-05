using Microsoft.Data.Sqlite;

namespace TeachBot.DB
{
    internal class DataBase
    {
        private string _connectionString = "Data Source=bot.db";

        public void Initialize()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Создание таблицы пользователей
                var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ChatId INTEGER NOT NULL UNIQUE,
                    Username TEXT,
                    SelectedLanguage TEXT
                )";
                command.ExecuteNonQuery();

                // Создание таблицы администраторов
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Admins (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ChatId INTEGER NOT NULL UNIQUE
                )";
                command.ExecuteNonQuery();

                // Создание таблицы слов
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Words (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Language TEXT NOT NULL,
                    Word TEXT NOT NULL,
                    Translation TEXT NOT NULL
                )";
                command.ExecuteNonQuery();

                // Создание таблицы прогресса пользователей
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserProgress (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    WordId INTEGER NOT NULL,
                    IsLearned INTEGER DEFAULT 0,
                    FOREIGN KEY (UserId) REFERENCES Users (Id),
                    FOREIGN KEY (WordId) REFERENCES Words (Id)
                )";
                command.ExecuteNonQuery();

                // Создание таблицы избранных слов
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Favorites (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                WordId INTEGER NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users (Id),
                FOREIGN KEY (WordId) REFERENCES Words (Id)
            )";
                command.ExecuteNonQuery();

                // Создание таблицы результатов тестов
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS TestResults (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                CorrectAnswers INTEGER NOT NULL,
                TotalQuestions INTEGER NOT NULL,
                TestDate TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users (Id)
            )";
                command.ExecuteNonQuery();
            }
        }
    }
}
