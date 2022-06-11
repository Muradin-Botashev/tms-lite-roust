using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Extensions
{
    public class CustomSqlMethodCallTranslator : NpgsqlCompositeMethodCallTranslator
    {
        public CustomSqlMethodCallTranslator(RelationalCompositeMethodCallTranslatorDependencies dependencies, INpgsqlOptions sqlServerOptions) : base(dependencies, sqlServerOptions)
        {
            AddTranslators(new[] { new DateFormatTranslator() });
        }
    }
}
