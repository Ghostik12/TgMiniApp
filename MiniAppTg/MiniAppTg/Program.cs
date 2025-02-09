using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniAppTg.Controllers;
using System.Text;
using Telegram.Bot;

namespace MiniAppTg
{
    internal class Program
    {
        static async void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) => ConfigureServices(services))
                .UseConsoleLifetime()
                .Build();

            Console.WriteLine("Servives launch");

            await host.RunAsync();
            Console.WriteLine("Services stop");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(provide => new TelegramBotClient("7636151838:AAF9FcP9-WnHoE8SJmYD-bEQNEObULfsyfs"));
            services.AddHostedService<Bot>();

            //services.AddTransient<DefaultMessage>();
            services.AddTransient<TextMessageController>();
            //services.AddTransient<VoiceMessageController>();
            //services.AddTransient<MessagesUsersService>();
            //services.AddTransient<MessagesBotService>();
        }
    }
}
