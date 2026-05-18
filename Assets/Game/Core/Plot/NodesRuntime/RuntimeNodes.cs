using UnityEngine;

namespace RubbishPot.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public abstract class RuntimeNode
    {
        public string ID = Guid.NewGuid().ToString();
        public Vector2 Position; // Храним позицию прямо тут для редактора
    
        public abstract void Execute(GraphRuntimeInterpreter interpreter);
    }

    [Serializable]
    public class RuntimeEntryNode : RuntimeNode
    {
        public override void Execute(GraphRuntimeInterpreter interpreter) => interpreter.Proceed(ID);
    }

    [Serializable]
    public class RuntimeExitNode : RuntimeNode
    {
        public override void Execute(GraphRuntimeInterpreter interpreter) => EventBus.Raise(new NodeCompletedCommand(ID));
    }

    [Serializable]
    public class RuntimePhraseNode : RuntimeNode
    {
        public string Text;
        public override void Execute(GraphRuntimeInterpreter interpreter) => EventBus.Raise(new PlayPhraseCommand(ID, Text));
    }
    
    [Serializable]
    public class RuntimeAnimationNode : RuntimeNode
    {
        public string AnimationName;
        public override void Execute(GraphRuntimeInterpreter interpreter) => EventBus.Raise(new PlayAnimationCommand(ID, AnimationName));
    }
    
    [Serializable]
    public class RuntimeGroupNode : RuntimeNode
    {
        [NonSerialized] private int _completedCount = 0;
        [NonSerialized] private int _totalExpected = 0;

        public override void Execute(GraphRuntimeInterpreter interpreter)
        {
            // Логика счёта завершений инкапсулирована в интерпретаторе/самой ноде при параллельном проходе
            interpreter.ExecuteAllOutputs(ID);
        }
    }
    
    [Serializable]
    public class RuntimeParallelNode : RuntimeNode
    {
        public override void Execute(GraphRuntimeInterpreter interpreter) => interpreter.ExecuteAllOutputs(ID);
    }

    [Serializable]
    public class RuntimeChoiceNode : RuntimeNode
    {
        public List<string> Choices = new();
        public override void Execute(GraphRuntimeInterpreter interpreter) => EventBus.Raise(new ShowPlayerChoicesCommand(ID, Choices));
    }
    
    [Serializable]
    public class RuntimeMakeOrderNode : RuntimeNode
    {
        public string OrderID;
        public override void Execute(GraphRuntimeInterpreter interpreter) => EventBus.Raise(new MakeOrderCommand(ID, OrderID));
    }
    
    [Serializable]
    public class RuntimeStorageNode : RuntimeNode
    {
        public string DataKey;
        public override void Execute(GraphRuntimeInterpreter interpreter)
        {
            int sampleInjectedData = 777; // Сюда прилетает Inject
            EventBus.Raise(new SetStorageDataCommand<int>(ID, DataKey, sampleInjectedData));
            interpreter.Proceed(ID); // Пропускаем дальше мгновенно
        }
    }
    
    [Serializable]
    public class RuntimeHandOverItemNode : RuntimeNode
    {
        public override void Execute(GraphRuntimeInterpreter interpreter) => interpreter.Proceed(ID); 
    }
    
    [Serializable]
    public class RuntimeCancelOrderNode : RuntimeNode
    {
        public override void Execute(GraphRuntimeInterpreter interpreter) => EventBus.Raise(new ToggleCancelOrderButtonCommand(ID, true));
    }
    
    [Serializable]
    public class RuntimeAddKnowledgeNode : RuntimeNode
    {
        public override void Execute(GraphRuntimeInterpreter interpreter) => interpreter.Proceed(ID);
    }
}
