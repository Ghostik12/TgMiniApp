using LanguageBot.DB;
using LanguageBot.Games;
using LanguageBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static LanguageBot.Games.HangmanGame;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Update = Telegram.Bot.Types.Update;

namespace LanguageBot.Controller
{
    public class TextMessageController
    {
        private ITelegramBotClient _botClient { get; set; }
        private string[] validLevels = new[] { "A1", "A2", "B1", "B2", "C1", "C2" };
        private int pageNumber = 1;
        private int pageSize = 1;

        public TextMessageController(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task StartRegistration(Update update)//Старт регистрации
        {
            using var db = new AppDbContext();
            var user = await db.Users.FirstOrDefaultAsync(c => c.ChatId == update.Message.Chat.Id);
            if (user != null) //Проверка что пользователь не зарегистрирован
            {
                await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Вы уже зарегистрированны, если хотите что-то исправить заходите в настройки😊");
                await SendMainMenu(update.Message.Chat.Id);
                return;
            }
            RegistrationStates.States[update.Message.Chat.Id] = new RegistrationState
            {
                ChatId = update.Message.Chat.Id,
                CurrentStep = "FirstName",
                UserData = new Models.User { ChatId = update.Message.Chat.Id, Username = update.Message.From.Username ?? "Unknown" }
            };

            await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Для регистрации введите данные как указано далее: Имя Фамилия Возраст Язык (Английский, Русский, Французкий) Уровень" +
                "(A1, A2, B1, B2, C1, C2)");
        }


        public async Task HandleRegistrationStep(long chatId, string text)//Следующий шаг регистрации
        {
            var state = RegistrationStates.States?[chatId];
            switch (state.CurrentStep)
            {
                case "FirstName":
                    var parts = text.Split(' ');
                    if (parts.Length < 5)//Проверка на введенного формата
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Недостаточно данных. Формат: Имя Фамилия Возраст Язык Уровень");
                        return;
                    }

                    if (!int.TryParse(parts[2], out var age))//Проверка что возраст введен цифрами
                    {
                        await _botClient.SendTextMessageAsync(chatId,"Возраст должен быть числом.");
                        return;
                    }
                    // Извлекаем данные
                    state.UserData.FirstName = parts[0];
                    state.UserData.LastName = parts[1];
                    state.UserData.Age = int.Parse(parts[2]); // Преобразуем возраст в число
                    if (parts[3] == "en" || parts[3] == "английский")
                        state.UserData.Language = "Английский";
                    else if (parts[3] == "fr" || parts[3] == "французский")
                        state.UserData.Language = "Французский";
                    else if (parts[3] == "ru" || parts[3] == "русский")
                        state.UserData.Language = "Русский";
                    else
                        state.UserData.Language = parts[3];
                    if (!validLevels.Contains(parts[4]))
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Недопустимый уровень. Допустимые уровни: A1, A2, B1, B2, C1, C2");
                        return;
                    }
                    state.UserData.Level = parts[4];

