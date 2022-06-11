using Domain.Persistables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain.Extensions
{
    public static class QuerybleExtentions
    {
        public static IQueryable<Translation> WhereTranslation(this IQueryable<Translation> query, string search)
        {
            return query.Where(i => i.Ru.ToLower().Contains(search) || i.En.ToLower().Contains(search));
        }
    }
}
