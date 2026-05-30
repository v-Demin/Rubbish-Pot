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
            // Приводим интерфейс к конкретному классу Plot, чтобы получить доступ к текущему стейту
            if (_plot is Plot runtimePlot)
            {
                // 1. Сначала ищем локальные связи внутри текущего активного сабстейта
                if (runtimePlot.ActiveSubState != null)
                {
                    var localEdges = runtimePlot.ActiveSubState.Edges.FindAll(ed => ed.OutputNodeID == e.NodeId);
                    foreach (var edge in localEdges)
                    {
                        _plot.Execute(edge.InputNodeID);
                    }
                    
                    // Если нашли локальные переходы, значит мы внутри диалога/цепочки команд, выходим
                    if (localEdges.Count > 0) return; 
                }

                // 2. Если локальных связей нет, значит локальный граф завершился (например, дошли до ExitNode),
                // и это сигнал для выхода на глобальную карту связей верхнего уровня
                var globalEdges = _plotData.GlobalEdges.FindAll(ed => ed.OutputNodeID == e.NodeId);
                foreach (var edge in globalEdges)
                {
                    var nextGlobalNode = _plotData.GlobalNodes.Find(n => n.ID == edge.InputNodeID);
                    if (nextGlobalNode != null)
                    {
                        // Переключаем глобальную ноду на верхнем уровне
                        runtimePlot.ExecuteGlobalNode(nextGlobalNode);
                    }
                }
            }
        }

        private void OnNodeBranch(NodeBranchEvent e)
        {
            // Ветки выбора (BranchIndex для ChoiceNode) всегда работают локально внутри сабстейтов
            if (_plot is Plot runtimePlot && runtimePlot.ActiveSubState != null)
            {
                var edge = runtimePlot.ActiveSubState.Edges.Find(ed => 
                    ed.OutputNodeID == e.NodeId && ed.OutputPortIndex == e.BranchIndex);
                    
                if (edge != null)
                {
                    _plot.Execute(edge.InputNodeID);
                }
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