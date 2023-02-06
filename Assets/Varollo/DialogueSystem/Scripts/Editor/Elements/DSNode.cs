using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Data.Save;
    using Enumerations;
    using System.Linq;
    using UnityEditor.TestTools.TestRunner.Api;
    using Utilities;
    using Windows;

    public class DSNode : Node
    {
        protected DSGraphView GraphView { get; set; }
        public string DialogueID { get; set; }
        public string SpeakerID { get; set; } = "Speaker";
        public Dictionary<string, DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; } = "Dialogue text.";
        public DSDialogueType DialogueType { get; set; }
        public DSGroup Group { get; set; }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }

        public virtual void Initialize(string speakerID, DSGraphView dsGraphView, Vector2 position)
        {
            DialogueID = Guid.NewGuid().ToString();
            SpeakerID = speakerID;
            GraphView = dsGraphView;

            DSChoiceSaveData defaultChoice = new(Guid.NewGuid().ToString()) { Text = "Next" };
            Choices = new() { { defaultChoice.ChoiceID, defaultChoice } };

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            SetPosition(new Rect(position, Vector2.zero));
        }

        public virtual void Draw()
        {
            DrawNodeTitle();
            DrawTextArea();
            DrawInputPorts();
            DrawOutputPorts();
            RefreshExpandedState();
        }

        protected virtual void DrawTextArea()
        {
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(textTextField);

            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }

        protected virtual void DrawNodeTitle()
        {
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(SpeakerID, null, callback =>
            {
                TextField target = (TextField)callback.target;

                target.value = callback.newValue;
            });

            dialogueNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(0, dialogueNameTextField);
        }

        protected virtual void DrawInputPorts()
        {
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);
        }

        protected virtual void DrawOutputPorts()
        {
            DSChoiceSaveData[] choiceArray = Choices.Values.ToArray();
            for (int i = 0; i < choiceArray.Length; i++)
            {
                Port choicePort = CreateChoicePort(i, choiceArray[i]);
                outputContainer.Add(choicePort);
            }
        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
                if (port.connected)
                    GraphView.DeleteElements(port.connections);
        }

        protected virtual Port CreateChoicePort(int portID, object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            DSChoiceSaveData choiceData = (DSChoiceSaveData)userData;

            TextField choiceTextField = DSElementUtility.CreateTextField(choiceData.Text, null, callback => OnChoiceTextChanges(choiceData.ChoiceID, callback.newValue));

            choiceTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__choice-text-field"
            );

            choicePort.Insert(0, choiceTextField);

            return choicePort;
        }

        protected virtual void OnChoiceTextChanges(string choiceID, string newText)
        {
            DSChoiceSaveData choice = Choices[choiceID];
            choice.Text = newText;
            Choices[choiceID] = choice;
        }
    }
}