using Serilog;
using System;
using System.Globalization;
using System.Xml;

namespace Tasks.Helpers
{
    public static class ParseXmlExtensions
    {
        public static string ExtractSPGR(this string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return null;
            }
            rawValue = rawValue.Trim();
            if (rawValue.Length < 5)
            {
                Log.Warning("Длина поля SPGR должна быть равна 5, но обнаружено значение: {rawValue}.", rawValue);
                return rawValue;
            }
            else
            {
                return rawValue.Substring(rawValue.Length - 5);
            }
        }

        public static decimal ParseUom(this XmlNode node, string xPath, string[] keys, decimal[] values, decimal defaultValue, int? entryInd = null)
        {
            string actualKey = node.SelectSingleNode(xPath)?.InnerText?.ToUpper();
            if (!string.IsNullOrEmpty(actualKey))
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].ToUpper() == actualKey)
                    {
                        return values[i];
                    }
                }
                Log.Warning("Неизвестное значение {actualKey} элемента {xPath} записи #{entryInd}.", actualKey, xPath, entryInd);
            }
            else if (entryInd.HasValue)
            {
                Log.Warning("Элемент {xPath} не найден в записи #{entryInd}.", xPath, entryInd);
            }
            else
            {
                Log.Warning("Элемент {xPath} не найден.", xPath);
            }
            return defaultValue;
        }

        public static decimal? ParseDecimal(this XmlNode node, string xPath, int? entryInd = null, bool isRequired = false)
        {
            string value = node.SelectSingleNode(xPath)?.InnerText;
            if (!string.IsNullOrEmpty(value))
            {
                decimal result;
                if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }
                else if (entryInd.HasValue)
                {
                    Log.Warning("Некорректное значение {value} элемента {xPath} записи #{entryInd}, ожидалось число.", value, xPath, entryInd);
                }
                else
                {
                    Log.Warning("Некорректное значение {value} элемента {xPath}, ожидалось число.", value, xPath);
                }
            }
            else if (isRequired)
            {
                if (entryInd.HasValue)
                {
                    Log.Warning("Элемент {xPath} не найден в записи #{entryInd}.", xPath, entryInd);
                }
                else
                {
                    Log.Warning("Элемент {xPath} не найден.", xPath);
                }
            }
            return null;
        }

        public static DateTime? ParseDateTime(this XmlNode node, string xPath, int? entryInd = null, bool isRequired = false)
        {
            string value = node.SelectSingleNode(xPath)?.InnerText;
            if (!string.IsNullOrEmpty(value))
            {
                DateTime result;
                if (DateTime.TryParseExact(value, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
                else if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
                else if (entryInd.HasValue)
                {
                    Log.Warning("Некорректное значение {value} элемента {xPath} записи #{entryInd}, ожидалась дата/время.", value, xPath, entryInd);
                }
                else
                {
                    Log.Warning("Некорректное значение {value} элемента {xPath}, ожидалась дата/время.", value, xPath);
                }
            }
            else if (isRequired)
            {
                if (entryInd.HasValue)
                {
                    Log.Warning("Элемент {xPath} не найден в записи #{entryInd}.", xPath, entryInd);
                }
                else
                {
                    Log.Warning("Элемент {xPath} не найден.", xPath);
                }
            }
            return null;
        }

        public static int? ParseInt(this XmlNode node, string xPath, int? entryInd = null, bool isRequired = false)
        {
            string value = node.SelectSingleNode(xPath)?.InnerText;
            if (!string.IsNullOrEmpty(value))
            {
                int result;
                if (int.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }
                else if (entryInd.HasValue)
                {
                    Log.Warning("Некорректное значение {value} элемента {xPath} записи #{entryInd}, ожидалось целое число.", value, xPath, entryInd);
                }
                else
                {
                    Log.Warning("Некорректное значение {value} элемента {xPath}, ожидалось целое число.", value, xPath);
                }
            }
            else if (isRequired)
            {
                if (entryInd.HasValue)
                {
                    Log.Warning("Элемент {xPath} не найден в записи #{entryInd}.", xPath, entryInd);
                }
                else
                {
                    Log.Warning("Элемент {xPath} не найден.", xPath);
                }
            }
            return null;
        }

        public static int? ApplyUowCoeff(this decimal? value, decimal coeff)
        {
            return value == null ? (int?)null : (int)(value * coeff);
        }

        public static decimal? ApplyDecimalUowCoeff(this decimal? value, decimal coeff)
        {
            return value == null ? (decimal?)null : (decimal)(value * coeff);
        }

        private static string[] DateTimeFormats = new[]
        {
            "yyyyMMddTHHmmss", "dd.MM.yyyy HH:mm", "MM/dd/yyyy hh:mm tt",
            "yyyyMMdd", "dd.MM.yyyy", "MM/dd/yyyy"
        };
    }
}
