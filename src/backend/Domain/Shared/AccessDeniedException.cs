using Domain.Services.Translations;
using System;

namespace Domain.Shared
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string lang)
            : base ("AccessDeniedException".Translate(lang))
        { 
        }
    }
}
