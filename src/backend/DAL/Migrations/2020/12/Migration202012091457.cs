using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202012091457)]
    public class Migration202012091457 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Articles", new Column("TemperatureRegime", DbType.String, ColumnProperty.Null));
            Database.RemoveColumn("Articles", "Spgr");
            Database.RemoveColumn("Articles", "CountryOfOrigin");
            Database.RemoveColumn("Articles", "ShelfLife");
            Database.RemoveColumn("Articles", "Status");
            Database.RemoveColumn("Articles", "Ean");
            Database.RemoveColumn("Articles", "UnitLengthGoodsMm");
            Database.RemoveColumn("Articles", "WidthUnitsGoodsMm");
            Database.RemoveColumn("Articles", "UnitHeightGoodsMm");
            Database.RemoveColumn("Articles", "WeightUnitsGrossProductG");
            Database.RemoveColumn("Articles", "WeightUnitsNetGoodsG");
            Database.RemoveColumn("Articles", "EanShrink");
            Database.RemoveColumn("Articles", "PiecesInShrink");
            Database.RemoveColumn("Articles", "LengthShrinkMm");
            Database.RemoveColumn("Articles", "WidthShrinkMm");
            Database.RemoveColumn("Articles", "HeightShrinkMm");
            Database.RemoveColumn("Articles", "GrossShrinkWeightG");
            Database.RemoveColumn("Articles", "NetWeightShrinkG");
            Database.RemoveColumn("Articles", "EanBox");
            Database.RemoveColumn("Articles", "PiecesInABox");
            Database.RemoveColumn("Articles", "BoxLengthMm");
            Database.RemoveColumn("Articles", "WidthOfABoxMm");
            Database.RemoveColumn("Articles", "BoxHeightMm");
            Database.RemoveColumn("Articles", "GrossBoxWeightG");
            Database.RemoveColumn("Articles", "NetBoxWeightG");
            Database.RemoveColumn("Articles", "PiecesInALayer");
            Database.RemoveColumn("Articles", "LayerLengthMm");
            Database.RemoveColumn("Articles", "LayerWidthMm");
            Database.RemoveColumn("Articles", "LayerHeightMm");
            Database.RemoveColumn("Articles", "GrossLayerWeightMm");
            Database.RemoveColumn("Articles", "NetWeightMm");
            Database.RemoveColumn("Articles", "EanPallet");
            Database.RemoveColumn("Articles", "PiecesOnAPallet");
            Database.RemoveColumn("Articles", "PalletLengthMm");
            Database.RemoveColumn("Articles", "WidthOfPalletsMm");
            Database.RemoveColumn("Articles", "PalletHeightMm");
            Database.RemoveColumn("Articles", "GrossPalletWeightG");
            Database.RemoveColumn("Articles", "NetWeightPalletsG");
        }
    }
}