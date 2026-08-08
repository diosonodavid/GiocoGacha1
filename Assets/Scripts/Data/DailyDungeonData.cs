using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class GuaranteedDrop
    {
        public ItemData item;
        public int amount;
    }

    [CreateAssetMenu(fileName = "DailyDungeonData", menuName = "GachaGame/Data/Daily Dungeon Data")]
    public class DailyDungeonData : ScriptableObject
    {
        public string dungeonId;
        public string dungeonName;
        public DungeonType dungeonType;
        public List<DayOfWeek> openDays = new();
        public int staminaCost;
        public List<GuaranteedDrop> guaranteedDrops = new();
        public List<CharacterBaseData> listNemici = new();
    }
}
