using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RubbishPot.Core
{
    public class StateMachineEditorWindow : EditorWindow
    {
        private GlobalGraphView _topGraphView;
        private SubStateGraphView _bottomGraphView;
        private VisualElement _rightInspectorPanel;
        
        private PlotAsset _currentAsset;
        // Публичный геттер, чтобы граф мог вызывать EditorUtility.SetDirty
        public PlotAsset CurrentAsset => _currentAsset;
        private Label _assetLabel;

        [MenuItem("Tools/State Machine Editor")]
        public static void Open() => GetWindow<StateMachineEditorWindow>("State Machine");

        private void OnEnable()
        {
            _topGraphView = new GlobalGraphView(this) { style = { flexGrow = 1 } };
            _bottomGraphView = new SubStateGraphView(this) { style = { flexGrow = 1 } };
            
            _rightInspectorPanel = new VisualElement 
            { 
                style = 
                { 
                    width = 250, 
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 10,
                    paddingBottom = 10
                } 
            };
            _rightInspectorPanel.Add(new Label("Инспектор ноды") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var leftVerticalSplitter = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Vertical);
            leftVerticalSplitter.Add(_topGraphView);
            leftVerticalSplitter.Add(_bottomGraphView);

            var mainHorizontalSplitter = new TwoPaneSplitView(0, 500, TwoPaneSplitViewOrientation.Horizontal);
            mainHorizontalSplitter.Add(leftVerticalSplitter);
            mainHorizontalSplitter.Add(_rightInspectorPanel);
            
            mainHorizontalSplitter.style.flexGrow = 1;

            GenerateToolbar();
            rootVisualElement.Add(mainHorizontalSplitter);
        }

        public void LoadAsset(PlotAsset asset)
        {
            _currentAsset = asset;
            _assetLabel.text = asset != null ? $"Активный сценарий: {asset.name}" : "Ассет не выбран";

            if (_currentAsset != null && _currentAsset.Data != null)
            {
                _topGraphView.PopulateView(_currentAsset.Data);
            }
            
            _bottomGraphView.PopulateView(null);
            UpdateInspector(null);
        }

        public void SetActiveSubState(RuntimeSubState subState)
        {
            _bottomGraphView.PopulateView(subState);
        }

        public void UpdateInspector(object targetRuntimeObject)
        {
            while (_rightInspectorPanel.childCount > 1)
            {
                _rightInspectorPanel.RemoveAt(1);
            }

            if (targetRuntimeObject == null)
            {
                _rightInspectorPanel.Add(new Label("Ничего не выбрано"));
                return;
            }

            _rightInspectorPanel.Add(new Label($"Тип данных: {targetRuntimeObject.GetType().Name}"));
            
            if (targetRuntimeObject is RuntimeNode node)
            {
                _rightInspectorPanel.Add(new Label($"ID: {node.ID}"));
            }
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            _assetLabel = new Label("Дважды кликните по PlotAsset для начала работы");
            _assetLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _assetLabel.style.paddingLeft = 5;
            _assetLabel.style.paddingRight = 15;
            toolbar.Add(_assetLabel);
    
            var addNodeButton = new Button(() => ShowGlobalNodeMenu()) { text = "➕ Добавить глобальный этап" };
            toolbar.Add(addNodeButton);
    
            toolbar.Add(new Button(SaveData) { text = "Сохранить Сценарий" });
            rootVisualElement.Add(toolbar);
        }
        
        private void ShowGlobalNodeMenu()
        {
            if (_currentAsset == null) return;

            var provider = ScriptableObject.CreateInstance<GlobalSearchWindowProvider>();
            provider.Init(_topGraphView);

            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            SearchWindow.Open(new SearchWindowContext(mousePos), provider);
        }

        private void SaveData()
        {
            if (_currentAsset == null) return;
            
            EditorUtility.SetDirty(_currentAsset);
            AssetDatabase.SaveAssets();
            Debug.Log("[Editor] Сценарий успешно сохранен!");
        }
    }
}