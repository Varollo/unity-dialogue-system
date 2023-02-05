using DS.Data;
using DS.Windows;
using UnityEditor;
using UnityEngine;

namespace DS.Inspectors
{
    [CustomEditor(typeof(DSDialogue))]
    internal class DSDialogueEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Open in Dialogue Graph Editor"))
            {
                DSEditorWindow.Open(target as DSDialogue);
            }
        }
    }
}
