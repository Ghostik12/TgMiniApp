using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageBot.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Type { get; set; } // Тип упражнения (карточки, multiple choice и т.д.)
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Language { get; set; }
        public string Level { get; set; }
    }
}
