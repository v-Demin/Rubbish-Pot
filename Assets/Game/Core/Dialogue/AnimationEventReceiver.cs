using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [RequireComponent(typeof(Animator))]
    public class AnimationEventReceiver : MonoBehaviour
    {
        private readonly Dictionary<string, Action> _eventCallbacks = new();

        /// <summary>
        /// Регистрирует действие на определенный маркер анимации.
        /// Если на один маркер подписываются несколько систем, они добавятся в цепочку.
        /// </summary>
        public void RegisterEvent(string eventKey, Action callback)
        {
            if (string.IsNullOrEmpty(eventKey) || callback == null) return;

            if (_eventCallbacks.ContainsKey(eventKey))
            {
                _eventCallbacks[eventKey] += callback;
            }
            else
            {
                _eventCallbacks[eventKey] = callback;
            }
        }

        /// <summary>
        /// Отписка от события
        /// </summary>
        public void UnregisterEvent(string eventKey, Action callback)
        {
            if (string.IsNullOrEmpty(eventKey) || callback == null) return;

            if (_eventCallbacks.ContainsKey(eventKey))
            {
                _eventCallbacks[eventKey] -= callback;
                
                // Если подписчиков больше нет, чистим ключ
                if (_eventCallbacks[eventKey] == null)
                {
                    _eventCallbacks.Remove(eventKey);
                }
            }
        }

        /// <summary>
        /// Очистить вообще все подписки (полезно при смене сцены или глобальном сбросе)
        /// </summary>
        public void ClearAllEvents()
        {
            _eventCallbacks.Clear();
        }

        /// <summary>
        /// ВНИМАНИЕ: Этот метод НАМЕРТВО завязан на Unity Animation Events.
        /// Имя этого метода нужно вписывать в инспекторе анимационного клипа.
        /// </summary>
        public void OnAnimationEventTriggered(string eventKey)
        {
            if (string.IsNullOrEmpty(eventKey)) return;

            if (_eventCallbacks.TryGetValue(eventKey, out var callback))
            {
                // Вызываем все подписанные экшены
                callback?.Invoke();
            }
        }
    }
}