using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RubbishPot.Core
{
    public class GlobalGraphView : VisualElement
    {
        public StateMachineEditorWindow Window { get; private set; }
        private List<RuntimeGlobalNode> _nodes;
        private VisualElement _container;
        private PlotData _plotData; 

        public GlobalGraphView(StateMachineEditorWindow window)
        {
            Window = window;
            style.flexGrow = 1;
            
            _container = new VisualElement();
            _container.style.flexDirection = FlexDirection.Row;
            _container.style.flexWrap = Wrap.Wrap;
            _container.style.paddingTop = 10;
            _container.style.paddingBottom = 10;
            _container.style.paddingLeft = 10;
            _container.style.paddingRight = 10;
            Add(_container);
        }

        /// <summary>
        /// Инициализация и отрисовка верхнего окна. Авто-генерирует Вход/Выход, если сценарий пустой.
        /// </summary>
        public void PopulateView(PlotData data)
        {
            _plotData = data;
            
            if (_plotData != null)
            {
                if (_plotData.GlobalNodes == null)
                {
                    _plotData.GlobalNodes = new List<RuntimeGlobalNode>();
                }

                // Авто-генерация глобального Входа и Выхода, если граф пустой
                if (_plotData.GlobalNodes.Count == 0)
                {
                    _plotData.GlobalNodes.Add(new GlobalEntryNode { ID = Guid.NewGuid().ToString(), Name = "Вход" });
                    _plotData.GlobalNodes.Add(new GlobalExitNode { ID = Guid.NewGuid().ToString(), Name = "Выход" });
                    
                    // Пересобираем связи для дефолтного состояния (Вход -> Выход)
                    RebuildGlobalEdges();

                    if (Window != null && Window.CurrentAsset != null)
                    {
                        EditorUtility.SetDirty(Window.CurrentAsset);
                    }
                }
                
                _nodes = _plotData.GlobalNodes;
            }
            else
            {
                _nodes = new List<RuntimeGlobalNode>();
            }

            Refresh(_nodes);
        }

        /// <summary>
        /// Создание новой кастомной глобальной ноды через верхнее меню редактора
        /// </summary>
        public void AddGlobalNode(Type concreteNodeType)
        {
            if (_plotData == null || _plotData.GlobalNodes == null)
            {
                Debug.LogWarning("[GlobalGraphView] Не удалось добавить ноду: Сценарий не загружен!");
                return;
            }

            // Защита от дублирования системных нод
            if (concreteNodeType == typeof(GlobalEntryNode) || concreteNodeType == typeof(GlobalExitNode))
            {
                EditorUtility.DisplayDialog("Ошибка", "Системные ноды Входа и Выхода уже созданы автоматически!", "ОК");
                return;
            }

            var newNode = (RuntimeGlobalNode)Activator.CreateInstance(concreteNodeType);
            
            // Намертво генерируем уникальный ID, чтобы рантайм мог найти эту ноду по Edges
            newNode.ID = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(newNode.Name))
            {
                newNode.Name = concreteNodeType.Name.Replace("GlobalNode", "").Replace("Node", "") + " Stage";
            }

            // Вставляем строго ПЕРЕД Выходом (Выход всегда сдвигается в самый конец списка)
            int insertIndex = Mathf.Max(1, _plotData.GlobalNodes.Count - 1);
            _plotData.GlobalNodes.Insert(insertIndex, newNode);
            
            // ПРИНУДИТЕЛЬНЫЙ РЕБИЛД: В цепочку встал новый элемент, обновляем связи для рантайма
            RebuildGlobalEdges();

            if (Window != null && Window.CurrentAsset != null)
            {
                EditorUtility.SetDirty(Window.CurrentAsset);
            }
            
            PopulateView(_plotData);
        }

        /// <summary>
        /// Очистка старых UI элементов карточек и создание новых
        /// </summary>
        public void Refresh(List<RuntimeGlobalNode> nodes)
        {
            _nodes = nodes;
            _container.Clear();

            if (_nodes == null) return;

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                var nodeElement = BuildNodeElement(node, i, _nodes.Count);
                _container.Add(nodeElement);
            }
        }

        /// <summary>
        /// Сборка UI-карточки для конкретной глобальной ноды
        /// </summary>
        private VisualElement BuildNodeElement(RuntimeGlobalNode node, int index, int totalCount)
        {
            var card = new VisualElement();
            card.style.width = 220;
            
            card.style.marginLeft = 6; card.style.marginRight = 6;
            card.style.marginTop = 6; card.style.marginBottom = 6;
            
            card.style.paddingLeft = 8; card.style.paddingRight = 8;
            card.style.paddingTop = 8; card.style.paddingBottom = 8;
            
            card.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            card.style.borderLeftWidth = 1; card.style.borderRightWidth = 1;
            card.style.borderTopWidth = 1; card.style.borderBottomWidth = 1;
            
            var borderCol = new Color(0.12f, 0.12f, 0.12f, 1f);
            card.style.borderLeftColor = borderCol; card.style.borderRightColor = borderCol;
            card.style.borderTopColor = borderCol; card.style.borderBottomColor = borderCol;
            
            card.style.borderTopLeftRadius = 5; card.style.borderTopRightRadius = 5;
            card.style.borderBottomLeftRadius = 5; card.style.borderBottomRightRadius = 5;

            System.Action<EventBase> handleCardClick = (evt) =>
            {
                if (evt.target == card || !(evt.target is Button))
                {
                    Window.UpdateInspector(node);
                    evt.StopPropagation();
                }
            };

            card.RegisterCallback<PointerDownEvent>(evt => handleCardClick(evt));
            card.RegisterCallback<MouseDownEvent>(evt => handleCardClick(evt));

            var titleLabel = new Label(node.Name);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 4;
            titleLabel.style.fontSize = 13;
            card.Add(titleLabel);

            var typeLabel = new Label(node.GetType().Name);
            typeLabel.style.fontSize = 10;
            typeLabel.style.opacity = 0.4f;
            typeLabel.style.marginBottom = 10;
            card.Add(typeLabel);

            bool isStart = node is GlobalEntryNode;
            bool isEnd = node is GlobalExitNode;

            // --- ИСПРАВЛЕНО: Рендерим кнопки ТОЛЬКО если у ноды ПРЯМО СЕЙЧАС есть сабстейты ---
            if (node.SubStates != null && node.SubStates.Count > 0)
            {
                var subStateContainer = new VisualElement();
                subStateContainer.style.marginTop = 5;
                subStateContainer.style.marginBottom = 10;

                foreach (var subState in node.SubStates)
                {
                    if (subState == null) continue;

                    var btn = new Button(() =>
                    {
                        // Передаем ТЕКУЩУЮ глобальную ноду как родителя, и сам сабстейт
                        Window.SetActiveSubState(node, subState);
                        Window.UpdateInspector(subState); 
                    })
                    {
                        text = $"-> {subState.Name}"
                    };

                    btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    btn.style.fontSize = 11;
                    btn.style.marginTop = 2; btn.style.marginBottom = 2;
                    btn.style.paddingLeft = 5;

                    btn.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
                    btn.RegisterCallback<PointerUpEvent>(evt => evt.StopPropagation());
                    btn.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
                    btn.RegisterCallback<MouseUpEvent>(evt => evt.StopPropagation());

                    subStateContainer.Add(btn);
                }
                
                card.Add(subStateContainer);
            }

            // Панель управления (Стрелочки влево/вправо и удаление ноды)
            var controlPanel = new VisualElement();
            controlPanel.style.flexDirection = FlexDirection.Row;
            controlPanel.style.justifyContent = Justify.SpaceBetween;
            controlPanel.style.marginTop = StyleKeyword.Auto;

            var btnLeft = new Button(() => { MoveNode(index, -1); }) { text = "◀" };
            var btnDelete = new Button(() => { DeleteNode(node); }) { text = "❌" };
            var btnRight = new Button(() => { MoveNode(index, 1); }) { text = "▶" };

            // Ограничения на перемещение, чтобы обычные ноды не перепрыгивали системные края
            btnLeft.SetEnabled(index > 1); 
            btnRight.SetEnabled(index < totalCount - 2); 

            System.Action<Button> shieldControlFields = (b) =>
            {
                b.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
                b.RegisterCallback<PointerUpEvent>(evt => evt.StopPropagation());
                b.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
                b.RegisterCallback<MouseUpEvent>(evt => evt.StopPropagation());
            };

            shieldControlFields(btnLeft);
            shieldControlFields(btnDelete);
            shieldControlFields(btnRight);

            controlPanel.Add(btnLeft);
            controlPanel.Add(btnDelete);
            controlPanel.Add(btnRight);
            card.Add(controlPanel);

            // Если карточка — системный Вход или Выход, полностью скрываем панель управления
            if (isStart || isEnd)
            {
                controlPanel.style.display = DisplayStyle.None;
            }

            return card;
        }

        /// <summary>
        /// Сдвиг ноды по списку влево или вправо кнопками-стрелками
        /// </summary>
        private void MoveNode(int currentIndex, int direction)
        {
            int targetIndex = currentIndex + direction;
            
            // Жесткая проверка границ: Вход (0) и Выход (последний элемент) двигать нельзя!
            if (currentIndex <= 0 || currentIndex >= _nodes.Count - 1) return;
            if (targetIndex <= 0 || targetIndex >= _nodes.Count - 1) return;

            var temp = _plotData.GlobalNodes[currentIndex];
            _plotData.GlobalNodes[currentIndex] = _plotData.GlobalNodes[targetIndex];
            _plotData.GlobalNodes[targetIndex] = temp;

            // ПРИНУДИТЕЛЬНЫЙ РЕБИЛД: Порядок карточек изменился, переписываем связи GlobalEdges
            RebuildGlobalEdges();

            if (Window != null && Window.CurrentAsset != null)
            {
                EditorUtility.SetDirty(Window.CurrentAsset);
            }

            PopulateView(_plotData);
        }

        /// <summary>
        /// Удаление промежуточной ноды из сценария
        /// </summary>
        private void DeleteNode(RuntimeGlobalNode node)
        {
            if (node is GlobalEntryNode || node is GlobalExitNode) return; // Страховка

            if (_plotData != null && _plotData.GlobalNodes != null && _plotData.GlobalNodes.Contains(node))
            {
                _plotData.GlobalNodes.Remove(node);
                
                // ПРИНУДИТЕЛЬНЫЙ РЕБИЛД: Нода пропала из цепи, пересобираем связи без неё
                RebuildGlobalEdges();

                if (Window != null && Window.CurrentAsset != null)
                {
                    EditorUtility.SetDirty(Window.CurrentAsset);
                }
                
                PopulateView(_plotData);
            }
        }

        /// <summary>
        /// Генерирует и перезаписывает список GlobalEdges на основе текущего положения карточек в листе
        /// </summary>
        private void RebuildGlobalEdges()
        {
            if (_plotData == null || _plotData.GlobalNodes == null) return;

            if (_plotData.GlobalEdges == null)
            {
                _plotData.GlobalEdges = new List<EdgeSaveData>();
            }
            
            _plotData.GlobalEdges.Clear();

            // Бежим по цепочке нод и последовательно связываем i-ю ноду с (i+1)-й
            for (int i = 0; i < _plotData.GlobalNodes.Count - 1; i++)
            {
                var currentNode = _plotData.GlobalNodes[i];
                var nextNode = _plotData.GlobalNodes[i + 1];

                // Если у нод по какой-то причине нет ID, пропускаем (чтобы не упасть)
                if (string.IsNullOrEmpty(currentNode.ID) || string.IsNullOrEmpty(nextNode.ID)) continue;

                var globalEdge = new EdgeSaveData
                {
                    OutputNodeID = currentNode.ID,
                    OutputPortIndex = 0, // Условно считаем, что у глобальной карточки один сквозной выход
                    InputNodeID = nextNode.ID
                };

                _plotData.GlobalEdges.Add(globalEdge);
            }
        }
    }
}