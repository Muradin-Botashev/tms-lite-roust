using Domain.Persistables;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Services.Translations
{
    public static class TranslationProvider
    {
        public static void FillCache(IEnumerable<Translation> translations)
        {
            _cache.Ru.Clear();
            _cache.En.Clear();

            foreach (Translation t in translations)
            {
                _cache.Ru[t.Name] = t.Ru;
                _cache.En[t.Name] = t.En;
            }
        }

        public static string Translate(string key, string lang)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (lang == "en")
                {
                    if (_cache.En.TryGetValue(key, out string result))
                    {
                        return result;
                    }
                }
                else
                {
                    if (_cache.Ru.TryGetValue(key, out string result))
                    {
                        return result;
                    }
                }
            }
            return key;
        }

        public static IEnumerable<string> GetKeysByTranslation(string value)
        {
            return _cache.Ru.Concat(_cache.En)
                        .Where(x => string.Compare(x.Value, value, true) == 0)
                        .Select(x => x.Key);
        }

        private static readonly TranslationCache _cache = new TranslationCache();
    }
}
