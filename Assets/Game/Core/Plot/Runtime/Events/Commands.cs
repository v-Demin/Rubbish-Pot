using System.Collections.Generic;

namespace RubbishPot.Core
{

    public interface ICommand
    {
    }

// Базовая команда ноды (чтобы система знала, какая именно нода её вызвала)
    public abstract class BaseNodeCommand : ICommand
    {
        public string NodeId { get; protected set; }
    }

// 1. Фраза персонажа
    public class PlayPhraseCommand : BaseNodeCommand
    {
        public string Text { get; }

        public PlayPhraseCommand(string nodeId, string text)
        {
            NodeId = nodeId;
            Text = text;
        }
    }

// 2. Анимация
    public class PlayAnimationCommand : BaseNodeCommand
    {
        public string AnimationName { get; }

        public PlayAnimationCommand(string nodeId, string animName)
        {
            NodeId = nodeId;
            AnimationName = animName;
        }
    }

// 3. Выбор игрока (генерирует UI с кнопками)
    public class ShowPlayerChoicesCommand : BaseNodeCommand
    {
        public List<string> Choices { get; }

        public ShowPlayerChoicesCommand(string nodeId, List<string> choices)
        {
            NodeId = nodeId;
            Choices = choices;
        }
    }

// 4. Совершение заказа
    public class MakeOrderCommand : BaseNodeCommand
    {
        public string OrderId { get; }

        public MakeOrderCommand(string nodeId, string orderId)
        {
            NodeId = nodeId;
            OrderId = orderId;
        }
    }

// 5. Выставление данных в хранилище (с поддержкой Dependency Injection / Data Pass)
    public class SetStorageDataCommand<T> : BaseNodeCommand
    {
        public string Key { get; }
        public T Value { get; } // Сюда доходит переданный inject

        public SetStorageDataCommand(string nodeId, string key, T value)
        {
            NodeId = nodeId;
            Key = key;
            Value = value;
        }
    }

// 6. Отказ от заказа (включает/выключает кнопку в UI)
    public class ToggleCancelOrderButtonCommand : BaseNodeCommand
    {
        public bool IsVisible { get; }

        public ToggleCancelOrderButtonCommand(string nodeId, bool isVisible)
        {
            NodeId = nodeId;
            IsVisible = isVisible;
        }
    }

// 7. Системная команда: Нода завершила работу
// Нужна для Групповой и Параллельной нод, чтобы они знали, когда идти дальше
    public class NodeCompletedCommand : ICommand
    {
        public string NodeId { get; }

        public NodeCompletedCommand(string nodeId)
        {
            NodeId = nodeId;
        }
    }
}
