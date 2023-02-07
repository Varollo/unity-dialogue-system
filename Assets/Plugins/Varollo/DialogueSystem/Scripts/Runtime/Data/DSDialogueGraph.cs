using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DS.Data.Save;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DS.Data
{
    public class DSDialogueGraph : ScriptableObject, IEnumerable<DSGroupSaveData>, IEnumerable<DSNodeSaveData>
    {
        [field: SerializeField] internal string FileName { get; private set; }
        [field: SerializeField] internal List<DSGroupSaveData> Groups { get; private set; }
        [field: SerializeField] internal List<DSNodeSaveData> Nodes { get; private set; }

        private Dictionary<string, DSGroupSaveData> _groupDictionary;
        private Dictionary<string, DSNodeSaveData> _nodeDictionary;

        public DSDialogue GetDialogue() => GetDialogue(Nodes[0].Choices[0].NodeID);
        public DSDialogue GetDialogue(int orderInList) => new(this, Nodes[orderInList]);
        
        internal DSDialogue GetDialogue(string nodeID) => new(this, (_nodeDictionary ??= Nodes.ToDictionary(node => node.ID))[nodeID]);
        internal string GetGroup(string groupID) => string.IsNullOrEmpty(groupID) ? string.Empty : (_groupDictionary ??= Groups.ToDictionary(group => group.ID))[groupID].Name;

        IEnumerator<DSGroupSaveData> IEnumerable<DSGroupSaveData>.GetEnumerator() => Groups?.GetEnumerator();
        IEnumerator<DSNodeSaveData> IEnumerable<DSNodeSaveData>.GetEnumerator() => Nodes?.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Groups?.GetEnumerator();

#if UNITY_EDITOR
        public static DSDialogueGraph GetOrCreateGraph(string relativePath, IEnumerable<DSGroupSaveData> groupData = null, IEnumerable<DSNodeSaveData> nodeData = null)
        {
            relativePath = relativePath.Remove(0, relativePath.IndexOf("Assets"));

            DSDialogueGraph asset = AssetDatabase.LoadAssetAtPath<DSDialogueGraph>(relativePath);

            if (asset == null)
            {
                asset = CreateInstance<DSDialogueGraph>();
                AssetDatabase.CreateAsset(asset, relativePath);
            }

            asset.FileName = asset.name;
            asset.Groups = groupData != null ? new(groupData) : new();
            asset.Nodes = nodeData != null ? new(nodeData) : new();

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }
#endif
    }
}