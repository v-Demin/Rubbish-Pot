using System;

namespace RubbishPot.Core
{
    public class GraphController : IDisposable
    {
        private readonly IInternalPlot _plot; // Работаем через внутренний интерфейс
        private readonly PlotData _plotData;

        public GraphController(IInternalPlot plot, PlotData plotData)
        {
            _plot = plot;
            _plotData = plotData;
            
            EventBus.Subscribe<NodeFinishedEvent>(OnNodeFinished);
            EventBus.Subscribe<NodeBranchEvent>(OnNodeBranch);
        }

        private void OnNodeFinished(NodeFinishedEvent e)
        {
            var edges = _plotData.Edges.FindAll(ed => ed.OutputNodeID == e.NodeId);
            foreach (var edge in edges)
            {
                _plot.Execute(edge.InputNodeID); // Вызываем внутренний метод по ID
            }
        }

        private void OnNodeBranch(NodeBranchEvent e)
        {
            var edge = _plotData.Edges.Find(ed => ed.OutputNodeID == e.NodeId && ed.OutputPortIndex == e.BranchIndex);
            if (edge != null)
            {
                _plot.Execute(edge.InputNodeID); // Вызываем внутренний метод по ID
            }
        }

        // Защита от «Мертвых душ» в EventBus при смене сценариев
        public void Dispose()
        {
            EventBus.Unsubscribe<NodeFinishedEvent>(OnNodeFinished);
            EventBus.Unsubscribe<NodeBranchEvent>(OnNodeBranch);
        }
    }
}
