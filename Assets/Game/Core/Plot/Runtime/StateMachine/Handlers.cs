namespace RubbishPot.Core
{
public interface INodeHandler { void Handle(RuntimeNode node); }

    // Специфичные интерфейсы управления для интерактивных нод
    public interface IPhraseNodeHandler { void CompletePhrase(string nodeId); }
    public interface IChoiceNodeHandler { void SubmitChoice(string nodeId, int choiceIndex); }
    public interface ICancelOrderNodeHandler { void ExecuteCancel(string nodeId); void SkipCancel(string nodeId); }

    // Реализация всех обработчиков
    public class EntryNodeHandler : INodeHandler 
    { 
        public void Handle(RuntimeNode n) => EventBus.Raise(new NodeFinishedEvent(n.ID)); 
    }
    
    public class ExitNodeHandler : INodeHandler 
    { 
        public void Handle(RuntimeNode n) 
        {
            EventBus.Raise(new NodeCompletedCommand(n.ID));
            EventBus.Raise(new NodeFinishedEvent(n.ID)); 
        }
    }

    public class PhraseNodeHandler : INodeHandler, IPhraseNodeHandler 
    { 
        public void Handle(RuntimeNode n) 
        { 
            InteractionHub.Register<IPhraseNodeHandler>(this);
            EventBus.Raise(new PlayPhraseCommand(n.ID, ((RuntimePhraseNode)n).Text)); 
        } 
        public void CompletePhrase(string nodeId)
        {
            InteractionHub.Unregister<IPhraseNodeHandler>();
            EventBus.Raise(new NodeFinishedEvent(nodeId));
        }
    }

    public class AnimationNodeHandler : INodeHandler 
    { 
        public void Handle(RuntimeNode n) { 
            EventBus.Raise(new PlayAnimationCommand(n.ID, ((RuntimeAnimationNode)n).AnimationName)); 
            EventBus.Raise(new NodeFinishedEvent(n.ID)); 
        } 
    }

    public class ChoiceNodeHandler : INodeHandler, IChoiceNodeHandler 
    {
        public void Handle(RuntimeNode n) {
            InteractionHub.Register<IChoiceNodeHandler>(this);
            EventBus.Raise(new ShowPlayerChoicesCommand(n.ID, ((RuntimeChoiceNode)n).Choices));
        }
        public void SubmitChoice(string nodeId, int choiceIndex) {
            InteractionHub.Unregister<IChoiceNodeHandler>();
            EventBus.Raise(new NodeBranchEvent(nodeId, choiceIndex));
        }
    }

    public class MakeOrderNodeHandler : INodeHandler 
    { 
        public void Handle(RuntimeNode n) { 
            EventBus.Raise(new MakeOrderCommand(n.ID, ((RuntimeMakeOrderNode)n).OrderID)); 
            EventBus.Raise(new NodeFinishedEvent(n.ID)); 
        } 
    }

    public class StorageNodeHandler : INodeHandler 
    { 
        public void Handle(RuntimeNode n) { 
            EventBus.Raise(new SetStorageDataCommand<int>(n.ID, ((RuntimeStorageNode)n).DataKey, 777)); 
            EventBus.Raise(new NodeFinishedEvent(n.ID)); 
        } 
    }

    public class HandOverItemNodeHandler : INodeHandler { public void Handle(RuntimeNode n) => EventBus.Raise(new NodeFinishedEvent(n.ID)); }

    public class CancelOrderNodeHandler : INodeHandler, ICancelOrderNodeHandler 
    { 
        public void Handle(RuntimeNode n) { 
            InteractionHub.Register<ICancelOrderNodeHandler>(this);
            EventBus.Raise(new ToggleCancelOrderButtonCommand(n.ID, true)); 
            EventBus.Raise(new NodeFinishedEvent(n.ID)); // Пролетаем дальше, но оставляем активным интерфейс отмены в хабе
        } 
        public void ExecuteCancel(string nodeId)
        {
            InteractionHub.Unregister<ICancelOrderNodeHandler>();
            EventBus.Raise(new ToggleCancelOrderButtonCommand(nodeId, false));
            EventBus.Raise(new NodeBranchEvent(nodeId, 1)); // Идем по ветке отмены (Порт 1)
        }
        public void SkipCancel(string nodeId)
        {
            InteractionHub.Unregister<ICancelOrderNodeHandler>();
            EventBus.Raise(new ToggleCancelOrderButtonCommand(nodeId, false));
        }
    }

    public class AddKnowledgeNodeHandler : INodeHandler { public void Handle(RuntimeNode n) => EventBus.Raise(new NodeFinishedEvent(n.ID)); }
    public class ParallelNodeHandler : INodeHandler { public void Handle(RuntimeNode n) => EventBus.Raise(new NodeFinishedEvent(n.ID)); }
    public class GroupNodeHandler : INodeHandler { public void Handle(RuntimeNode n) => EventBus.Raise(new NodeFinishedEvent(n.ID)); }
}
