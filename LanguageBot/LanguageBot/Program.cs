using System.Text;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LanguageBot.Controller;
using LanguageBot.Games;

namespace LanguageBot
{
    internal class Program
    {
        static async Task Main(string[] args)
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

        public static void ConfigureServices(IServiceCollection services)
        {

            services.AddScoped<ITelegramBotClient>(provide => new TelegramBotClient("7636151838:AAF9FcP9-WnHoE8SJmYD-bEQNEObULfsyfs"));
            services.AddHostedService<Bot>();

            services.AddScoped<HangmanGame>();
            services.AddScoped<TextMessageController>();
            //services.AddTransient<VoiceMessageController>();
            //services.AddTransient<Statistics>();
            //services.AddTransient<TestQuestion>();
            //services.AddTransient<UserData>();
            //services.AddTransient<Word>();
        }
    }
}
