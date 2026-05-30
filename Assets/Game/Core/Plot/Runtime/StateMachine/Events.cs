namespace RubbishPot.Core
{
    // Обычное завершение (для нод с одним выходом)
    public class NodeFinishedEvent
    {
        public string NodeId { get; }
        public NodeFinishedEvent(string nodeId) => NodeId = nodeId;
    }

    // Завершение с выбором ветки (УБРАЛИ НАСЛЕДОВАНИЕ)
    public class NodeBranchEvent
    {
        public string NodeId { get; }
        public int BranchIndex { get; }

        public NodeBranchEvent(string nodeId, int branchIndex)
        {
            NodeId = nodeId;
            BranchIndex = branchIndex;
        }
    }
}