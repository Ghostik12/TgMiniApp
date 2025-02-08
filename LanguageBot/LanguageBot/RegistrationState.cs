using LanguageBot.Models;

namespace LanguageBot
{
    public class RegistrationState
    {
        public long ChatId { get; set; }
        public string CurrentStep { get; set; }
        public User UserData { get; set; }
    }
}
