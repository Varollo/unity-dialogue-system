using DS.Data.Save;
using System.Linq;

namespace DS.Data
{
    public class DSDialogue
    {
        private readonly DSDialogueGraph _graphObject;
        private readonly DSNodeSaveData _saveData;

        internal DSDialogue(DSDialogueGraph graphObject, DSNodeSaveData saveData)
        {
            _graphObject = graphObject;
            _saveData = saveData;

            Speaker = _saveData.Name;
            Text = _saveData.Text;
            Group = _graphObject.GetGroup(_saveData.GroupID);
            Choices = _saveData.Choices.Select(choice => new DSChoice(graphObject, choice)).ToArray();
        }

        public string Speaker { get; set; }
        public string Text { get; set; }
        public string Group { get; set; }
        public DSChoice[] Choices { get; set; }

        public struct DSChoice
        {
            private readonly DSDialogueGraph _graphObject;
            private readonly DSChoiceSaveData _saveData;

            public DSChoice(DSDialogueGraph graphObject, DSChoiceSaveData saveData) : this()
            {
                _graphObject = graphObject;
                _saveData = saveData;

                Text = _saveData.Text;
                Next = string.IsNullOrEmpty(_saveData.NodeID) ? null : _graphObject.GetDialogue(saveData.NodeID);
            }

            public string Text { get; set; }
            public DSDialogue Next { get; set; }
        }
    }
}
