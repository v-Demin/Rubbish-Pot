using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RubbishPot.Core
{
    public class SubStateSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private SubStateGraphView _graphView;
        private RuntimeSubState _contextSubState;

        public void Init(SubStateGraphView graphView, RuntimeSubState contextSubState)
        {
            _graphView = graphView;
            _contextSubState = contextSubState;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent($"Доступные ноды для {_contextSubState.Name}"), 0)
            };

            // База данных всех доступных локальных нод в игре
            var allLocalNodes = new Dictionary<string, Type>
            {
                { "Диалоги/Фраза (Phrase Node)", typeof(RuntimePhraseNode) },
                { "Диалоги/Выбор (Choice Node)", typeof(RuntimeChoiceNode) },
                { "Заказы/Создать Заказ (Make Order)", typeof(RuntimeMakeOrderNode) },
                { "Заказы/Отменить Заказ (Cancel Order)", typeof(RuntimeCancelOrderNode) },
            };

            // Фильтруем список на основе логики самого Сабстейта!
            foreach (var pair in allLocalNodes)
            {
                if (_contextSubState.IsNodeAllowed(pair.Value))
                {
                    tree.Add(new SearchTreeEntry(new GUIContent(pair.Key)) { level = 1, userData = pair.Value });
                }
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var targetType = (Type)searchTreeEntry.userData;
            var nodeInstance = (RuntimeNode)Activator.CreateInstance(targetType);

            var windowRoot = _graphView.panel.visualTree;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot, context.screenMousePosition - _graphView.Window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            _graphView.AddLocalNode(nodeInstance, graphMousePosition);
            return true;
        }
    }
}