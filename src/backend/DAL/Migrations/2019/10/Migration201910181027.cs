using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910181027)]
    public class FixTariffsColumnsCase : Migration
    {
        public override void Apply()
        {
            Database.RenameColumn("Tariffs", "FTLRate", "FtlRate");
            Database.RenameColumn("Tariffs", "LTLRate1", "LtlRate1");
            Database.RenameColumn("Tariffs", "LTLRate2", "LtlRate2");
            Database.RenameColumn("Tariffs", "LTLRate3", "LtlRate3");
            Database.RenameColumn("Tariffs", "LTLRate4", "LtlRate4");
            Database.RenameColumn("Tariffs", "LTLRate5", "LtlRate5");
            Database.RenameColumn("Tariffs", "LTLRate6", "LtlRate6");
            Database.RenameColumn("Tariffs", "LTLRate7", "LtlRate7");
            Database.RenameColumn("Tariffs", "LTLRate8", "LtlRate8");
            Database.RenameColumn("Tariffs", "LTLRate9", "LtlRate9");
            Database.RenameColumn("Tariffs", "LTLRate10", "LtlRate10");
            Database.RenameColumn("Tariffs", "LTLRate11", "LtlRate11");
            Database.RenameColumn("Tariffs", "LTLRate12", "LtlRate12");
            Database.RenameColumn("Tariffs", "LTLRate13", "LtlRate13");
            Database.RenameColumn("Tariffs", "LTLRate14", "LtlRate14");
            Database.RenameColumn("Tariffs", "LTLRate15", "LtlRate15");
            Database.RenameColumn("Tariffs", "LTLRate16", "LtlRate16");
            Database.RenameColumn("Tariffs", "LTLRate17", "LtlRate17");
            Database.RenameColumn("Tariffs", "LTLRate18", "LtlRate18");
            Database.RenameColumn("Tariffs", "LTLRate19", "LtlRate19");
            Database.RenameColumn("Tariffs", "LTLRate20", "LtlRate20");
            Database.RenameColumn("Tariffs", "LTLRate21", "LtlRate21");
            Database.RenameColumn("Tariffs", "LTLRate22", "LtlRate22");
            Database.RenameColumn("Tariffs", "LTLRate23", "LtlRate23");
            Database.RenameColumn("Tariffs", "LTLRate24", "LtlRate24");
            Database.RenameColumn("Tariffs", "LTLRate25", "LtlRate25");
            Database.RenameColumn("Tariffs", "LTLRate26", "LtlRate26");
            Database.RenameColumn("Tariffs", "LTLRate27", "LtlRate27");
            Database.RenameColumn("Tariffs", "LTLRate28", "LtlRate28");
            Database.RenameColumn("Tariffs", "LTLRate29", "LtlRate29");
            Database.RenameColumn("Tariffs", "LTLRate30", "LtlRate30");
            Database.RenameColumn("Tariffs", "LTLRate31", "LtlRate31");
            Database.RenameColumn("Tariffs", "LTLRate32", "LtlRate32");
            Database.RenameColumn("Tariffs", "LTLRate33", "LtlRate33");
        }
    }
}
