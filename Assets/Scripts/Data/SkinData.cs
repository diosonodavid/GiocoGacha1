using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "SkinData", menuName = "GachaGame/Data/Skin Data")]
    public class SkinData : ScriptableObject
    {
        public string skinId;
        public string skinName;
        public string characterId;
        public Sprite portraitSprite;
        public GameObject battleModelPrefab;
        public GearStat bonusStat;
    }
}
