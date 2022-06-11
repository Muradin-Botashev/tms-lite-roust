using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    /// <summary>
    /// Аттрибут для валидации длины строки
    /// </summary>
    public class MaxLengthAttribute: Attribute
    {
        /// <summary>
        /// Максимальная длина строки
        /// </summary>
        public int Length { get; set; }

        public MaxLengthAttribute(int length)
        {
            Length = length;
        }
    }
}
