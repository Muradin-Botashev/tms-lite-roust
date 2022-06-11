using System.Collections.Generic;

namespace Infrastructure.Translations
{
    public class TranslationFile
    {
        public TranslationFile()
        {
            Ru = new Dictionary<string, string>();
            En = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Ru { get; set; }

        public Dictionary<string, string> En { get; set; }
    }
}
