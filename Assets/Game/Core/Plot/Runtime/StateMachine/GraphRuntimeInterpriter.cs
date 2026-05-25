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

        public Plot(PlotData data, string name)
        {
            Name = name;
            _data = data;
            _registry = new PlotRegistry();
            _controller = new GraphController(this, _data);
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _registry.Register<RuntimeEntryNode>(new EntryNodeHandler());
            _registry.Register<RuntimeExitNode>(new ExitNodeHandler());
            _registry.Register<RuntimePhraseNode>(new PhraseNodeHandler());
            _registry.Register<RuntimeAnimationNode>(new AnimationNodeHandler());
            _registry.Register<RuntimeChoiceNode>(new ChoiceNodeHandler());
            _registry.Register<RuntimeMakeOrderNode>(new MakeOrderNodeHandler());
            _registry.Register<RuntimeStorageNode>(new StorageNodeHandler());
            _registry.Register<RuntimeHandOverItemNode>(new HandOverItemNodeHandler());
            _registry.Register<RuntimeCancelOrderNode>(new CancelOrderNodeHandler());
            _registry.Register<RuntimeAddKnowledgeNode>(new AddKnowledgeNodeHandler());
            _registry.Register<RuntimeParallelNode>(new ParallelNodeHandler());
            _registry.Register<RuntimeGroupNode>(new GroupNodeHandler());
        }

        public void Execute()
        {
            var entryNode = _data.Nodes.OfType<RuntimeEntryNode>().FirstOrDefault();
            
            if (entryNode == null)
            {
                Debug.LogError($"[Plot] Ошибка старта '{Name}': В файле данных отсутствует EntryNode!");
                return;
            }

            Execute(entryNode.ID);
        }

        public void Stop()
        {
            _controller?.Dispose();
        }

        public void Execute(string nodeId)
        {
            var node = _data.Nodes.Find(n => n.ID == nodeId);
            if (node == null) return;
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