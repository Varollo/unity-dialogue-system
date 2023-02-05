using DS.Utilities;
using DS.Windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    internal class DSStartNode : DSSingleChoiceNode
    {
        public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dsGraphView, position);
            
            DialogueType = Enumerations.DSDialogueType.Start;
        }

        protected override void DrawNodeTitle()
        {
            Label titleLabel = new(SpeakerID);

            titleLabel.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(0, titleLabel);
        }

        protected override void DrawInputPorts() { }
        protected override void DrawTextArea() { }
    }
}
