using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    #region Commands
    
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
    
    public class PreloadScenarioResourcesCommand : ICommand
    {
        public List<LoadedCharacterData> Characters { get; }
        public List<string> Backgrounds { get; }
    
        public PreloadScenarioResourcesCommand(List<LoadedCharacterData> characters, List<string> backgrounds)
        {
            Characters = characters;
            Backgrounds = backgrounds;
        }
    }

    #endregion
    
    #region Subcommands
    
    public interface ISubCommand : ICommand
    {
    }

    [System.Serializable]
    public class PlayShowAnimationCommand : ISubCommand
    {
        [SerializeField] private LoadedCharacterData _characterData;
        [SerializeField] private Character.ShowType _showType;

        public LoadedCharacterData CharacterInfo => _characterData;
        public Character.ShowType Animation => _showType;

        public PlayShowAnimationCommand() { }
        public PlayShowAnimationCommand(LoadedCharacterData characterInfo, Character.ShowType animation)
        {
            _characterData = characterInfo;
            _showType = animation;
        }
    }
    
    [System.Serializable]
    public class PlayEmotionAnimationCommand : ISubCommand
    {
        [SerializeField] private LoadedCharacterData _characterData;
        [SerializeField] private Character.AnimationState _emotion;
        [SerializeField] private bool _fast = false;
        
        public LoadedCharacterData CharacterInfo => _characterData;
        public Character.AnimationState Animation => _emotion;
        public bool Fast => _fast;
    
        public PlayEmotionAnimationCommand() { }
        public PlayEmotionAnimationCommand(LoadedCharacterData characterInfo, Character.AnimationState animation, bool fast)
        {
            _characterData = characterInfo;
            _emotion = animation;
            _fast = fast;
        }
    }
    
    #endregion
}
