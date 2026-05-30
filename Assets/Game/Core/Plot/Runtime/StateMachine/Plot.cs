using System.Linq;
using UnityEngine;

namespace RubbishPot.Core
{
    public class Plot : IInternalPlot
    {
        public string Name { get; }
        private readonly PlotData _data;
        private readonly PlotRegistry _registry;
        private readonly GraphController _controller;

        // Свойства для отслеживания текущего контекста выполнения
        public RuntimeGlobalNode CurrentGlobalNode { get; private set; }
        public RuntimeSubState ActiveSubState { get; private set; }

        public Plot(PlotData data, string name)
        {
            Name = name;
            _data = data;
            _registry = new PlotRegistry();
            
            // Передаем ссылку на этот Plot в контроллер, чтобы он управлял переходами
            _controller = new GraphController(this, _data);
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _registry.Register<RuntimeEntryNode>(new EntryNodeHandler());
            _registry.Register<RuntimeExitNode>(new ExitNodeHandler());
            _registry.Register<RuntimePhraseNode>(new PhraseNodeHandler());
            _registry.Register<RuntimeChoiceNode>(new ChoiceNodeHandler());
            _registry.Register<RuntimeMakeOrderNode>(new MakeOrderNodeHandler());
            _registry.Register<RuntimeCancelOrderNode>(new CancelOrderNodeHandler());
        }

        public void Execute()
        {
            // 1. Старт рантайма теперь начинается с Глобальной Входной Ноды
            var entryGlobalNode = _data.GlobalNodes.FirstOrDefault(n => n is GlobalEntryNode);
            
            if (entryGlobalNode == null)
            {
                Debug.LogError($"[Plot] Ошибка старта '{Name}': В глобальных данных отсутствует GlobalEntryNode!");
                return;
            }
            
            EventBus.Raise(new PreloadScenarioResourcesCommand(
                entryGlobalNode.PreloadedCharacters, 
                entryGlobalNode.PreloadedBackgrounds
            ));

            // 2. Переходим на эту глобальную ноду
            ExecuteGlobalNode(entryGlobalNode);
        }

        // Метод переключения крупных этапов (вызывается при переходах на верхнем уровне)
        public void ExecuteGlobalNode(RuntimeGlobalNode globalNode)
        {
            CurrentGlobalNode = globalNode;
            
            // Запускаем первый сабстейт глобальной ноды по умолчанию
            if (globalNode.SubStates != null && globalNode.SubStates.Count > 0)
            {
                SetActiveSubState(globalNode.SubStates[0]);
            }
            else
            {
                Debug.Log($"[Plot] Достигнута глобальная нода без подсостояний: {globalNode.Name}");
            }
        }

        // Метод активации конкретного сабстейта внутри глобальной ноды
        public void SetActiveSubState(RuntimeSubState subState)
        {
            ActiveSubState = subState;
            if (ActiveSubState == null) return;

            // Находим локальную точку входа (EntryNode) ИМЕННО внутри этого сабстейта
            var entryNode = ActiveSubState.Nodes.OfType<RuntimeEntryNode>().FirstOrDefault();
            
            if (entryNode != null)
            {
                Execute(entryNode.ID); // Запускаем цепочку локальных команд
            }
            else
            {
                Debug.LogWarning($"[Plot] В сабстейте '{subState.Name}' отсутствует локальный RuntimeEntryNode!");
            }
        }

        public void Stop()
        {
            _controller?.Dispose();
        }

        // Выполнение локальной ноды (вызывается хендлерами и контроллером)
        public void Execute(string nodeId)
        {
            if (ActiveSubState == null) return;

            // Ищем ноду строго в границах активного сабстейта
            var node = ActiveSubState.Nodes.Find(n => n.ID == nodeId);
            
            if (node == null)
            {
                // Если не нашли локально, проверяем, вдруг это сквозной ID глобальной ноды
                var globalNode = _data.GlobalNodes.Find(n => n.ID == nodeId);
                if (globalNode != null)
                {
                    ExecuteGlobalNode(globalNode);
                    return;
                }
                return;
            }

            foreach (var command in node.SubCommands)
            {
                EventBus.Raise(command);
            }
            
            _registry.GetHandler(node).Handle(node);
        }
    }
    
    public interface IPlot
    {
        string Name { get; }
        void Execute();
        void Stop();
    }

    public interface IInternalPlot : IPlot
    {
        void Execute(string nodeId); 
    }
}