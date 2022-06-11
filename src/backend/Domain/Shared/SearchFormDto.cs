using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Shared
{
    /// <summary>
    /// Basic Search Form
    /// </summary>
    public class SearchFormDto: PagingFormDto
    {
        public string Search { get; set; }
    }
}
