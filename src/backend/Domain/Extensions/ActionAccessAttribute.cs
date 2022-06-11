using Domain.Enums;
using System;

namespace Domain.Extensions
{
    public class ActionAccessAttribute : Attribute
    {
        public ActionAccess Access { get; set; }

        public ActionAccessAttribute(ActionAccess access)
        {
            Access = access;
        }
    }
}
