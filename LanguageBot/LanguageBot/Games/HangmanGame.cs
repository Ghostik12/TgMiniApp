using System.Text;
using Telegram.Bot;

namespace LanguageBot.Games
{
    public class HangmanGame
    {
        private string _word; // Загаданное слово
        private StringBuilder _guessedWord; // Текущее состояние угаданного слова
        private int _attemptsLeft; // Оставшиеся попытки
        private List<char> _guessedLetters; // Уже названные буквы
        private ITelegramBotClient _botClient;

        public HangmanGame(string word, ITelegramBotClient botClient)
        {
            _word = word.ToUpper();
            _guessedWord = new StringBuilder(new string('_', word.Length));
            _attemptsLeft = 6; // Обычно в виселице 6 попыток
            _guessedLetters = new List<char>();
            _botClient = botClient;
        }

        // Метод для угадывания буквы
        public string Guess(char letter)
        {
            letter = char.ToUpper(letter);

            if (_guessedLetters.Contains(letter))
            {
                return "Вы уже называли эту букву.";
            }

            _guessedLetters.Add(letter);

            if (_word.Contains(letter))
            {
                for (int i = 0; i < _word.Length; i++)
                {
                    if (_word[i] == letter)
                    {
                        _guessedWord[i] = letter;
                    }
                }

                if (_guessedWord.ToString() == _word)
                {
                    return $"Поздравляем! Вы угадали слово: {_word}";
                }

                return $"Правильно! Текущее состояние: {_guessedWord}";
            }
            else
            {
                _attemptsLeft--;
                if (_attemptsLeft == 0)
                {
                    return $"Неверно! Попытки закончились. Загаданное слово: {_word}";
                }
                return $"Неверно! Осталось попыток: {_attemptsLeft}. Текущее состояние: {_guessedWord}";
            }
        }

        // Метод для получения текущего состояния игры
        public string GetCurrentState()
        {
            return $"Осталось попыток: {_attemptsLeft}";
        }

        public static class HangmanGames
        {
            public static Dictionary<long, HangmanGame> Games = new();
        }
    }
}
