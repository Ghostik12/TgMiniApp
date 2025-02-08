using LanguageBot.DB;
using LanguageBot.Games;
using LanguageBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static LanguageBot.Games.HangmanGame;

namespace LanguageBot.Controller
{
    public class TextMessageController
    {
        private ITelegramBotClient _botClient { get; set; }

        public TextMessageController(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task StartRegistration(Update update)
        {
            RegistrationStates.States[update.Message.Chat.Id] = new RegistrationState
            {
                ChatId = update.Message.Chat.Id,
                CurrentStep = "FirstName",
                UserData = new Models.User { ChatId = update.Message.Chat.Id, Username = update.Message.From.Username }
            };

            await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Введите ваше имя:");
        }


        public async Task HandleRegistrationStep(long chatId, string text)
        {
            var state = RegistrationStates.States?[chatId];
            switch (state.CurrentStep)
            {
                case "FirstName":
                    state.UserData.FirstName = text;
                    state.CurrentStep = "LastName";
                    await _botClient.SendTextMessageAsync(chatId, "Введите вашу фамилию:");
                    break;

                case "LastName":
                    state.UserData.LastName = text;
                    state.CurrentStep = "Age";
                    await _botClient.SendTextMessageAsync(chatId, "Введите ваш возраст:");
                    break;

                case "Age":
                    if (int.TryParse(text, out int age))
                    {
                        state.UserData.Age = age;
                        state.CurrentStep = "Language";
                        await _botClient.SendTextMessageAsync(chatId, "Выберите язык (английский, русский, французский):");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Пожалуйста, введите корректный возраст.");
                    }
                    break;

                case "Language":
                    state.UserData.Language = text;
                    state.CurrentStep = "Level";
                    await _botClient.SendTextMessageAsync(chatId, "Выберите ваш уровень (A1, A2, B1, B2, C1, C2):");
                    break;

                case "Level":
                    state.UserData.Level = text;
                    await CompleteRegistration(chatId);
                    break;
            }
        }

        private async Task CompleteRegistration(long chatId)
        {
            var state = RegistrationStates.States[chatId];

            using var db = new AppDbContext();
            db.Users.Add(state.UserData);
            await db.SaveChangesAsync();

            RegistrationStates.States.Remove(chatId);

            await _botClient.SendTextMessageAsync(chatId, "Регистрация завершена! Добро пожаловать в LinguaBot!");
            SendMainMenu(chatId);
        }

        internal async Task Handle(Update update, CancellationToken cancellationToken)
        {
            var messageText = update.Message.Text;
            // Обработка команд
            switch (messageText)
            {
                case "📖 Учить" or "Учить" or "учить":
                    await SendWordToLearn(update.Message.Chat.Id);
                    break;

                case "🎯 Игра" or "игра" or "игры":
                    await SendGame(update.Message.Chat.Id);
                    break;

                case "📊 Статистика" or "Статистика" or "статистика":
                    await SendStatistics(update.Message.Chat.Id);
                    break;

                case "🎉 Достижения" or "Достижения" or "достижения":
                    await SendAchivement(update.Message.Chat.Id);
                    break;

                case "⚙ Настройки":
                    await SendSettings(update.Message.Chat.Id);
                    break;

                case "ℹ Помощь" or "/help" or "Помощь":
                    await SendHelp(update.Message.Chat.Id);
                    break;

                default:
                    // Обработка угадывания буквы
                    if (messageText.Length == 1 && char.IsLetter(messageText[0]))
                    {
                        await HandleHangmanGuess(update.Message.Chat.Id, messageText[0]);
                        return;
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "Неизвестная команда. Используй /start для начала.",
                            cancellationToken: cancellationToken);
                    }
                    break;
            }
        }

        private async Task SendSettings(long chatId)
        {
            return;
        }

        private async Task SendAchivement(long chatId)
        {
            using var db = new AppDbContext();
            var userAchiv = await db.Achievements.FirstOrDefaultAsync(c => c.UserId == chatId);

            if (userAchiv != null)
            {
                await _botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: $"Достижения: {userAchiv.Name}");
            }
        }

