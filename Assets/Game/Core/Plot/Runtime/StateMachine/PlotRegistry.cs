using System;
using System.Collections.Generic;

namespace RubbishPot.Core
{
    public class PlotRegistry
    {
        private readonly Dictionary<Type, INodeHandler> _handlers = new();
        public void Register<T>(INodeHandler handler) where T : RuntimeNode => _handlers[typeof(T)] = handler;
        
        public INodeHandler GetHandler(RuntimeNode node)
        {
            if (_handlers.TryGetValue(node.GetType(), out var handler)) return handler;
            throw new Exception($"Нет хендлера для {node.GetType()}");
        }
    }
}
