using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Data.Save;
    using Enumerations;
    using System;
    using Utilities;
    using Windows;

    public class DSMultipleChoiceNode : DSSingleChoiceNode
    {
        public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dsGraphView, position);
            DialogueType = DSDialogueType.MultipleChoice;

            DSChoiceSaveData choice = new(Guid.NewGuid().ToString()) { Text = "Next" };
            Choices = new() { { choice.ChoiceID, choice } };
        }

        protected override void DrawOutputPorts()
        {
            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
            {
                DSChoiceSaveData choiceData = new(Guid.NewGuid().ToString()) { Text = "New Choice" };

                Choices.Add(choiceData.ChoiceID, choiceData);
                outputContainer.Add(CreateChoicePort(Choices.Count - 1, choiceData));
            });

            addChoiceButton.AddToClassList("ds-node__button");
            mainContainer.Insert(1, addChoiceButton);

            base.DrawOutputPorts();
        }

        protected override Port CreateChoicePort(int choiceID, object userData)
        {
            Port choicePort = base.CreateChoicePort(choiceID, userData);

            if(choiceID > 0)
                DrawDeleteChoiceButton(choicePort, (DSChoiceSaveData)userData);

            return choicePort;
        }

        protected virtual void DrawDeleteChoiceButton(Port choicePort, DSChoiceSaveData choiceData)
        {
            Button deleteChoiceButton = DSElementUtility.CreateButton("x", () =>
            {
                if (Choices.Count == 1)
                {
                    return;
                }

                if (choicePort.connected)
                {
                    GraphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData.ChoiceID);

                GraphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("ds-node__button-delete");
            choicePort.Add(deleteChoiceButton);
        }
    }
}