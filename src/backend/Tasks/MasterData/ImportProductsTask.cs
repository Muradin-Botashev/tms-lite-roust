using DAL;
using Domain.Persistables;
using Domain.Services.Articles;
using Domain.Services.Injections;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Tasks.Common;
using Tasks.Helpers;
using Tasks.Statistics;

namespace Tasks.MasterData
{
    [Description("Импорт инжекций MatMas на сихронизацию мастер-данных по продуктам")]
    public class ImportProductsTask : TaskBase<ImportProductsProperties>, IScheduledTask
    {
        public string Schedule => "*/5 * * * *";

        protected override async Task Execute(IServiceProvider serviceProvider, ImportProductsProperties props, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(props.ConnectionString))
            {
                throw new Exception("ConnectionString является обязательным параметром");
            }

            if (string.IsNullOrEmpty(props.Folder))
            {
                props.Folder = "/";
            }

            if (string.IsNullOrEmpty(props.FileNamePattern))
            {
                props.FileNamePattern = @"^.*MATMAS.*\.xml$";
            }

            if (string.IsNullOrEmpty(props.ViewHours))
            {
                props.ViewHours = "24";
            }

            int viewHours;
            if (!int.TryParse(props.ViewHours, out viewHours))
            {
                throw new Exception("Параметр ViewHours должен быть целым числом");
            }

