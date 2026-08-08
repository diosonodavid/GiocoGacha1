using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextDialogueId;
    }

    [CreateAssetMenu(fileName = "DialogueData", menuName = "GachaGame/Data/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;
        public string speakerName;
        public Sprite speakerPortrait;
        [TextArea] public string text;
        public AudioClip audioClip;
        public string nextDialogueId;
        public List<DialogueChoice> choices = new();
    }
}
