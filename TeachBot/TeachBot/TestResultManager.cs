using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachBot
{
    internal class TestResultManager
    {
        private static string _connectionString = "Data Source=bot.db";

        public static void SaveTestResult(int userId, int correctAnswers, int totalQuestions)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO TestResults (UserId, CorrectAnswers, TotalQuestions, TestDate)
                VALUES ($userId, $correctAnswers, $totalQuestions, $testDate)";
                command.Parameters.AddWithValue("$userId", userId);
                command.Parameters.AddWithValue("$correctAnswers", correctAnswers);
                command.Parameters.AddWithValue("$totalQuestions", totalQuestions);
                command.Parameters.AddWithValue("$testDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
        }

        public static List<TestResult> GetTestResults(int userId)
        {
            var results = new List<TestResult>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM TestResults WHERE UserId = $userId";
                command.Parameters.AddWithValue("$userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new TestResult
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            CorrectAnswers = reader.GetInt32(2),
                            TotalQuestions = reader.GetInt32(3),
                            TestDate = reader.GetString(4)
                        });
                    }
                }
            }
            return results;
        }
    }
}
