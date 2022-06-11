using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003050717)]
    public class Migration202003050717 : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery($@"
                delete from ""FieldPropertyItems""
                where ""ForEntity"" = 1 
                    and ""FieldName"" in ('basicDeliveryCostWithoutVAT', 'totalDeliveryCost', 'totalDeliveryCostWithoutVAT')
            ");
        }
    }
}
