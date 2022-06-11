using System;
using Domain.Enums;

namespace Domain.Extensions
{
    public class StateColorAttribute : Attribute
    {
        public AppColor Color { get; }

        public StateColorAttribute(AppColor color)
        {
            Color = color;
        }
    }
}