using System;
using System.Globalization;

namespace Domain.Extensions
{
    public static class FormatExtensions
    {
        public const string DateTimeFormat = "dd.MM.yyyy HH:mm";
        public const string DateFormat = "dd.MM.yyyy";
        public const string TimeFormat = "hh\\:mm";

        private readonly static string[] ValidDateTimeFormats = new[] {
            "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm", "dd.MM.yyyy",
            "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm", "MM/dd/yyyy",
            "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm", "yyyy-MM-dd"
        };
        private readonly static string[] ValidTimeFormats = new[] { "hh\\:mm\\:ss", "hh\\:mm" };

        public static string FormatDate(this DateTime value)
        {
            return value.ToString(DateFormat);
        }

        public static string FormatDate(this DateTime? value)
        {
            return value?.FormatDate();
        }

        public static string FormatTime(this TimeSpan value)
        {
            return value.ToString(TimeFormat);
        }

        public static string FormatTime(this TimeSpan? value)
        {
            return value?.FormatTime();
        }

        public static string FormatDateTime(this DateTime value)
        {
            return value.ToString(DateTimeFormat);
        }

        public static string FormatDateTime(this DateTime? value)
        {
            return value?.FormatDateTime();
        }

        public static string FormatInt(this int value)
        {
            return value.ToString();
        }

        public static string FormatInt(this int? value)
        {
            return value?.FormatInt();
        }

        public static string FormatDecimal(this decimal value, int? decimals = null)
        {
            if (decimals.HasValue)
            {
                return Math.Round(value, decimals.Value).ToString();
            }
            else
            {
                return value.ToString();
            }
        }

        public static string FormatDecimal(this decimal? value, int? decimals = null)
        {
            return value?.FormatDecimal(decimals);
        }

        public static string FormatGuid(this Guid value)
        {
            return value.ToString();
        }

        public static string FormatGuid(this Guid? value)
        {
            return value?.FormatGuid();
        }

        public static string FormatEnum<TEnum>(this TEnum value)
        {
            return value.ToString().ToLowerFirstLetter();
        }

        public static string FormatEnum<TEnum>(this TEnum? value) where TEnum : struct
        {
            return value?.FormatEnum();
        }

        public static DateTime? ToDateTime(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (DateTime.TryParseExact(value, ValidDateTimeFormats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out DateTime parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        public static DateTime? ToDate(this string value)
        {
            DateTime? parsedValue = value.ToDateTime();
            return parsedValue == null ? (DateTime?)null : parsedValue.Value.Date;
        }

        public static TimeSpan? ToTime(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (TimeSpan.TryParseExact(value, ValidTimeFormats, CultureInfo.InvariantCulture,
                                       TimeSpanStyles.None, out TimeSpan parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        public static Guid? ToGuid(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (Guid.TryParse(value, out Guid parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        public static TEnum? ToEnum<TEnum>(this string value) where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (Enum.TryParse(value, true, out TEnum parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        public static decimal? ToDecimal(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number,
                                 CultureInfo.InvariantCulture, out decimal parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        public static int? ToInt(this string value)
        {
            decimal? parsedValue = value.ToDecimal();
            return parsedValue == null ? (int?)null : (int)parsedValue.Value;
        }

        public static bool? ToBool(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (bool.TryParse(value, out bool parsedValue))
            {
                return parsedValue;
            }

            return null;
        }
    }
}
