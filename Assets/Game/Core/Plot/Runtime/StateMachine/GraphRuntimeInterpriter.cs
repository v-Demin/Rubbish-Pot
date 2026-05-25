namespace RubbishPot.Core
{
    public class GraphRuntimeInterpreter
    {
        private readonly Plot _plot;
        private readonly NodeHandlerRegistry _registry;

        public GraphRuntimeInterpreter(Plot plot)
        {
            _plot = plot;
            _registry = new NodeHandlerRegistry();
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

        public void Execute(string nodeId)
        {
            var node = _plot.Nodes.Find(n => n.ID == nodeId);
            if (node == null) return;
            _registry.GetHandler(node).Handle(node);
        }
    }
}