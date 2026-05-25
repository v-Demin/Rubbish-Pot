using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [Serializable]
    public class PlotData
    {
        [SerializeReference]
        public List<RuntimeNode> Nodes = new();
        public List<EdgeSaveData> Edges = new();
    }
}
