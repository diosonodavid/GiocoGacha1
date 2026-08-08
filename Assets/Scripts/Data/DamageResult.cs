using System;

namespace GachaGame.Data
{
    [Serializable]
    public struct DamageResult
    {
        public int damageAmount;
        public bool isCritical;
        public ElementType element;
        public bool isWeakness;
    }
}
