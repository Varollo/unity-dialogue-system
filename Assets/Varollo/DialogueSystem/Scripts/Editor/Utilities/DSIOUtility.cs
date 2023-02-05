using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using DS.Data.Save;
using DS.Elements;
using DS.Windows;

using Edge = UnityEditor.Experimental.GraphView.Edge;
using DS.Data;

namespace DS.Utilities
{
    public static class DSIOUtility
    {
        public static DSDialogue Save(DSGraphView graph, string fileName)
        {
            if (!OpenSavePanel(fileName, out string path))
                return null;

            List<DSNodeSaveData> nodes = new();
            List<DSGroupSaveData> groups = new();
                        
            graph.graphElements.ForEach(graphElement =>
            {
                switch (graphElement)
                {
                    case DSNode node:
                        DSChoiceSaveData[] choices = new DSChoiceSaveData[node.Choices.Count];
                        node.Choices.CopyTo(choices);
                        nodes.Add(new()
                        {
                            ID = node.DialogueID,
                            Name = node.SpeakerID,
                            Choices = choices != null ? new(choices) : new(),
                            Text = node.Text,
                            GroupID = node.Group?.ID,
                            DialogueType = node.DialogueType,
                            Position = node.GetPosition().position
                        });
                        break;
                    case DSGroup group:
                        groups.Add(new()
                        {
                            ID = group.ID,
                            Name = group.title,
                            Position = group.GetPosition().position
                        });
                        break;
                }
            });

            return DSDialogue.GetOrCreateGraph(path, groups, nodes);
        }

        public static void Load(DSGraphView graph)
        {
            if (!OpenLoadPanel(out string path))
                return;

            DSDialogue graphData = LoadAsset<DSDialogue>(path);

            if (graphData != null)
                Load(graph, graphData);
        }

        public static void Load(DSGraphView graph, DSDialogue graphData)
        {
            DSEditorWindow.UpdateFileName(graphData.name);

            Dictionary<string, DSGroup> groups = new();
            Dictionary<string, DSNode> nodes = new();

            foreach (DSGroupSaveData groupData in (IEnumerable<DSGroupSaveData>)graphData)
            {
                DSGroup group = graph.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                groups.Add(group.ID, group);
            }

            foreach (DSNodeSaveData nodeData in (IEnumerable<DSNodeSaveData>)graphData)
            {
                DSChoiceSaveData[] choices = new DSChoiceSaveData[nodeData.Choices.Count];
                nodeData.Choices.CopyTo(choices);

                DSNode node = graph.CreateNode(nodeData.DialogueType, nodeData.Position, nodeData.Name, false);

                node.DialogueID = nodeData.ID;
                node.Choices = choices != null ? new(choices) : new();
                node.Text = nodeData.Text;

                nodes.Add(node.DialogueID, node);
                node.Draw();

                graph.AddElement(node);

                if (!string.IsNullOrEmpty(nodeData.GroupID))
                {
                    DSGroup group = groups[nodeData.GroupID];
                    node.Group = group;
                    group.AddElement(node);
                }
            }

            foreach (KeyValuePair<string, DSNode> loadedNode in nodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DSChoiceSaveData choiceData = (DSChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                    {
                        continue;
                    }

                    DSNode nextNode = nodes[choiceData.NodeID];

                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                    graph.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }
          
        public static T LoadAsset<T>(string path) where T : ScriptableObject
        {
            path = path.Remove(0, path.IndexOf("Assets"));
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static bool OpenSavePanel(string fileName, out string path)
        {
            try { path = EditorUtility.SaveFilePanel("Save Dialogue Graph", Application.dataPath, fileName, "asset"); if (string.IsNullOrEmpty(path)) throw new(); else return true; }
            catch { path = string.Empty; return false; }
        }

        private static bool OpenLoadPanel(out string path)
        {
            try { path = EditorUtility.OpenFilePanel("Save Dialogue Graph", Application.dataPath, "asset"); if (string.IsNullOrEmpty(path)) throw new(); else return true; }
            catch { path = string.Empty; return false; }
        }
    }
}