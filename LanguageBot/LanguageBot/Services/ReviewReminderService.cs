using LanguageBot.DB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using LanguageBot.Models;
using Microsoft.EntityFrameworkCore;

namespace LanguageBot.Services
{
    public class ReviewReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ReviewReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var botClient = scope.ServiceProvider.GetRequiredService<TelegramBotClient>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // Получаем всех пользователей
                        var users = await dbContext.Users.ToListAsync(stoppingToken);

                        foreach (var user in users)
                        {
                            // Получаем слова для повторения
                            var words = await dbContext.DictionaryWord
                                .Where(uw => uw.UserID == user.ChatId && uw.NextReviewDate <= DateTime.UtcNow)
                                .ToListAsync(stoppingToken);

                            if (words.Any())
                            {
                                // Отправляем напоминание
                                await SendReviewReminderAsync(botClient, user.ChatId, words);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    Console.WriteLine($"Ошибка в ReviewReminderService: {ex.Message}");
                }

                // Ждем перед следующей проверкой (например, 1 час)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task SendReviewReminderAsync(TelegramBotClient botClient, long userId, List<DictionaryWords> words)
        {
            var message = new StringBuilder("📚 Время повторить слова");

            //foreach (var userWord in words)
            //{
            //    message.AppendLine($"🔤 {userWord.OriginalWord} — {userWord.TranslatedWord}");
            //}

            message.AppendLine("\nХотите повторить сейчас?");

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Да", $"reviewconfirm_{userId}"),
                InlineKeyboardButton.WithCallbackData("❌ Нет", $"reviewcancel_{userId}")
            }
        });

            await botClient.SendTextMessageAsync(userId, message.ToString(), replyMarkup: keyboard);
        }
    }
}