                    await CompleteRegistration(chatId);
                    break;
                case "TwoName":
                    state.UserData.FirstName = text;
                    await CompleteSettings(chatId);
                    return;
                case "LastName":
                    state.UserData.LastName = text;
                    await CompleteSettings(chatId);
                    return;
                case "Age":
                    if (!int.TryParse(text, out var ages))//Проверка что возраст введен цифрами
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Возраст должен быть числом.");
                        return;
                    }
                    state.UserData.Age = int.Parse(text);
                    await CompleteSettings(chatId);
                    return;
                case "Language":
                    state.UserData.Language = text;
                    await CompleteSettings(chatId);
                    return;
            }
        }

        private async Task CompleteRegistration(long chatId)//Окончание регистрации
        {
            var state = RegistrationStates.States[chatId];

            using var db = new AppDbContext();
            db.Users.Add(state.UserData);
            await db.SaveChangesAsync();

            RegistrationStates.States.Remove(chatId);

            await _botClient.SendTextMessageAsync(chatId, "Регистрация завершена! Добро пожаловать в LinguaBot!");
            await SendMainMenu(chatId);
        }

        private async Task CompleteSettings(long chatId)
        {
            var state = RegistrationStates.States[chatId];

            using var db = new AppDbContext();
            var user = await db.Users.FirstOrDefaultAsync(x => x.ChatId == chatId);
            user.FirstName = state.UserData.FirstName ?? user.FirstName;
            user.LastName = state.UserData.LastName ?? user.LastName;
            if (state.UserData.Age != 0)
                user.Age = state.UserData.Age;
            else
                user.Age = user.Age;
            if (state.UserData.Language == "en" || state.UserData.Language == "английский")
                user.Language = "Английский";
            else if (state.UserData.Language == "fr" || state.UserData.Language == "французский")
                user.Language = "Французский";
            else if (state.UserData.Language == "ru" || state.UserData.Language == "русский")
                user.Language = "Русский";
            else if (!string.IsNullOrWhiteSpace(state.UserData.Language))
                user.Language = state.UserData.Language;
            else
                user.Language = user.Language;

            db.Users.Update(user);
            await db.SaveChangesAsync();

            RegistrationStates.States.Remove(chatId);

            await _botClient.SendTextMessageAsync(chatId, "Изменения внесены!");
            await SendMainMenu(chatId);
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

                case "🎯 Игра" or "игра" or "игры" or "Игра":
                    await SendGame(update.Message.Chat.Id);
                    break;

                case "📊 Статистика" or "Статистика" or "статистика":
                    await SendStatistics(update.Message.Chat.Id);
                    break;

                case "🎉 Достижения" or "Достижения" or "достижения":
                    await SendAchivement(update.Message.Chat.Id);
                    break;

                case "⚙ Настройки" or "Настройки" or "настройки":
                    await SendSettings(update.Message.Chat.Id);
                    break;

                case "ℹ Помощь" or "/help" or "Помощь" or "помощь":
                    await SendHelp(update.Message.Chat.Id);
                    break;
                case "🏆 Рейтинг 🏆" or "Рейтинг" or "рейтинг" or "рейт":
                    var ranking = await SendRating(update.Message.Chat.Id, pageNumber);
                    await _botClient.SendTextMessageAsync(update.Message.Chat.Id, ranking.Ranking, replyMarkup: ranking.Keyboard);
                    await SendMainMenu(update.Message.Chat.Id);
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

        private async Task<(string Ranking, InlineKeyboardMarkup Keyboard)> SendRating(long chatId, int pageNumber)
        {
            using var db = new AppDbContext();
            var userM = await db.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);

            // Получаем всех пользователей с указанным уровнем
            var users = await db.Users
                .Where(u => u.Level == userM.Level) // Фильтруем по уровню
                .OrderByDescending(u => u.XP) // Сортируем по XP
                .Select(u => new
                {
                    u.FirstName,
                    u.XP
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(users.Count / (double)pageSize);

            if (pageNumber < 1 || pageNumber > totalPages)
            {
                return ("Недопустимый номер страницы.", null);
            }

            var pagedUsers = users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Формируем текстовое представление рейтинга
            var result = new StringBuilder($"🏆 Рейтинг пользователей уровня {userM.Level}:\n");
            result.AppendLine($"\n📊 Уровень - {userM.Level}");
            for (int i = 0; i < pagedUsers.Count; i++)
            {
                var user = pagedUsers[i];
                var rank = (pageNumber - 1) * pageSize + i + 1; // Общий ранг
                result.AppendLine($"👤 {user.FirstName} - {user.XP} XP (Ранг: {rank})");
            }

            var keyboard = new List<InlineKeyboardButton[]>();

            if (pageNumber > 1)
            {
                keyboard.Add(new[]
                {
            InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"rating_{userM.Level}_{pageNumber - 1}")
        });
            }

            if (pageNumber < totalPages)
            {
                keyboard.Add(new[]
                {
            InlineKeyboardButton.WithCallbackData("Вперед ➡️", $"rating_{userM.Level}_{pageNumber + 1}")
        });
            }

            var inlineKeyboard = new InlineKeyboardMarkup(keyboard);

            return (result.ToString(), inlineKeyboard);
        }

        private async Task SendSettings(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Редактировать имя", "firstName"),
                InlineKeyboardButton.WithCallbackData("Редактировать фамилию", "lastName"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Поменять возраст", "age"),
                //InlineKeyboardButton.WithCallbackData("Поменять язык", "language"),
            }
        });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "⚙ Настройки:",
                replyMarkup: inlineKeyboard);
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
                text: "🎯 Игры:",
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
                text: "📒 Модули:",
                replyMarkup: inlineKeyboard);
        }

        private async Task SendHelp(long chatId)
        {
            await _botClient.SendTextMessageAsync(chatId, "По всем возникшим вопросам можете обащаться к администратору");
        }

        private async Task SendMainMenu(long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📖 Учить") },
                new[] { new KeyboardButton("🎯 Игра"), new KeyboardButton("📊 Статистика") },new[] { new KeyboardButton("🎉 Достижения") },
                new[] { new KeyboardButton("⚙ Настройки"), new KeyboardButton("ℹ Помощь") }, new[] { new KeyboardButton("🏆 Рейтинг 🏆") }
            })
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "🗄Главное меню:",
                replyMarkup: replyKeyboard);
        }

        public async Task HandleCallBack(CallbackQuery? callbackQuery, ITelegramBotClient botClient, Update update)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var callbackData = callbackQuery.Data;
            //var userName = callbackQuery.From.Username;
            using AppDbContext db = new();
            var userG = await db.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);
            var language = userG.Language;
            var level = userG.Level;
            var parts = callbackData.Split('_');
            if (parts.Length == 3 && parts[0] == "rating") 
            {
                var levelPart = parts[1];
                var pageNumber = int.Parse(parts[2]);
                var (ranking, keyboard) = await SendRating(chatId, pageNumber);// Получаем рейтинг по уровню с пагинацией
                await _botClient.EditMessageTextAsync(// Редактируем сообщение с новым рейтингом и кнопками
                    chatId,
                    callbackQuery.Message.MessageId,
                    ranking,
                    replyMarkup: keyboard);
            }

                Console.WriteLine($"Обработка CallbackQuery: {callbackData}");

            switch (callbackData)
            {
                case "hangman":
                    await StartHangmanGame(chatId);
                    return;
                case "gram":
                    var lesson = await db.Lessons
                        .FirstOrDefaultAsync(l => l.Title == "Базовая грамматика");

                    if (lesson != null)
                    {
                        await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: {lesson.Title}\nОписание:{lesson.Description}");
                        await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: {lesson.VideoUrl}");
                        //await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: {lesson.AudioUrl}");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Уроки для вашего уровня пока недоступны.");
                    }
                    return;
                case "fonet":
                    var lesson1 = await db.Lessons
                        .FirstOrDefaultAsync(l => l.Title == "Базовая грамматика");

                    if (lesson1 != null)
                    {
                        await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: {lesson1.Title}\nОписание:{lesson1.Description}");
                        await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: {lesson1.VideoUrl}");
                        //await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: {lesson.AudioUrl}");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Уроки для вашего уровня пока недоступны.");
                    }
                    return;
                case "readlis":
                    var lesson2 = await db.Lessons
                    .FirstOrDefaultAsync(l => l.Title == "Чтение и аудирование");

                    if (lesson2 != null)
                    {
                        await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: {lesson2.Title}\nОписание:{lesson2.Description}");
                        await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: {lesson2.VideoUrl}");
                        //await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: {lesson.AudioUrl}");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Уроки для вашего уровня пока недоступны.");
                    }
                    return;
                case "talk":
                    var lesson3 = await db.Lessons
                    .FirstOrDefaultAsync(l => l.Title == "Разговорная практика");

                    if (lesson3 != null)
                    {
                        await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: {lesson3.Title}\nОписание:{lesson3.Description}");
                        await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: {lesson3.VideoUrl}");
                        //await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: {lesson.AudioUrl}");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Уроки для вашего уровня пока недоступны.");
                    }
                    return;
                case "slang":
                    var lesson4 = await db.Lessons
                    .FirstOrDefaultAsync(l => l.Title == "Сленг и идиомы");

                    if (lesson4 != null)
                    {
                        await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: {lesson4.Title}\nОписание:{lesson4.Description}");
                        await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: {lesson4.VideoUrl}");
                        //await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: {lesson.AudioUrl}");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Уроки для вашего уровня пока недоступны.");
                    }
                    return;
                case "preparation":
                    var lesson5 = await db.Lessons
                    .FirstOrDefaultAsync(l => l.Title == "Сленг и идиомы");

                    if (lesson5 != null)
                    {
                        await _botClient.SendTextMessageAsync(chatId, $"📚 Урок: {lesson5.Title}\nОписание:{lesson5.Description}");
                        await _botClient.SendTextMessageAsync(chatId, $"🎥 Видео: {lesson5.VideoUrl}");
                        //await _botClient.SendTextMessageAsync(chatId, $"🎧 Аудио: {lesson.AudioUrl}");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Уроки для вашего уровня пока недоступны.");
                    }
                    return;
                case "firstName":
                    RegistrationStates.States[chatId] = new RegistrationState
                    {
                        ChatId = chatId,
                        CurrentStep = "TwoName",
                        UserData = new Models.User {}
                    };
                    await _botClient.SendTextMessageAsync(chatId, "Введите имя:");
                    break;
                case "lastName":
                    RegistrationStates.States[chatId] = new RegistrationState
                    {
                        ChatId = chatId,
                        CurrentStep = "LastName",
                        UserData = new Models.User { }
                    };
                    await _botClient.SendTextMessageAsync(chatId, "Введите фамилию:");
                    break;
                case "age":
                    RegistrationStates.States[chatId] = new RegistrationState
                    {
                        ChatId = chatId,
                        CurrentStep = "Age",
                        UserData = new Models.User { }
                    };
                    await _botClient.SendTextMessageAsync(chatId, "Введите возраст(только цифрами):");
                    break;
                case "language":
                    RegistrationStates.States[chatId] = new RegistrationState
                    {
                        ChatId = chatId,
                        CurrentStep = "Language",
                        UserData = new Models.User { }
                    };
                    await _botClient.SendTextMessageAsync(chatId, "Введите язык(Английский, Русский, Французский):");
                    break;
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
                await SendMainMenu(chatId);
            }
        }
        public async Task StartHangmanGame(long chatId)
        {
            using var db = new AppDbContext();
            var user = await db.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);
            var language = user.Language;
            var words = db.Words.Where(c => c.Language == language);
            var randomWord = new List<string>();
            foreach(var words1 in words)
                randomWord.Add(words1.Text);
            var random = new Random();
            var word = randomWord[random.Next(randomWord.Count)];

            HangmanGames.Games[chatId] = new HangmanGame(word, _botClient);

            await _botClient.SendTextMessageAsync(chatId, $"Игра началась! Угадайте слово Из {word.Length} букв\n{HangmanGames.Games[chatId].GetCurrentState()}\n");
        }

        public class RegistrationStates
        {
            public static Dictionary<long, RegistrationState> States = new();
        }
    }
}
