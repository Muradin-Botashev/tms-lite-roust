using System.Collections.Generic;

namespace Domain.Services.Translations
{
    public class TranslationCache
    {
        public Dictionary<string, string> Ru { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> En { get; private set; } = new Dictionary<string, string>();
    }
}
