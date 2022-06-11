using DAL;
using Domain.Persistables;
using Domain.Services.Translations;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Infrastructure.Translations
{
    public static class TranslationsUpdater
    {
        public static void SyncTranslations(this IServiceCollection services)
        {
            var buildServiceProvider = services.BuildServiceProvider();
            var appDbContext = buildServiceProvider.GetService<AppDbContext>();
            SyncTranslations(appDbContext);
        }

        public static void SyncTranslations(this AppDbContext db)
        {
            try
            {
                var yamlReader = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .Build();

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "translations.yml");
                string fileData = File.ReadAllText(filePath);
                var fileDto = yamlReader.Deserialize<TranslationFile>(fileData);

                if (db.Translations.Any())
                {
                    db.Translations.RemoveRange(db.Translations.ToList());
                }

                string[] keys = fileDto.Ru.Keys.Concat(fileDto.En.Keys).Distinct().ToArray();
                foreach (string key in keys)
                {
                    fileDto.Ru.TryGetValue(key, out string ruValue);
                    fileDto.En.TryGetValue(key, out string enValue);

                    Translation entity = new Translation
                    {
                        Name = key,
                        Ru = ruValue,
                        En = enValue
                    };
                    db.Translations.Add(entity);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Не удалось актуализировать локализацию сайта");
            }

            var actualTranslations = db.Translations.ToList();
            TranslationProvider.FillCache(actualTranslations);
        }
    }
}
