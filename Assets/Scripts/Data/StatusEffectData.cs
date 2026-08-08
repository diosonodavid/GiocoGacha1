using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "StatusEffectData", menuName = "GachaGame/Data/Status Effect Data")]
    public class StatusEffectData : ScriptableObject
    {
        public string effectId;
        public string effectName;
        public Sprite icon;
        public int duration;
        public bool isBuff;
        public StatModifier modifier;
    }
}
