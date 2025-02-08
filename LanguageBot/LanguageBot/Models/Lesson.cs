using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageBot.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Level { get; set; }
        public string VideoUrl { get; set; } // Ссылка на видеоурок
        public string AudioUrl { get; set; } // Ссылка на аудиоурок
    }
}
