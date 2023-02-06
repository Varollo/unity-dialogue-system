using System;
using UnityEngine;

namespace DS.Data.Save
{
    [Serializable]
    public struct DSChoiceSaveData
    {
        public DSChoiceSaveData(string choiceID) : this() => ChoiceID = choiceID;

        [field: SerializeField] public string ChoiceID { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
        [field: SerializeField] public string Text { get; set; }

        public void SetNextNode(string dialogueID)
        {
            NodeID = dialogueID;
        }

        public void SetText(string text)
        {
            Text = text;
        }
    }
}