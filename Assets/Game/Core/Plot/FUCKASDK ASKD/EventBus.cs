using UnityEngine;

namespace RubbishPot.Core
{
    using System;
    using System.Collections.Generic;

// Интерфейс команды
    public interface ICommand
    {
        void Execute();
    }

// Центральный шина событий (EventBus)
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _listeners = new();

        public static void Subscribe<T>(Action<T> listener) where T : ICommand
        {
            var type = typeof(T);
            if (!_listeners.ContainsKey(type))
            {
                _listeners[type] = new List<Delegate>();
            }
            _listeners[type].Add(listener);
        }

        public static void Unsubscribe<T>(Action<T> listener) where T : ICommand
        {
            var type = typeof(T);
            if (_listeners.ContainsKey(type))
            {
                _listeners[type].Remove(listener);
            }
        }

        // Публикация команды в шину
        public static void Raise<T>(T command) where T : ICommand
        {
            var type = typeof(T);
            if (_listeners.ContainsKey(type))
            {
                // Копируем список, чтобы избежать ошибок при отписке во время итерации
                var listenersCopy = new List<Delegate>(_listeners[type]);
                foreach (var listener in listenersCopy)
                {
                    ((Action<T>)listener).Invoke(command);
                }
            }
        
            // Сама команда также может выполнить свою внутреннюю логику, если необходимо
            command.Execute();
        }
    }
}
