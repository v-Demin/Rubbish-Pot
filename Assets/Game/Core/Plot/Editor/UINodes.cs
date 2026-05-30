using System.Linq;
using UnityEngine.UIElements;

namespace RubbishPot.Core
{
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;

    public abstract class BaseGraphViewNode : Node
    {
        // Быстрый доступ к ID и позиции без кастов
        public abstract string NodeID { get; }
        public abstract RuntimeNode GetRuntimeNode();
        public abstract void UpdatePosition();
    }

    public abstract class GraphViewNode<T> : BaseGraphViewNode where T : RuntimeNode
    {
        public T RuntimeTarget { get; private set; } 
        public override string NodeID => RuntimeTarget.ID;

        // По умолчанию для всех нод вход — Single
        protected virtual Port.Capacity InputCapacity => Port.Capacity.Single;

        public GraphViewNode(T runtimeTarget)
        {
            RuntimeTarget = runtimeTarget;
            viewDataKey = runtimeTarget.ID;
            SetPosition(new Rect(runtimeTarget.Position, Vector2.zero));
        
            if (runtimeTarget is not RuntimeEntryNode)
            {
                // Используем наше виртуальное свойство вместо хардкода Single
                inputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Input, InputCapacity, typeof(float)));
            }
            BuildCustomUI();
        }

        protected abstract void BuildCustomUI();
        public override RuntimeNode GetRuntimeNode() => RuntimeTarget;
        public override void UpdatePosition() => RuntimeTarget.Position = GetPosition().position;
    }
    
    public class EntryUiNode : GraphViewNode<RuntimeEntryNode>
    {
        public EntryUiNode(RuntimeEntryNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "ENTRY (START)";
            style.backgroundColor = new Color(0.15f, 0.4f, 0.15f);
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
            capabilities &= ~Capabilities.Deletable;
        }
    }
    
    public class ExitUiNode : GraphViewNode<RuntimeExitNode>
    {
        public ExitUiNode(RuntimeExitNode t) : base(t) { }

        // Перегружаем емкость входа: теперь сюда можно воткнуть бесконечно много стрелок
        protected override Port.Capacity InputCapacity => Port.Capacity.Multi;

        protected override void BuildCustomUI()
        {
            title = "EXIT (END)";
            style.backgroundColor = new Color(0.5f, 0.15f, 0.15f);
        
            // У Exit-ноды нет выходов, контейнер output остаётся пустым
            capabilities &= ~Capabilities.Deletable; // Запрет на удаление
        }
    }
    
    public class PhraseUiNode : GraphViewNode<RuntimePhraseNode>
    {
        protected override Port.Capacity InputCapacity => Port.Capacity.Multi;

        public PhraseUiNode(RuntimePhraseNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Character Phrase";
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
        
            var field = new TextField("Dialogue Text") { value = RuntimeTarget.Text, multiline = true };
            field.RegisterValueChangedCallback(evt => RuntimeTarget.Text = evt.newValue);
            extensionContainer.Add(field);
            RefreshExpandedState();
        }
    }
    
    
    public class ChoiceUiNode : GraphViewNode<RuntimeChoiceNode>
    {
        public ChoiceUiNode(RuntimeChoiceNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Player Choice";
            var btn = new Button(() => AddChoicePort("")) { text = "Add Option" };
            titleContainer.Add(btn);

            foreach (var choice in RuntimeTarget.Choices) AddChoicePort(choice);
        }

        private void AddChoicePort(string val)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            var field = new TextField { value = val };
            field.RegisterValueChangedCallback(evt => {
                int idx = outputContainer.IndexOf(port);
                RuntimeTarget.Choices[idx] = evt.newValue;
                port.portName = evt.newValue;
            });
            port.contentContainer.Add(field);
            outputContainer.Add(port);

            if (string.IsNullOrEmpty(val)) RuntimeTarget.Choices.Add("");
            RefreshPorts(); RefreshExpandedState();
        }
    }
    
    public class MakeOrderUiNode : GraphViewNode<RuntimeMakeOrderNode>
    {
        public MakeOrderUiNode(RuntimeMakeOrderNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Make Order";
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
        
            var field = new TextField("Order ID") { value = RuntimeTarget.OrderID };
            field.RegisterValueChangedCallback(evt => RuntimeTarget.OrderID = evt.newValue);
            extensionContainer.Add(field);
            RefreshExpandedState();
        }
    }
    
    public class CancelOrderUiNode : GraphViewNode<RuntimeCancelOrderNode>
    {
        public CancelOrderUiNode(RuntimeCancelOrderNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Cancel Order Handle";
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
        }
    }
}
