

namespace LanguageBot.Models
{
    public class DictionaryWords
    {
        public int Id { get; set; }
        public long UserID { get; set; } // Ссылка на пользователя
        public string OriginalWord { get; set; } // Оригинальное слово
        public string TranslatedWord { get; set; } // Перевод
        public DateTime AddedDate { get; set; } // Дата добавления
        public DateTime NextReviewDate { get; set; } // Дата следующего повторения
        public int ReviewInterval { get; set; } // Интервал повторения (в днях)
        public User user { get; set; }

    }
}
