using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Social;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class ChatUI : UIController
    {
        [SerializeField] private List<GameObject> channelTabPanels = new();
        [SerializeField] private Transform messageListContainer;
        [SerializeField] private GameObject messageEntryPrefab;
        [SerializeField] private InputField messageInputField;

        private ChatManager chatManager;
        private PlayerReportService reportService;
        private ChatChannel activeChannel = ChatChannel.World;

        private string localPlayerId;
        private string localPlayerName;

        public void Bind(string playerId, string playerName)
        {
            localPlayerId = playerId;
            localPlayerName = playerName;
        }

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out chatManager);
            ServiceLocator.Instance.TryGet(out reportService);

            if (chatManager != null) chatManager.OnMessageReceived += HandleMessageReceived;

            ShowChannel(ChatChannel.World);
        }

        protected override void OnHidden()
        {
            if (chatManager != null) chatManager.OnMessageReceived -= HandleMessageReceived;
        }

        public void ShowChannel(ChatChannel channel)
        {
            activeChannel = channel;
            for (int i = 0; i < channelTabPanels.Count; i++)
                channelTabPanels[i].SetActive(i == (int)channel);

            RebuildHistory();
        }

        public async void HandleSendPressed()
        {
            if (chatManager == null || messageInputField == null) return;

            string text = messageInputField.text;
            if (string.IsNullOrWhiteSpace(text)) return;

            await chatManager.SendMessageAsync(activeChannel, localPlayerId, localPlayerName, text);
            messageInputField.text = string.Empty;
        }

        private void HandleMessageReceived(ChatMessageData message)
        {
            if (message.channel != activeChannel) return;
            if (reportService != null && reportService.IsBlocked(message.senderId)) return;

            AppendEntry(message);
        }

        private void RebuildHistory()
        {
            ClearContainer();
            if (chatManager == null) return;

            foreach (var message in chatManager.GetHistory(activeChannel))
            {
                if (reportService != null && reportService.IsBlocked(message.senderId)) continue;
                AppendEntry(message);
            }
        }

        private void AppendEntry(ChatMessageData message)
        {
            if (messageListContainer == null || messageEntryPrefab == null) return;

            var entry = Instantiate(messageEntryPrefab, messageListContainer);
            var label = entry.GetComponentInChildren<Text>();
            if (label != null) label.text = $"[{message.senderName}] {message.message}";
        }

        private void ClearContainer()
        {
            if (messageListContainer == null) return;
            for (int i = messageListContainer.childCount - 1; i >= 0; i--)
                Destroy(messageListContainer.GetChild(i).gameObject);
        }
    }
}
