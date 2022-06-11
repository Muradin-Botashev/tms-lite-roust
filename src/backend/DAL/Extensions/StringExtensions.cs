using System;

namespace DAL.Extensions
{
    public static class StringExt
    {
        public const string SqlDateFormat = "dd.mm.yyyy HH24:MI";
        public const string SqlTimeFormat = "HH24:MI";

        public static string SqlFormat(this DateTime text, string sub)
        {
            throw new NotImplementedException("This method is not supposed to run on client");
        }

        public static string SqlFormat(this TimeSpan text, string sub)
        {
            throw new NotImplementedException("This method is not supposed to run on client");
        }
    }
}
