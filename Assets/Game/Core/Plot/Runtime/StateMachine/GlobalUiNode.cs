using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace RubbishPot.Core
{
    public class GlobalUiNode : Node
    {
        public RuntimeGlobalNode Target { get; private set; }
        private StateMachineEditorWindow _window;

        public GlobalUiNode(RuntimeGlobalNode target, StateMachineEditorWindow window)
        {
            Target = target;
            _window = window;
            
            // Использованием GetType().Name вместо target.Type
            title = $"{target.Name} ({target.GetType().Name})";

            // Стандартные порты Входа и Выхода для глобального движения
            inputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float)));
            outputContainer.Add(InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)));

            BuildSubStatesUI();
        }

        private void BuildSubStatesUI()
        {
            var subStateContainer = new VisualElement();
            subStateContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            subStateContainer.style.paddingBottom = 5;
            subStateContainer.style.paddingTop = 5;

            foreach (var subState in Target.SubStates)
            {
                var btn = new Button(() => 
                {
                    // ИСПРАВЛЕНО: передаем только subState, как требует StateMachineEditorWindow
                    _window.SetActiveSubState(subState);
                }) 
                { 
                    // ИСПРАВЛЕНО: берем человекочитаемое имя сабстейта из его поля Name
                    text = $"-> {subState.Name}" 
                };
                
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                subStateContainer.Add(btn);
            }

            extensionContainer.Add(subStateContainer);
            RefreshExpandedState();
        }
    }
}