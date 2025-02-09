

namespace LanguageBot.Models
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateEarned { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }

    }
}
