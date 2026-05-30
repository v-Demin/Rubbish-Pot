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
        private Button _addBtn;

        public ChoiceUiNode(RuntimeChoiceNode t) : base(t) { }

        protected override void BuildCustomUI()
        {
            title = "Player Choice";
            
            // Инициализируем кнопку добавления и сохраняем на неё ссылку
            _addBtn = new Button(() => CreateAndRegisterNewChoice()) { text = "Add Option" };
            titleContainer.Add(_addBtn);

            // Отрисовываем уже существующие порты
            foreach (var choice in RuntimeTarget.Choices) 
            {
                BuildChoicePortUI(choice);
            }

            // Проверяем лимит при первой загрузке ноды
            UpdateAddButtonState();
        }

        /// <summary>
        /// Безопасное добавление нового варианта (с проверкой лимита)
        /// </summary>
        private void CreateAndRegisterNewChoice()
        {
            if (RuntimeTarget.Choices.Count >= 4) return;

            RuntimeTarget.Choices.Add(""); 
            BuildChoicePortUI("");
            
            // Обновляем состояние кнопки после добавления
            UpdateAddButtonState();
        }

        /// <summary>
        /// Отрисовка порта с текстовым полем и кнопкой удаления
        /// </summary>
        private void BuildChoicePortUI(string val)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            
            // Текстовое поле ответа
            var field = new TextField { value = val };
            field.style.flexGrow = 1; // Растягиваем поле, чтобы оно занимало всё доступное место
            
            field.RegisterValueChangedCallback(evt => {
                int idx = outputContainer.IndexOf(port);
                if (idx >= 0 && idx < RuntimeTarget.Choices.Count)
                {
                    RuntimeTarget.Choices[idx] = evt.newValue;
                    port.portName = evt.newValue;
                }
            });

            // Кнопка удаления ("X")
            var deleteBtn = new Button(() => DeleteChoicePort(port)) { text = "X" };
            deleteBtn.style.color = Color.red;
            deleteBtn.style.backgroundColor = new Color(0.25f, 0.1f, 0.1f);
            deleteBtn.style.marginLeft = 4;

            // Складываем всё в контейнер порта
            port.contentContainer.Add(field);
            port.contentContainer.Add(deleteBtn);
            
            outputContainer.Add(port);

            RefreshPorts(); 
            RefreshExpandedState();
        }

        /// <summary>
        /// Метод удаления конкретного порта и чистки рантайм-данных
        /// </summary>
        private void DeleteChoicePort(Port port)
        {
            // Находим точный индекс порта в контейнере прямо сейчас
            int idx = outputContainer.IndexOf(port);
            
            if (idx >= 0 && idx < RuntimeTarget.Choices.Count)
            {
                RuntimeTarget.Choices.RemoveAt(idx);
            }
            
            // Удаляем сам порт из интерфейса GraphView
            outputContainer.Remove(port);
            
            // Перерисовываем связи и интерфейс ноды
            RefreshPorts(); 
            RefreshExpandedState();
            
            // Включаем кнопку добавления обратно, если вариантов стало меньше 4
            UpdateAddButtonState();
        }

        /// <summary>
        /// Проверка лимита на 4 опции
        /// </summary>
        private void UpdateAddButtonState()
        {
            if (_addBtn != null)
            {
                // SetEnabled(true) если меньше 4, иначе кнопка становится серой и некликабельной
                _addBtn.SetEnabled(RuntimeTarget.Choices.Count < 4);
            }
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
