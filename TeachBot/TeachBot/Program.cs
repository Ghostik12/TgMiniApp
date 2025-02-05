using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TeachBot.DB;

namespace TeachBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //DataBase db = new DataBase();
            //db.Initialize();
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

            services.AddSingleton<ITelegramBotClient>(provide => new TelegramBotClient(""));
            services.AddHostedService<Bot>();

            services.AddTransient<DefaultMessage>();
            services.AddTransient<TextMessageController>();
            services.AddTransient<VoiceMessageController>();
            services.AddTransient<Statistics>();
            services.AddTransient<TestQuestion>();
            services.AddTransient<UserData>();
            services.AddTransient<Word>();
        }
    }
}
