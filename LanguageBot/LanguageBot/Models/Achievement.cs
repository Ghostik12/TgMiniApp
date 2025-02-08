using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
