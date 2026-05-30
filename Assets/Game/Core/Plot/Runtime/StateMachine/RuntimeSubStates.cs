using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [Serializable]
    public abstract class RuntimeSubState
    {
        public string ID = Guid.NewGuid().ToString();
        public string Name; // Отображаемое имя в среднем окне
        
        // Локальный граф этого сабстейта (нижнее окно)
        [SerializeReference] public List<RuntimeNode> Nodes = new();
        public List<EdgeSaveData> Edges = new();

        // Абстрактный метод фильтрации: сабстейт сам решает, что в нем можно создать
        public abstract bool IsNodeAllowed(Type nodeType);
    }

    // --- Конкретные Сабстейты ---

    [Serializable]
    public class BriefingSubState : RuntimeSubState
    {
        public BriefingSubState() => Name = "Briefing";

        // В брифинге нужны только фразы и выборы
        public override bool IsNodeAllowed(Type nodeType) =>
            nodeType == typeof(RuntimePhraseNode) || 
            nodeType == typeof(RuntimeChoiceNode);
    }

    [Serializable]
    public class InvestigateSubState : RuntimeSubState
    {
        public InvestigateSubState() => Name = "Investigate";

        // В расследовании можно говорить, выбирать и находить улики/знания
        public override bool IsNodeAllowed(Type nodeType) =>
            nodeType == typeof(RuntimePhraseNode) ||
            nodeType == typeof(RuntimeChoiceNode);
    }

    [Serializable]
    public class PassiveWatchSubState : RuntimeSubState
    {
        public PassiveWatchSubState() => Name = "Passive Watch";

        public override bool IsNodeAllowed(Type nodeType) =>
            nodeType == typeof(RuntimePhraseNode); // Только наблюдение/слушание фразы
    }

    [Serializable]
    public class TradeSubState : RuntimeSubState
    {
        public TradeSubState() => Name = "Trade";

        // В торговле разрешены фразы, выборы и нода совершения/отмены заказа
        public override bool IsNodeAllowed(Type nodeType) =>
            nodeType == typeof(RuntimePhraseNode) || 
            nodeType == typeof(RuntimeChoiceNode) ||
            nodeType == typeof(RuntimeMakeOrderNode) ||
            nodeType == typeof(RuntimeCancelOrderNode);
    }
}