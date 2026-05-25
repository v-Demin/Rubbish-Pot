using System;
using System.Collections.Generic;

namespace RubbishPot.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Listeners = new();

        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!Listeners.ContainsKey(type)) Listeners[type] = new List<Delegate>();
            Listeners[type].Add(listener);
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!Listeners.TryGetValue(type, out var listeners)) return;
            
            listeners.Remove(listener);
        }

        public static void Raise<T>(T eventArgs)
        {
            var type = typeof(T);
            if (!Listeners.TryGetValue(type, out var listeners)) return;
            
            foreach (var listener in new List<Delegate>(listeners))
            {
                ((Action<T>)listener)?.Invoke(eventArgs);
            }
        }
    }
}