            try
            {
                Regex fileNameRe = new Regex(props.FileNamePattern, RegexOptions.IgnoreCase);
                IInjectionsService injectionsService = serviceProvider.GetService<IInjectionsService>();

                ConnectionInfo sftpConnection = GetSftpConnection(props.ConnectionString);
                using (SftpClient sftpClient = new SftpClient(sftpConnection))
                {
                    sftpClient.Connect();

                    DateTime barrierTime = DateTime.UtcNow.AddHours(-viewHours);
                    IEnumerable<InjectionDto> processedInjections = injectionsService.GetByTaskName(TaskName);
                    HashSet<string> processedFileNames = new HashSet<string>(processedInjections.Select(i => i.FileName));

                    var files = sftpClient.ListDirectory(props.Folder);
                    files = files.Where(f => f.LastWriteTimeUtc >= barrierTime && f.IsRegularFile)
                                 .OrderBy(f => f.LastWriteTimeUtc);

                    var filesQueueLength = files.Count();

                    foreach (SftpFile file in files)
                    {
                        StatisticsStore.UpdateFilesQueueLength(TaskName, filesQueueLength);
                        --filesQueueLength;

                        if (!fileNameRe.IsMatch(file.Name))
                        {
                            continue;
                        }

                        if (!processedFileNames.Contains(file.Name))
                        {
                            Log.Information("Найден новый файл: {FullName}.", file.FullName);

                            InjectionDto injection = new InjectionDto
                            {
                                Type = TaskName,
                                FileName = file.Name,
                                ProcessTimeUtc = DateTime.UtcNow
                            };

                            try
                            {
                                string content = sftpClient.ReadAllText(file.FullName);
                                bool isSuccess = ProcessProductsFile(serviceProvider, file.Name, content);
                                injection.Status = isSuccess ? InjectionStatus.Success : InjectionStatus.Failed;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Не удалось обработать файл {Name}.", file.Name);
                                injection.Status = InjectionStatus.Failed;
                            }

                            injectionsService.SaveOrCreate(injection, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Ошибка при обработке {TaskName} инжекции.");
                throw ex;
            }
        }

        private ConnectionInfo GetSftpConnection(string connectionString)
        {
            Uri connection = new Uri(connectionString);
            string[] authParts = connection.UserInfo.Split(':');
            string login = authParts.Length == 2 ? HttpUtility.UrlDecode(authParts[0]) : null;
            string password = authParts.Length == 2 ? HttpUtility.UrlDecode(authParts[1]) : null;
            int port = connection.Port > 0 ? connection.Port : 22;
            ConnectionInfo result = new ConnectionInfo(connection.Host, port, login, new PasswordAuthenticationMethod(login, password));
            return result;
        }

        private bool ProcessProductsFile(IServiceProvider serviceProvider, string fileName, string fileContent)
        {
            // Загружаем справочник стран
            AppDbContext db = serviceProvider.GetService<AppDbContext>();
            var countries = db.Countries.ToList();
            Dictionary<string, string> countryNameLookup = new Dictionary<string, string>();
            foreach (var country in countries)
            {
                countryNameLookup[country.Key] = country.Name;
            }

            // Загружаем текущий список продуктов из базы
            IArticlesService articlesService = serviceProvider.GetService<IArticlesService>();
            var articlesFilter = new FilterFormDto<ArticleFilterDto>
            {
                Filter = new ArticleFilterDto(),
                Skip = 0,
                Take = int.MaxValue
            };
            IEnumerable<ArticleDto> currentProducts = articlesService.Search(articlesFilter).Items;

            // Получаем список продуктов из файла, обновляем имеющиеся продукты
            bool hasErrors;
            IEnumerable<ArticleDto> products = LoadProductsFromFile(fileName, fileContent, currentProducts, countryNameLookup, out hasErrors);

            if (!hasErrors)
            {
                // Сохраняем изменения в базу
                articlesService.Import(products, true);
            }

            return !hasErrors;
        }

        private IEnumerable<ArticleDto> LoadProductsFromFile(
            string fileName, 
            string fileContent, 
            IEnumerable<ArticleDto> currentProducts,
            Dictionary<string, string> countryNameLookup,
            out bool hasErrors)
        {
            List<ArticleDto> result = new List<ArticleDto>();
            hasErrors = false;

            // Формируем справочники старых и новых значений
            Dictionary<string, ArticleDto> currentLookup = new Dictionary<string, ArticleDto>();
            foreach (var product in currentProducts)
            {
                if (!string.IsNullOrEmpty(product.Nart))
                {
                    currentLookup[product.Nart] = product;
                }
            }

            // Загружаем данные из файла
            XmlDocument doc = new XmlDocument();
            using (StringReader reader = new StringReader(fileContent))
            {
                doc.Load(reader);
            }

            // Разбираем записи в файле по полям Dto
            int entryInd = 0;
            var productRoots = doc.DocumentElement.SelectNodes("//E1MARAM");
            var entriesQueue = productRoots.Count;
            foreach (XmlNode pRoot in productRoots)
            {
                StatisticsStore.UpdateEntriesQueueLength(TaskName, entriesQueue);
                --entriesQueue;

                ++entryInd;

                string nart = pRoot.SelectSingleNode("MATNR")?.InnerText?.TrimStart('0');
                if (string.IsNullOrEmpty(nart))
                {
                    Log.Warning("Пустое значение NART в записи #{entryInd} файла {fileName}, запись пропущена.", entryInd, fileName);
                    continue;
                }

                // Ищем имеющуюся запись по Nart
                ArticleDto product;
                if (!currentLookup.TryGetValue(nart, out product))
                {
                    // Не нашли - создаем новую
                    product = new ArticleDto();
                    Log.Information("Найден новый продукт NART = {nart} в файле {fileName}.", nart, fileName);
                }
                else
                {
                    Log.Information("Обновлен продукт {Id} (NART = {nart}) данными из файла {fileName}.", product.Id, nart, fileName);
                }

                // Получение единиц измерения по каждому полю или группе смежных полей

                decimal weightNetUowCoeff = pRoot.ParseUom("GEWEI", new[] { "GRM", "GR", "KGM", "KG" }, new[] { 1M, 1M, 1000M, 1000M }, 1, entryInd);
                decimal weightPieceUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='PCE']/GEWEI", new[] { "GRM", "GR", "KGM", "KG" }, new[] { 1M, 1M, 1000M, 1000M }, 1, entryInd);
                decimal weightShrinkUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='#2R']/GEWEI", new[] { "GRM", "GR", "KGM", "KG" }, new[] { 1M, 1M, 1000M, 1000M }, 1, entryInd);
                decimal weightBoxUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='CT']/GEWEI", new[] { "GRM", "GR", "KGM", "KG" }, new[] { 1M, 1M, 1000M, 1000M }, 1, entryInd);
                decimal weightLayerUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='#18']/GEWEI", new[] { "GRM", "GR", "KGM", "KG" }, new[] { 1M, 1M, 1000M, 1000M }, 1, entryInd);
                decimal weightPalletUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='PF']/GEWEI", new[] { "GRM", "GR", "KGM", "KG" }, new[] { 1M, 1M, 1000M, 1000M }, 1, entryInd);

