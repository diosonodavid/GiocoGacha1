using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "SeasonData", menuName = "GachaGame/Data/Season Data")]
    public class SeasonData : ScriptableObject
    {
        public string seasonId;
        public string seasonName;
        public long startUnix;
        public long endUnix;
        [TextArea] public string rulesDescription;
    }
}
