using Domain.Enums;
using Domain.Services.Translations;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Extensions
{
    public static class Extensions
    {
        public static string ToUpperFirstLetter(this string input)
        {
            return Char.ToUpperInvariant(input[0]) + input.Substring(1);
        }
        
        public static string ToLowerFirstLetter(this string input)
        {
            return Char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        public static string Pluralize(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            else if (input.EndsWith('s'))
            {
                return input + "es";
            }
            else if (input.EndsWith('y'))
            {
                return input.Substring(0, input.Length - 1) + "ies";
            }
            else
            {
                return input + "s";
            }
        }

        public static string GetHash(this string text)
        {
            return Convert.ToBase64String(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            // we're only getting the first description we find
                            // others will be ignored
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }

                        break;
                    }
                }
            }

            return description;
        }
        
        public static AppColor GetColor<T>(this T e)
        {
            AppColor description = AppColor.Black;

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == ((IConvertible) e).ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var attributes = memInfo[0].GetCustomAttributes(typeof(StateColorAttribute), false);
                        if (attributes.Length > 0)
                        {
                            // we're only getting the first description we find
                            // others will be ignored
                            description = ((StateColorAttribute)attributes[0]).Color;
                        }

                        break;
                    }
                }
            }

            return description;
        }

        public static IEnumerable<T> GetOrderedEnum<T>()
        {
            Type type = typeof(T);
            var values = Enum.GetValues(type);

            var valuesDict = new Dictionary<T, int>();
            foreach (var rawValue in values)
            {
                int orderNumber = -1;
                var memInfo = type.GetMember(type.GetEnumName(rawValue));
                if (memInfo?.Length > 0)
                {
                    var orderNumberAttributes = memInfo[0].GetCustomAttributes(typeof(OrderNumberAttribute), false);
                    if (orderNumberAttributes?.Length > 0)
                    {
                        orderNumber = ((OrderNumberAttribute)orderNumberAttributes[0]).Value;
                    }
                }
                valuesDict[(T)rawValue] = orderNumber;
            }

            return valuesDict.OrderBy(x => x.Value)
                             .Select(x => x.Key);
        }

        public static LookUpDto GetEnumLookup<T>(this T value, string language)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                string strValue = value.FormatEnum();
                string name = strValue.Translate(language);
                return new LookUpDto
                {
                    Value = strValue,
                    Name = name
                };
            }
        }
    }
}