                decimal sizePieceUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='PCE']/MEABM", new[] { "MMT", "MTR" }, new[] { 1M, 1000M }, 1, entryInd);
                decimal sizeShrinkUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='#2R']/MEABM", new[] { "MMT", "MTR" }, new[] { 1M, 1000M }, 1, entryInd);
                decimal sizeBoxUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='CT']/MEABM", new[] { "MMT", "MTR" }, new[] { 1M, 1000M }, 1, entryInd);
                decimal sizeLayerUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='#18']/MEABM", new[] { "MMT", "MTR" }, new[] { 1M, 1000M }, 1, entryInd);
                decimal sizePalletUowCoeff = pRoot.ParseUom("E1MARMM[MEINH='PF']/MEABM", new[] { "MMT", "MTR" }, new[] { 1M, 1000M }, 1, entryInd);

                string countryCode = pRoot.SelectSingleNode("E1MARCM/HERKL")?.InnerText;
                string countryName = null;
                if (!string.IsNullOrEmpty(countryCode))
                {
                    countryNameLookup.TryGetValue(countryCode, out countryName);
                }

                // Непосредственно заполнение полей

                product.Spgr = pRoot.SelectSingleNode("PRDHA")?.InnerText?.ExtractSPGR() ?? product.Spgr;
                product.Description = pRoot.SelectSingleNode("E1MAKTM[SPRAS_ISO='RU']/MAKTX")?.InnerText
                                   ?? pRoot.SelectSingleNode("E1MAKTM[SPRAS_ISO='EN']/MAKTX")?.InnerText
                                   ?? product.Description;
                product.Nart = nart;
                product.CountryOfOrigin = countryName ?? product.CountryOfOrigin;
                product.ShelfLife = pRoot.ParseInt("MHDHB", entryInd) ?? product.ShelfLife;
                product.Status = GetProductStatus(pRoot.SelectSingleNode("E1MARCM/MMSTA")?.InnerText) ?? product.Status;

                product.UnitLengthGoodsMm = pRoot.ParseDecimal("E1MARMM[MEINH='PCE']/LAENG", entryInd).ApplyUowCoeff(sizePieceUowCoeff) ?? product.UnitLengthGoodsMm;
                product.WidthUnitsGoodsMm = pRoot.ParseDecimal("E1MARMM[MEINH='PCE']/BREIT", entryInd).ApplyUowCoeff(sizePieceUowCoeff) ?? product.WidthUnitsGoodsMm;
                product.UnitHeightGoodsMm = pRoot.ParseDecimal("E1MARMM[MEINH='PCE']/HOEHE", entryInd).ApplyUowCoeff(sizePieceUowCoeff) ?? product.UnitHeightGoodsMm;
                product.WeightUnitsGrossProductG = pRoot.ParseDecimal("E1MARMM[MEINH='PCE']/BRGEW", entryInd).ApplyUowCoeff(weightPieceUowCoeff) ?? product.WeightUnitsGrossProductG;
                product.WeightUnitsNetGoodsG = pRoot.ParseDecimal("NTGEW", entryInd).ApplyUowCoeff(weightNetUowCoeff) ?? product.WeightUnitsNetGoodsG;

                product.EanShrink = pRoot.SelectSingleNode("E1MARMM[MEINH='#2R' and NUMTP='HK']/EAN11")?.InnerText
                                 ?? pRoot.SelectSingleNode("E1MARMM[MEINH='#2R' and NUMTP='HE']/EAN11")?.InnerText
                                 ?? product.EanShrink;
                product.PiecesInShrink = pRoot.ParseInt("E1MARMM[MEINH='#2R']/UMREZ", entryInd) ?? product.PiecesInShrink;
                product.LengthShrinkMm = pRoot.ParseDecimal("E1MARMM[MEINH='#2R']/LAENG", entryInd).ApplyUowCoeff(sizeShrinkUowCoeff) ?? product.LengthShrinkMm;
                product.WidthShrinkMm = pRoot.ParseDecimal("E1MARMM[MEINH='#2R']/BREIT", entryInd).ApplyUowCoeff(sizeShrinkUowCoeff) ?? product.WidthShrinkMm;
                product.HeightShrinkMm = pRoot.ParseDecimal("E1MARMM[MEINH='#2R']/HOEHE", entryInd).ApplyUowCoeff(sizeShrinkUowCoeff) ?? product.HeightShrinkMm;
                product.GrossShrinkWeightG = pRoot.ParseDecimal("E1MARMM[MEINH='#2R']/BRGEW", entryInd).ApplyUowCoeff(weightShrinkUowCoeff) ?? product.GrossShrinkWeightG;

                product.EanBox = pRoot.SelectSingleNode("E1MARMM[MEINH='CT' and NUMTP='HK']/EAN11")?.InnerText
                              ?? pRoot.SelectSingleNode("E1MARMM[MEINH='CT' and NUMTP='HE']/EAN11")?.InnerText
                              ?? product.EanBox;
                product.PiecesInABox = pRoot.ParseInt("E1MARMM[MEINH='CT']/UMREZ", entryInd) ?? product.PiecesInABox;
                product.BoxLengthMm = pRoot.ParseDecimal("E1MARMM[MEINH='CT']/LAENG", entryInd).ApplyUowCoeff(sizeBoxUowCoeff) ?? product.BoxLengthMm;
                product.WidthOfABoxMm = pRoot.ParseDecimal("E1MARMM[MEINH='CT']/BREIT", entryInd).ApplyUowCoeff(sizeBoxUowCoeff) ?? product.WidthOfABoxMm;
                product.BoxHeightMm = pRoot.ParseDecimal("E1MARMM[MEINH='CT']/HOEHE", entryInd).ApplyUowCoeff(sizeBoxUowCoeff) ?? product.BoxHeightMm;
                product.GrossBoxWeightG = pRoot.ParseDecimal("E1MARMM[MEINH='CT']/BRGEW", entryInd).ApplyUowCoeff(weightBoxUowCoeff) ?? product.GrossBoxWeightG;

                product.PiecesInALayer = pRoot.ParseInt("E1MARMM[MEINH='#18']/UMREZ", entryInd) ?? product.PiecesInALayer;
                product.LayerLengthMm = pRoot.ParseDecimal("E1MARMM[MEINH='#18']/LAENG", entryInd).ApplyUowCoeff(sizeLayerUowCoeff) ?? product.LayerLengthMm;
                product.LayerWidthMm = pRoot.ParseDecimal("E1MARMM[MEINH='#18']/BREIT", entryInd).ApplyUowCoeff(sizeLayerUowCoeff) ?? product.LayerWidthMm;
                product.LayerHeightMm = pRoot.ParseDecimal("E1MARMM[MEINH='#18']/HOEHE", entryInd).ApplyUowCoeff(sizeLayerUowCoeff) ?? product.LayerHeightMm;
                product.GrossLayerWeightMm = pRoot.ParseDecimal("E1MARMM[MEINH='#18']/BRGEW", entryInd).ApplyUowCoeff(weightLayerUowCoeff) ?? product.GrossLayerWeightMm;

                product.EanPallet = pRoot.SelectSingleNode("E1MARMM[MEINH='PF' and NUMTP='HK']/EAN11")?.InnerText
                                 ?? pRoot.SelectSingleNode("E1MARMM[MEINH='PF' and NUMTP='HE']/EAN11")?.InnerText
                                 ?? product.EanPallet;
                product.PiecesOnAPallet = pRoot.ParseInt("E1MARMM[MEINH='PF']/UMREZ", entryInd) ?? product.PiecesOnAPallet;
                product.PalletLengthMm = pRoot.ParseDecimal("E1MARMM[MEINH='PF']/LAENG", entryInd).ApplyUowCoeff(sizePalletUowCoeff) ?? product.PalletLengthMm;
                product.WidthOfPalletsMm = pRoot.ParseDecimal("E1MARMM[MEINH='PF']/BREIT", entryInd).ApplyUowCoeff(sizePalletUowCoeff) ?? product.WidthOfPalletsMm;
                product.PalletHeightMm = pRoot.ParseDecimal("E1MARMM[MEINH='PF']/HOEHE", entryInd).ApplyUowCoeff(sizePalletUowCoeff) ?? product.PalletHeightMm;
                product.GrossPalletWeightG = pRoot.ParseDecimal("E1MARMM[MEINH='PF']/BRGEW", entryInd).ApplyUowCoeff(weightPalletUowCoeff) ?? product.GrossPalletWeightG;

                result.Add(product);
            }

            return result;
        }

        private string GetProductStatus(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return code;
            }

            switch (code)
            {
                case "A":
                    return "Active";
                case "S1":
                    return "Under construction";
                case "S2":
                    return "Ready f.order intake";
                case "V0":
                    return "Announcement";
                case "V1":
                    return "Sold out";
                case "V2":
                    return "Ready to archive";
                default:
                    return null;
            }
        }
    }
}
