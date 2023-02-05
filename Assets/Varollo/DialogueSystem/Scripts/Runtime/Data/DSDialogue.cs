using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DS.Data.Save;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DS.Data
{
    public class DSDialogue : ScriptableObject, IEnumerable<DSGroupSaveData>, IEnumerable<DSNodeSaveData>
    {
        [field: SerializeField] internal string FileName { get; private set; }
        [field: SerializeField] internal List<DSGroupSaveData> Groups { get; private set; }
        [field: SerializeField] internal List<DSNodeSaveData> Nodes { get; private set; }

        IEnumerator<DSGroupSaveData> IEnumerable<DSGroupSaveData>.GetEnumerator() => Groups?.GetEnumerator();
        IEnumerator<DSNodeSaveData> IEnumerable<DSNodeSaveData>.GetEnumerator() => Nodes?.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Groups?.GetEnumerator();

#if UNITY_EDITOR
        public static DSDialogue GetOrCreateGraph(string relativePath, IEnumerable<DSGroupSaveData> groupData = null, IEnumerable<DSNodeSaveData> nodeData = null)
        {
            relativePath = relativePath.Remove(0, relativePath.IndexOf("Assets"));

            DSDialogue asset = AssetDatabase.LoadAssetAtPath<DSDialogue>(relativePath);

            if (asset == null)
            {
                asset = CreateInstance<DSDialogue>();
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