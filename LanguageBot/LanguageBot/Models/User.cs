using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageBot.Models
{
    public class User
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Language { get; set; }
        public string Level { get; set; }
        public int XP { get; set; } = 0;
        public List<Achievement> Achievements { get; set; } = new();
    }
}
