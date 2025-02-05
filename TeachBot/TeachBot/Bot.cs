using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;

namespace TeachBot
{
    internal class Bot : BackgroundService
    {
        private ITelegramBotClient _telegramClient;
        private VoiceMessageController _voiceMessageController;
        private TextMessageController _textMessageController;
        private DefaultMessage _defaultMessage;

        public Bot(ITelegramBotClient telegramClient, VoiceMessageController voiceMessageController, TextMessageController textMessageController, DefaultMessage defaultMessage)
        {
            _telegramClient = telegramClient;
            _voiceMessageController = voiceMessageController;
            _textMessageController = textMessageController;
            _defaultMessage = defaultMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new ReceiverOptions() { AllowedUpdates = { } }, cancellationToken: stoppingToken);

            Console.WriteLine("Бот запущен");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                await _textMessageController.HandleCallBack(update.CallbackQuery, _telegramClient);
                return;
            }

            if (update.Type == UpdateType.Message)
            {
                switch (update.Message!.Type)
                {
                    case MessageType.Voice:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка аудио не доступна для пользователя.");
                        return;
                    case MessageType.Text:
                        await _textMessageController.Handle(update.Message, cancellationToken);
                        return;
                    case MessageType.Document:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка документов не доступна для пользователя.");
                        return;
                    case MessageType.Photo:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка фото не доступна для пользователя.");
                        return;
                    case MessageType.Video:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка видео не доступна для пользователя.");
                        return;
                    case MessageType.Animation:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка анимации не доступна для пользователя.");
                        return;
                    case MessageType.Audio:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка аудио не доступна для пользователя.");
                        return;
                    case MessageType.Contact:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка контактов не доступна для пользователя.");
                        return;
                    case MessageType.Sticker:
                        await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправка стикеров не доступна для пользователя.");
                        return;
                    default:
                        await _defaultMessage.Handle(update.Message, cancellationToken);
                        return;
                }
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException}]\n{apiRequestException}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);

            Console.WriteLine("Ожидаем 10 секунд перед повторным подключением");
            Thread.Sleep(10000);

            return Task.CompletedTask;
        }
    }
}