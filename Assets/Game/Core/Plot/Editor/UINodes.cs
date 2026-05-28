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
    
    public class AnimationUiNode : GraphViewNode<RuntimeAnimationNode>
    {
        public AnimationUiNode(RuntimeAnimationNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Play Animation";
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
        
            var field = new TextField("Anim Name") { value = RuntimeTarget.AnimationName };
            field.RegisterValueChangedCallback(evt => RuntimeTarget.AnimationName = evt.newValue);
            extensionContainer.Add(field);
            RefreshExpandedState();
        }
    }
    
    public class GroupUiNode : GraphViewNode<RuntimeGroupNode>
    {
        public GroupUiNode(RuntimeGroupNode t) : base(t) { }

        protected override void BuildCustomUI()
        {
            title = "Group Sync (Wait All)";
            style.backgroundColor = new Color(0.15f, 0.15f, 0.35f);

            if (RuntimeTarget.Position == Vector2.zero || outputContainer.childCount == 0)
            {
                AddDynamicOutputPort();
            }
        }

        public void AddDynamicOutputPort()
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = "+"; 
            outputContainer.Add(port);
        
            RefreshPorts();
            RefreshExpandedState();
        }

        public void CleanUpPorts()
        {
            var allOutputPorts = outputContainer.Query<Port>().ToList();
        
            // 1. Сначала переименовываем все подключенные порты в нормальные имена
            for (int i = 0; i < allOutputPorts.Count; i++)
            {
                if (allOutputPorts[i].connected)
                {
                    allOutputPorts[i].portName = "Out";
                }
            }

            // 2. Схлопываем лишние пустые порты, оставляя только ОДИН "+" в самом конце
            for (int i = allOutputPorts.Count - 2; i >= 0; i--)
            {
                var port = allOutputPorts[i];
                if (!port.connected)
                {
                    outputContainer.Remove(port);
                }
            }

            // 3. Если вдруг так вышло, что последний порт перестал быть плюсом (был занят, а пустой исчез), чиним это
            allOutputPorts = outputContainer.Query<Port>().ToList();
            if (allOutputPorts.Count == 0 || allOutputPorts.Last().connected)
            {
                AddDynamicOutputPort();
            }

            RefreshPorts();
            RefreshExpandedState();
        }
    }
    
    public class ParallelUiNode : GraphViewNode<RuntimeParallelNode>
    {
        public ParallelUiNode(RuntimeParallelNode t) : base(t) { }

        protected override void BuildCustomUI()
        {
            title = "Parallel Split (Auto-Ports)";
            style.backgroundColor = new Color(0.3f, 0.3f, 0.15f);

            if (RuntimeTarget.Position == Vector2.zero || outputContainer.childCount == 0)
            {
                AddDynamicOutputPort();
            }
        }

        public void AddDynamicOutputPort()
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = "+"; 
            outputContainer.Add(port);
        
            RefreshPorts();
            RefreshExpandedState();
        }

        public void CleanUpPorts()
        {
            var allOutputPorts = outputContainer.Query<Port>().ToList();
        
            // 1. Сначала переименовываем все подключенные порты в нормальные имена
            for (int i = 0; i < allOutputPorts.Count; i++)
            {
                if (allOutputPorts[i].connected)
                {
                    allOutputPorts[i].portName = "Out";
                }
            }

            // 2. Схлопываем лишние пустые порты, оставляя только ОДИН "+" в самом конце
            for (int i = allOutputPorts.Count - 2; i >= 0; i--)
            {
                var port = allOutputPorts[i];
                if (!port.connected)
                {
                    outputContainer.Remove(port);
                }
            }

            // 3. Если вдруг так вышло, что последний порт перестал быть плюсом (был занят, а пустой исчез), чиним это
            allOutputPorts = outputContainer.Query<Port>().ToList();
            if (allOutputPorts.Count == 0 || allOutputPorts.Last().connected)
            {
                AddDynamicOutputPort();
            }

            RefreshPorts();
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
    
    public class StorageUiNode : GraphViewNode<RuntimeStorageNode>
    {
        public StorageUiNode(RuntimeStorageNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Storage Inject (No Logic)";
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
        
            var field = new TextField("Storage Key") { value = RuntimeTarget.DataKey };
            field.RegisterValueChangedCallback(evt => RuntimeTarget.DataKey = evt.newValue);
            extensionContainer.Add(field);
            RefreshExpandedState();
        }
    }
    
    public class HandOverItemUiNode : GraphViewNode<RuntimeHandOverItemNode>
    {
        public HandOverItemUiNode(RuntimeHandOverItemNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Hand Over Item (No Logic)";
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
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
    
    public class AddKnowledgeUiNode : GraphViewNode<RuntimeAddKnowledgeNode>
    {
        public AddKnowledgeUiNode(RuntimeAddKnowledgeNode t) : base(t) { }
        protected override void BuildCustomUI()
        {
            title = "Add Knowledge (No Logic)";
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));
        }
    }
}
