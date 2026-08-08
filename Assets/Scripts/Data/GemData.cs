using UnityEngine;

namespace GachaGame.Data
{
    public enum GemSocketColor
    {
        Red,
        Blue,
        Yellow
    }

    // Instances are created at runtime via ScriptableObject.CreateInstance<GemData>() when a gem
    // drops or is combined (see GemCombineService), not authored as pre-existing project assets -
    // same convention as RuneData, so combining or socketing one never mutates a shared definition.
    [CreateAssetMenu(fileName = "GemData", menuName = "GachaGame/Data/Gem Data")]
    public class GemData : ItemData
    {
        public GemSocketColor socketColor;
        public GearStat statBonus;
        public int gemLevel = 1;
    }
}
