using UnityEngine;

namespace RubbishPot.Core
{
    [System.Serializable]
    public class EdgeSaveData
    {
        public string OutputNodeID;
        public string InputNodeID;
        public string PortName;
        public int OutputPortIndex;
    }
}
