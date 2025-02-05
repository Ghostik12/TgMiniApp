using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TeachBot
{
    internal class TextMessageController
    {
        private ITelegramBotClient _botClient;
        private static Dictionary<long, UserData> _userData = new Dictionary<long, UserData>();

        public TextMessageController(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }
        public async Task Handle(Message message, CancellationToken cancellationToken)
        {
            if (message.Type != MessageType.Text)
                return;

            var chatId = message.Chat.Id;
            var messageText = message.Text;

            Console.WriteLine($"Получено сообщение: {messageText}");

            // Инициализация данных пользователя
            if (!_userData.ContainsKey(chatId))
            {
                _userData[chatId] = new UserData();
            }

            // Обработка команд
            switch (messageText)
            {
                case "/start":
                    await SendLanguageSelection(message);
                    break;

                case "📖 Учить слова":
                    await SendWordToLearn(chatId);
                    break;

                case "🎯 Тест":
                    await SendTestQuestion(chatId);
                    break;

                case "📊 Статистика":
                    await SendStatistics(chatId);
                    break;

                case "⚙ Настройки":
                    await SendSettings(chatId);
                    break;

                case "ℹ Помощь":
                    await SendHelp(chatId);
                    break;

                default:
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Неизвестная команда. Используй /start для начала.",
                        cancellationToken: cancellationToken);
                    break;
            }
        }

        private async Task SendHelp(long chatId)
        {
            await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Помощь:\n/start - Начать\n📖 Учить слова - Учить новые слова\n🎯 Тест - Пройти тест\n📊 Статистика - Просмотреть статистику\n⚙ Настройки - Настройки бота\nℹ Помощь - Справка");
        }

        private async Task SendSettings(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Режим обучения: Слова", "mode_words"),
                InlineKeyboardButton.WithCallbackData("Режим обучения: Фразы", "mode_phrases")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Напоминания: Вкл", "reminders_on"),
                InlineKeyboardButton.WithCallbackData("Напоминания: Выкл", "reminders_off")
            }
        });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Настройки:",
                replyMarkup: inlineKeyboard);
        }

        private async Task SendStatistics(long chatId)
        {
            var stats = _userData[chatId].Statistics;
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Статистика:\nИзучено слов: {stats.WordsLearned}\nПравильных ответов: {stats.CorrectAnswers}%");
        }

        private async Task SendTestQuestion(long chatId)
        {
            TestQuestion question = (TestQuestion)GetTestQuestion(_userData[chatId].SelectedLanguage);
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(question.Options[0], "answer_0"),
                InlineKeyboardButton.WithCallbackData(question.Options[1], "answer_1")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(question.Options[2], "answer_2"),
                InlineKeyboardButton.WithCallbackData(question.Options[3], "answer_3")
            }
        });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Вопрос: {question.Question}",
                replyMarkup: inlineKeyboard);
        }

        private object GetTestQuestion(object selectedLanguage)
        {
            return new TestQuestion
            {
                Question = "Как переводится 'Hello'?",
                Options = new[] { "Привет", "Пока", "Спасибо", "Пожалуйста" },
                CorrectAnswerIndex = 0
            };
        }

        private async Task SendWordToLearn(long chatId)
        {
            Word word = (Word)GetRandomWord(_userData[chatId].SelectedLanguage);
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Следующее слово", "next_word"),
                InlineKeyboardButton.WithCallbackData("Добавить в избранное", "add_to_favorites")
            }
        });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Слово: {word.Words}\nПеревод: {word.Translation}",
                replyMarkup: inlineKeyboard);
        }

        private object GetRandomWord(string selectedLanguage)
        {
            return new Word { Words = "Hello", Translation = "Привет" };
        }

        private async Task SendLanguageSelection(Message message)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Английский 🇬🇧", "lang_en"),
                InlineKeyboardButton.WithCallbackData("Турецкий 🇹🇷", "lang_tr"),
                InlineKeyboardButton.WithCallbackData("Французский 🇫🇷", "lang_fr")
            }
        });

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Выбери язык для изучения:",
                replyMarkup: inlineKeyboard);


        }

        internal async Task HandleCallBack(CallbackQuery? callbackQuery, ITelegramBotClient botClient)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var callbackData = callbackQuery.Data;
            var userName = callbackQuery.From.Username;

            Console.WriteLine($"Обработка CallbackQuery: {callbackData}");

            // Обработка выбора языка
            if (callbackData.StartsWith("lang_"))
            {
                var languageCode = callbackData.Split('_')[1];
                _userData[chatId].SelectedLanguage = languageCode;

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Выбран язык: {GetLanguageName(languageCode)}");

                // Отправляем главное меню после выбора языка
                await SendMainMenu(chatId);
            }

            // Обработка других CallbackQuery (например, "Следующее слово", "Добавить в избранное")
            else if (callbackData == "next_word")
            {
                await SendWordToLearn(chatId);
            }
            else if (callbackData == "add_to_favorites")
            {
                // Получаем текущее слово из сообщения
                var messageText = callbackQuery.Message.Text;
                var wordText = messageText.Split('\n')[0].Replace("Слово: ", "").Trim();

                // Находим wordId
                var user = UserManager.GetUserByChatId(chatId);
                var wordId = WordManager.FindWordId(user.SelectedLanguage, wordText);

                if (wordId != 0 || wordId != null)
                {
                    // Добавляем в избранное
                    FavoriteManager.AddFavorite(user.Id, wordId);
                    await botClient.SendTextMessageAsync(chatId, "Слово добавлено в избранное.");
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Слово не найдено в базе данных.");
                }
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Слово добавлено в избранное.");
            }

            // Подтверждаем обработку CallbackQuery
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }

        private object GetLanguageName(string languageCode)
        {
            return languageCode switch
            {
                "en" => "Английский 🇬🇧",
                "tr" => "Турецкий 🇹🇷",
                "fr" => "Французский 🇫🇷",
                _ => "Неизвестный язык"
            };
        }

        private async Task SendMainMenu(long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📖 Учить слова") },
                new[] { new KeyboardButton("🎯 Тест"), new KeyboardButton("📊 Статистика") },
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

        private async Task HandleAdminCommands(long chatId, string messageText)
        {
            if (!AdminManager.IsAdmin(chatId))
            {
                await _botClient.SendTextMessageAsync(chatId, "У вас нет прав администратора.");
                return;
            }

            switch (messageText)
            {
                case "/addadmin":
                    await _botClient.SendTextMessageAsync(chatId, "Введите ChatId нового администратора:");
                    break;

                case "/removeadmin":
                    await _botClient.SendTextMessageAsync(chatId, "Введите ChatId администратора для удаления:");
                    break;

                case "/addword":
                    await _botClient.SendTextMessageAsync(chatId, "Введите слово и перевод в формате: язык|слово|перевод");
                    break;

                case "/removeword":
                    await _botClient.SendTextMessageAsync(chatId, "Введите ID слова для удаления:");
                    break;

                case "/viewusers":
                    var users = UserManager.GetUsers();
                    var userList = string.Join("\n", users.Select(u => $"{u.Username} (ChatId: {u.ChatId})"));
                    await _botClient.SendTextMessageAsync(chatId, $"Список пользователей:\n{userList}");
                    break;
            }

        }
    }
}