namespace GachaGame.Data
{
    public enum ElementType
    {
        Fire,
        Water,
        Grass,
        Light,
        Dark
    }

    public enum Rarity
    {
        OneStar,
        TwoStars,
        ThreeStars,
        FourStars,
        FiveStars
    }

    public enum StatType
    {
        HP,
        ATK,
        DEF,
        SPD,
        CritRate,
        CritDMG,
        EffectHit,
        EffectRes
    }

    public enum ItemType
    {
        Currency,
        Consumable,
        Material,
        Fragment
    }

    public enum GearSlotType
    {
        Weapon,
        Head,
        Body,
        Legs,
        Ring,
        Amulet
    }

    public enum CurrencyType
    {
        Gold,
        Gems,
        Stamina
    }

    public enum ShopLimitPeriod
    {
        None,
        Daily,
        Weekly
    }
}
