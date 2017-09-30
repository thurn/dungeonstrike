namespace DungeonStrike.Source.Assets
{
    public class Materials
    {
        public static AssetReference MoveSelectorMaterial { get; }
            =
            new AssetReference("materials", "MoveSelectorMaterial");
    }

    public class SoldierRu
    {
        public static AssetReference BagsBlack { get; }
            =
            new AssetReference("soldier_ru", "Bags_black");
        public static AssetReference BagsGreen { get; }
            =
            new AssetReference("soldier_ru", "Bags_green");
        public static AssetReference HelmetBlack { get; }
            =
            new AssetReference("soldier_ru", "Helmet_black");
        public static AssetReference HelmetGreen { get; }
            =
            new AssetReference("soldier_ru", "Helmet_green");
        public static AssetReference SoldierRuBlack { get; }
            =
            new AssetReference("soldier_ru", "Soldier_ru_black");
        public static AssetReference SoldierRuForest { get; }
            =
            new AssetReference("soldier_ru", "Soldier_ru_forest");
        public static AssetReference SoldierRuCustomizable { get; }
            =
            new AssetReference("soldier_ru", "Soldier_ru_customizable");
        public static AssetReference VestBlack { get; }
            =
            new AssetReference("soldier_ru", "Vest_black");
        public static AssetReference VestGreen { get; }
            =
            new AssetReference("soldier_ru", "Vest_green");
    }

    public class Units
    {
        public static AssetReference Soldier { get; }
            =
            new AssetReference("units", "Soldier");
    }
}