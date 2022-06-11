using System;

namespace Domain.Extensions
{
    public class ActionGroupAttribute : Attribute
    {
        public string Group { get; set; }

        public ActionGroupAttribute(string group)
        {
            Group = group;
        }
    }
}
