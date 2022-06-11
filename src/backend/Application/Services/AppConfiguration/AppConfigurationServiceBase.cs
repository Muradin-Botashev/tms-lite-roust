using System;

namespace Application.Services.AppConfiguration
{
    public class AppConfigurationServiceBase
    {
        protected string GetName(Type type)
        {
            return ToLowerfirstLetter(type.Name.Replace("Service", ""));
        }

        protected string GetName<T>()
        {
            return GetName(typeof(T));
        }
                
        protected static string ToLowerfirstLetter(string input)
        {
            return Char.ToLowerInvariant(input[0]) + input.Substring(1);
        }        

    }
}