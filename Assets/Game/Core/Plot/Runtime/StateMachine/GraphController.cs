namespace RubbishPot.Core
{
    public class GraphController
    {
        private readonly GraphRuntimeInterpreter _interpreter;
        private readonly Plot _plot;

        public GraphController(GraphRuntimeInterpreter interpreter, Plot plot)
        {
            _interpreter = interpreter;
            _plot = plot;
            
            EventBus.Subscribe<NodeFinishedEvent>(OnNodeFinished);
            EventBus.Subscribe<NodeBranchEvent>(OnNodeBranch);
        }

        private void OnNodeFinished(NodeFinishedEvent e)
        {
            var edges = _plot.Edges.FindAll(ed => ed.OutputNodeID == e.NodeId);
            foreach (var edge in edges)
            {
                _interpreter.Execute(edge.InputNodeID);
            }
        }

        private void OnNodeBranch(NodeBranchEvent e)
        {
            var edge = _plot.Edges.Find(ed => ed.OutputNodeID == e.NodeId && ed.OutputPortIndex == e.BranchIndex);
            if (edge != null)
            {
                _interpreter.Execute(edge.InputNodeID);
            }
        }
    }
}
