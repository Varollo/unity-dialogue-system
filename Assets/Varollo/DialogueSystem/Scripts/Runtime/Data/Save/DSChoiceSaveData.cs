using System;
using UnityEngine;

namespace DS.Data.Save
{
    [Serializable]
    public struct DSChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}