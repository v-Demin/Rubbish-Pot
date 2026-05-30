using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [Serializable]
    public abstract class RuntimeGlobalNode
    {
        public string ID = Guid.NewGuid().ToString();
        public Vector2 Position;
        public string Name;

        public List<LoadedCharacterData> PreloadedCharacters = new();
        public List<string> PreloadedBackgrounds = new();
        
        [SerializeReference] 
        public List<RuntimeSubState> SubStates = new();
        
        // Рантайм-трекинг: какой сабстейт сейчас активен внутри этой ноды
        public string ActiveSubStateID; 
    }

    // --- Системные глобальные ноды (Всегда есть на верхнем графе) ---
    
    [Serializable]
    public class GlobalEntryNode : RuntimeGlobalNode
    {
        public GlobalEntryNode()
        {
            Name = "Вход";
            SubStates.Add(new BriefingSubState());
        }
    }

    [Serializable]
    public class GlobalExitNode : RuntimeGlobalNode
    {
        public GlobalExitNode() => Name = "Выход";
    }

    // --- Конкретные глобальные ноды сценария ---

    [Serializable]
    public class OrderGlobalNode : RuntimeGlobalNode
    {
        public OrderGlobalNode()
        {
            Name = "Заказ";
            // Жестко определяем структуру: 3 сабстейта
            SubStates.Add(new InvestigateSubState());
            SubStates.Add(new PassiveWatchSubState());
        }
    }

    [Serializable]
    public class MerchantGlobalNode : RuntimeGlobalNode
    {
        public MerchantGlobalNode()
        {
            Name = "Торговец";
            // Жестко определяем структуру: 2 сабстейта
            SubStates.Add(new TradeSubState());
        }
    }
}