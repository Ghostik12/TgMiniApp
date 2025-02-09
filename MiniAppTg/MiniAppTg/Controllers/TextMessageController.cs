using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MiniAppTg.Controllers
{
    internal class TextMessageController
    {
        ITelegramBotClient _telegramBotClient;
        public TextMessageController(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task Handle(Message message, CancellationToken cancellationToken)
        {

        }
    }
}
