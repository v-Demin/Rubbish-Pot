using System;
using System.Collections.Generic;

namespace RubbishPot.Core
{
    public static class InteractionHub
    {
        private static readonly Dictionary<Type, object> _handlers = new();
        public static void Register<T>(T handler) where T : class => _handlers[typeof(T)] = handler;
        public static void Unregister<T>() => _handlers.Remove(typeof(T));
        public static T Get<T>() where T : class => _handlers.TryGetValue(typeof(T), out var h) ? (T)h : null;
    }
}