        private async Task SendStatistics(long chatId)
        {
            using var db = new AppDbContext();
            var userL = await db.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);
            var userAch = await db.Achievements.FirstOrDefaultAsync(c => c.UserId == chatId);

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Статистика {userL.Username}\nОпыт: {userL.XP}\nДостижения: {userL.Achievements.Count}");
        }

        private async Task SendGame(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Виселица", "hangman"),
            }
        });
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Игры:",
                replyMarkup: inlineKeyboard);
        }

        private async Task SendWordToLearn(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Базовая грамматика", "gram"),
                InlineKeyboardButton.WithCallbackData("Фонетика и произношение", "fonet"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Чтение и аудирование", "readlis"),
                InlineKeyboardButton.WithCallbackData("Разговорная практика", "talk"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Сленг и идиомы", "slang"),
                InlineKeyboardButton.WithCallbackData("Подготовка к экзаменам", "preparation")
            }
        });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Модули:",
                replyMarkup: inlineKeyboard);
        }

        private async Task SendHelp(long chatId)
        {
            _botClient.SendTextMessageAsync(chatId, "По всем возникшим вопросам можете обащаться к администратору");
        }

        private async Task SendMainMenu(long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📖 Учить") },
                new[] { new KeyboardButton("🎯 Игра"), new KeyboardButton("📊 Статистика") },new[] { new KeyboardButton("🎉 Достижения") },
                new[] { new KeyboardButton("⚙ Настройки"), new KeyboardButton("ℹ Помощь") }
            })
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Главное меню:",
                replyMarkup: replyKeyboard);
        }

        public async Task HandleCallBack(CallbackQuery? callbackQuery, ITelegramBotClient botClient, Update update)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var callbackData = callbackQuery.Data;
            var userName = callbackQuery.From.Username;

            Console.WriteLine($"Обработка CallbackQuery: {callbackData}");

            if (callbackData == "hangman") 
            {
                await StartHangmanGame(chatId);
            }
            else if(callbackData == "gram" || callbackData == "fonet" || callbackData == "readlis" || callbackData == "talk" || callbackData == "slang" || callbackData == "preparation")
            {

                //using var db = new AppDbContext();
                //var userL = await db.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);
                //var language = userL.Language;
                //var level = userL.Level;
                //var lesson = await db.Lessons
                //    .FirstOrDefaultAsync(l => l.Language == language && l.Level == level);
                var lesson = 1;

                if (lesson != null)
                {
                    //await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: {lesson.Title}\n{lesson.Description}");
                    //await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: {lesson.VideoUrl}");
                    //await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: {lesson.AudioUrl}");

                    await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: Здесь будет название темы\nОписание");
                    await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: Будет ссылка");
                    await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: Будет ссылка");
                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId, "Уроки для вашего уровня пока недоступны.");
                }
            }
        }

        private async Task HandleHangmanGuess(long chatId, char letter)
        {
            if (!HangmanGames.Games.ContainsKey(chatId))
            {
                await _botClient.SendTextMessageAsync(chatId, "Игра не начата. Введите /hangman, чтобы начать.");
                return;
            }

            var game = HangmanGames.Games[chatId];
            var result = game.Guess(letter);

            await _botClient.SendTextMessageAsync(chatId, result);

            if (result.Contains("Поздравляем") || result.Contains("Попытки закончились"))
            {
                if (result.Contains("Поздравляем"))
                {
                    using var db = new AppDbContext();
                    var userL = await db.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);
                    var userAchiv = await db.Achievements.FirstOrDefaultAsync(c => c.UserId == chatId);
                    userL.XP += 10;
                    db.Users.Update(userL);
                    if (userL.Achievements.Count == 0)
                    {
                        var achiv = new Achievement()
                        {
                            Name = "Первая виселица!",
                            DateEarned = DateTime.UtcNow,
                            UserId = chatId,
                        };
                        db.Achievements.Update(achiv);
                        await db.SaveChangesAsync();
                        await _botClient.SendTextMessageAsync(chatId, "🎉 Поздравляем! Вы получили достижение: Первая виселица!");
                    }
                    await db.SaveChangesAsync();
                }
                HangmanGames.Games.Remove(chatId);
                SendMainMenu(chatId);
            }
        }
        public async Task StartHangmanGame(long chatId)
        {
            var words = new List<string> { "COMPUTER", "PROGRAMMING", "TELEGRAM", "BOT", "INTERNET" };
            var random = new Random();
            var word = words[random.Next(words.Count)];

            HangmanGames.Games[chatId] = new HangmanGame(word, _botClient);

            await _botClient.SendTextMessageAsync(chatId, $"Игра началась! Угадайте слово: {HangmanGames.Games[chatId].GetCurrentState()}\nИз {word.Length} букв");
        }

        public class RegistrationStates
        {
            public static Dictionary<long, RegistrationState> States = new();
        }
    }
}
