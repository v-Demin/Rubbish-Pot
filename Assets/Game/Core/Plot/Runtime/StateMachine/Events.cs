using UnityEngine;

namespace RubbishPot.Core
{
// Базовые события управления графом для Контроллера
    public class NodeFinishedEvent 
    { 
        public string NodeId { get; }
        public NodeFinishedEvent(string nodeId) => NodeId = nodeId;
    }

    public class NodeBranchEvent : NodeFinishedEvent 
    { 
        public int BranchIndex { get; }
        public NodeBranchEvent(string nodeId, int branchIndex) : base(nodeId) => BranchIndex = branchIndex;
    }
}
