using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView; // Используется ради интерфейса ISearchWindowProvider
using UnityEngine;

namespace RubbishPot.Core
{
    public class GlobalSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private GlobalGraphView _globalGraphView;
        private Texture2D _indentationIcon;

        public void Init(GlobalGraphView globalGraphView)
        {
            _globalGraphView = globalGraphView;
            
            // Прозрачная иконка-заглушка для красивых отступов в меню Unity
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Создать глобальный этап"), 0),
                
                new SearchTreeEntry(new GUIContent("Этап Торговца (Merchant)", _indentationIcon)) 
                    { level = 1, userData = typeof(MerchantGlobalNode) },
                
                new SearchTreeEntry(new GUIContent("Этап Заказа (Order)", _indentationIcon)) 
                    { level = 1, userData = typeof(OrderGlobalNode) }
            };
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData is Type concreteNodeType)
            {
                // Теперь сюда прилетит, например, typeof(SimpleGlobalNode),
                // у которого есть конструктор по умолчанию, и Activator сработает без ошибок!
                _globalGraphView.AddGlobalNode(concreteNodeType);
                return true;
            }

            return false;
        }
    }
}