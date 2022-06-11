using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910181254)]
    public class MakeBoxesCountDecimal : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Orders", "BoxesCount", DbType.Decimal.WithSize(19, 5), false);
            Database.ChangeColumn("Orders", "ConfirmedBoxesCount", DbType.Decimal.WithSize(19, 5), false);
        }
    }
}
