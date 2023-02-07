using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Windows
{
    using Data.Error;
    using Data.Save;
    using Elements;
    using Enumerations;
    using System.Linq;
    using System.Xml.Linq;
    using Utilities;

    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;
        private MiniMap miniMap;

        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;

            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddMiniMap();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();

            SetStartNode(CreateNode(DSDialogueType.Start, Vector2.zero));
        }

        internal DSNode StartNode { get; private set; }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DSDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DSDialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }

        public DSGroup CreateGroup(string title, Vector2 position)
        {
            DSGroup group = new DSGroup(title, position);
            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DSNode))
                {
                    continue;
                }

                DSNode node = (DSNode) selectedElement;

                group.AddElement(node);
            }

            return group;
        }

        public DSNode CreateNode(DSDialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"DS.Elements.DS{dialogueType}Node");

            DSNode node = (DSNode)Activator.CreateInstance(nodeType);

            node.Initialize(node.SpeakerID, this, position);

            if (shouldDraw)
            {
                node.Draw();
            }

            return node;
        }

        public DSNode CreateNode(DSDialogueType dialogueType, Vector2 position, string speakerID, bool shouldDraw = true)
        {
            DSNode node = CreateNode(dialogueType, position, shouldDraw);
            node.SpeakerID = speakerID;
            return node;
        }

        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            node.Group = group;
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (element is DSNode node && !node.Equals(StartNode))
                    {
                        DSGroup dsGroup = (DSGroup)group;
                        AddGroupedNode(node, dsGroup);
                    }
                }
            };
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DSGroup);
                Type edgeType = typeof(Edge);

                List<DSGroup> groupsToDelete = new List<DSGroup>();
                List<DSNode> nodesToDelete = new List<DSNode>();
                List<Edge> edgesToDelete = new List<Edge>();

                foreach (GraphElement selectedElement in selection.OfType<GraphElement>())
                {
                    switch (selectedElement)
                    {
                        case DSNode node:
                            nodesToDelete.Add(node);
                            break;
                        case Edge edge:
                            edgesToDelete.Add(edge);
                            break;
                        case DSGroup group:
                            groupsToDelete.Add(group);
                            break;
                    }
                }

                for (int i = groupsToDelete.Count - 1; i >= 0; i--)
                {
                    DSGroup groupToDelete = groupsToDelete[i];
                    DSNode[] nodesToRemove = groupToDelete.containedElements.OfType<DSNode>().ToArray();

                    groupToDelete.RemoveElements(nodesToRemove);
                    RemoveElement(groupToDelete);
                }

                DeleteElements(edgesToDelete);

                for (int i = nodesToDelete.Count - 1; i >= 0; i--)
                {
                    DSNode nodeToDelete = nodesToDelete[i];

                    if(!nodeToDelete.Equals(StartNode))
                    {
                        nodeToDelete.DisconnectAllPorts();
                        RemoveElement(nodeToDelete);
                    }
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                elements.OfType<DSNode>().Select(node => node.Group = null);
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = (DSGroup) group;

                dsGroup.title = newTitle;

                dsGroup.OldTitle = dsGroup.title;
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode) edge.input.node;
                        DSNode prevNode = (DSNode) edge.output.node;

                        string choiceID = ((DSChoiceSaveData)edge.output.userData).ChoiceID;
                        DSChoiceSaveData choice = prevNode.Choices[choiceID];

                        choice.NodeID = nextNode.DialogueID;
                        prevNode.Choices[choiceID] = choice;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge) element;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData) edge.output.userData;

                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
            }

            searchWindow.Initialize(this);

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DSGraphViewStyles.uss",
                "DialogueSystem/DSNodeStyles.uss"
            );
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => 
            {
                if (graphElement is not DSStartNode)
                    RemoveElement(graphElement); 
            });
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }

        public void SetStartNode(DSNode node)
        {
            if(StartNode != null)
                RemoveElement(StartNode);

            AddElement(StartNode = node);
        }
    }
}