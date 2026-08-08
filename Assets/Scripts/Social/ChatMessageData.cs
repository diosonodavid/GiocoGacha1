using System;

namespace GachaGame.Social
{
    public enum ChatChannel
    {
        World,
        Guild,
        Private,
        System
    }

    [Serializable]
    public class ChatMessageData
    {
        public ChatChannel channel;
        public string senderId;
        public string senderName;
        public string recipientId; // only populated for the Private channel
        public string message;
        public long sentAtUnix;
    }
}
