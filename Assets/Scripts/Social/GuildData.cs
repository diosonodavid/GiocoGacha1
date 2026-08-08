using System;

namespace GachaGame.Social
{
    [Serializable]
    public class GuildData
    {
        public string guildId;
        public string guildName;
        public int level;
        public int memberCount;
        public string guildLeaderId;
    }
}
