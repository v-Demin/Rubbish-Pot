using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    public static class EventBus
    {
        // Внутренний контейнер, чтобы вызывать обобщенные экшены без медленного DynamicInvoke
        private class ListenerInfo
        {
            public Delegate OriginalListener;
            public Action<object> Invoker;
        }

        private static readonly Dictionary<Type, List<ListenerInfo>> Listeners = new();
        private static readonly Dictionary<Type, List<Type>> TypeHierarchyCache = new();

        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!Listeners.ContainsKey(type))
            {
                Listeners[type] = new List<ListenerInfo>();
            }

            // Создаем быструю обертку с нативным кастом
            var info = new ListenerInfo
            {
                OriginalListener = listener,
                Invoker = (obj) => listener((T)obj)
            };

            Listeners[type].Add(info);
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!Listeners.TryGetValue(type, out var list)) return;

            // Ищем обертку по ссылке на оригинальный делегат
            var item = list.Find(x => ReferenceEquals(x.OriginalListener, listener));
            if (item != null)
            {
                list.Remove(item);
            }
        }

        public static void Raise<T>(T eventArgs)
        {
            if (eventArgs == null) return;

            // Берем реальный тип объекта в рантайме, а не тип переменной из вызова
            var runtimeType = eventArgs.GetType();
            var targets = GetTypeHierarchy(runtimeType);

            // Идем по всей цепочке типов (сам класс -> родители -> интерфейсы)
            foreach (var type in targets)
            {
                if (!Listeners.TryGetValue(type, out var list)) continue;

                // Создаем копию списка, чтобы избежать экссепшенов, если кто-то отпишется во время обработки
                var snapshot = new List<ListenerInfo>(list);
                foreach (var info in snapshot)
                {
                    try
                    {
                        info.Invoker(eventArgs);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventBus] Ошибка выполнения листенера для события {type.Name}: {e.Message}\n{e.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Собирает и кэширует все типы, к которым можно привести данный тип (родители + интерфейсы)
        /// </summary>
        private static List<Type> GetTypeHierarchy(Type type)
        {
            if (TypeHierarchyCache.TryGetValue(type, out var cachedList))
            {
                return cachedList;
            }

            var hierarchy = new List<Type> { type };
            
            // Собираем базовые классы
            var baseType = type.BaseType;
            while (baseType != null)
            {
                hierarchy.Add(baseType);
                baseType = baseType.BaseType;
            }

            // Собираем интерфейсы
            hierarchy.AddRange(type.GetInterfaces());

            TypeHierarchyCache[type] = hierarchy;
            return hierarchy;
        }
    }
}