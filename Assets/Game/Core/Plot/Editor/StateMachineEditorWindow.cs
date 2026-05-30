using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public PlotAsset CurrentAsset => _currentAsset;
        private Label _assetLabel;

        private RuntimeGlobalNode _activeGlobalNode;

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
                    width = 290, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10,
                    borderLeftWidth = 1, borderLeftColor = new Color(0.15f, 0.15f, 0.15f)
                } 
            };
            
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
            _activeGlobalNode = null;
            _assetLabel.text = asset != null ? $"Активный сценарий: {asset.name}" : "Ассет не выбран";

            if (_currentAsset != null && _currentAsset.Data != null)
            {
                _topGraphView.PopulateView(_currentAsset.Data);
            }
            
            _bottomGraphView.PopulateView(null);
            UpdateInspector(null);
        }

        public void SetActiveSubState(RuntimeGlobalNode parentGlobalNode, RuntimeSubState subState)
        {
            _activeGlobalNode = parentGlobalNode;
            _bottomGraphView.PopulateView(subState);
        }

        /// <summary>
        /// Возвращает список отформатированных строк FormId() всех preloaded персонажей для PropertyDrawer
        /// </summary>
        public List<string> GetPreloadedCharacters()
        {
            var masterEntryNode = _currentAsset?.Data?.GlobalNodes?.FirstOrDefault(n => n is GlobalEntryNode) as GlobalEntryNode;
            if (masterEntryNode?.PreloadedCharacters == null) return new List<string>();
            return masterEntryNode.PreloadedCharacters.Select(c => c.FormId()).ToList();
        }

        public void UpdateInspector(object targetRuntimeObject)
        {
            _rightInspectorPanel.Clear();
            _rightInspectorPanel.Add(new Label("ИНСПЕКТОР СВОЙСТВ") { 
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, fontSize = 12, opacity = 0.7f } 
            });

            if (targetRuntimeObject == null)
            {
                _rightInspectorPanel.Add(new Label("Ничего не выбрано") { style = { opacity = 0.4f, unityTextAlign = TextAnchor.MiddleCenter, marginTop = 20 } });
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:StoryConfig");
            StoryConfig config = null;
            if (guids.Length > 0) config = AssetDatabase.LoadAssetAtPath<StoryConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));

            if (config == null)
            {
                _rightInspectorPanel.Add(new Label("⚠️ Ошибка: Создайте файл StoryConfig в проекте!") { style = { color = Color.yellow, whiteSpace = WhiteSpace.Normal, marginTop = 10 } });
                return;
            }

            // Кэшируем филды рефлексии для LoadedCharacterData
            var charIdField = typeof(LoadedCharacterData).GetField("_characterID", BindingFlags.NonPublic | BindingFlags.Instance);
            var variantField = typeof(LoadedCharacterData).GetField("_selectedVariant", BindingFlags.NonPublic | BindingFlags.Instance);

            // --- КЕЙС 1: ВЫБРАНА ГЛОБАЛЬНАЯ НОДА ---
            if (targetRuntimeObject is RuntimeGlobalNode globalNode)
            {
                _rightInspectorPanel.Add(new Label($"ГЛОБАЛЬНЫЙ ЭТАП: {globalNode.GetType().Name}") { style = { fontSize = 10, opacity = 0.5f, marginBottom = 5 } });
                
                var nameField = new TextField("Название этапа") { value = globalNode.Name };
                nameField.RegisterValueChangedCallback(evt => {
                    globalNode.Name = evt.newValue;
                    if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                    _topGraphView.Refresh(_currentAsset.Data.GlobalNodes);
                });
                _rightInspectorPanel.Add(nameField);

                if (globalNode is GlobalEntryNode entryNode)
                {
                    _rightInspectorPanel.Add(new VisualElement { style = { height = 1, backgroundColor = new Color(0.3f, 0.3f, 0.3f), marginTop = 10, marginBottom = 10 } });
                    _rightInspectorPanel.Add(new Label("Доступные персонажи сценария:") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 3 } });
                    
                    if (entryNode.PreloadedCharacters == null) entryNode.PreloadedCharacters = new List<LoadedCharacterData>();
                    
                    foreach (var loadedChar in entryNode.PreloadedCharacters.ToList())
                    {
                        var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginTop = 3 } };
                        
                        string currentCharId = (string)charIdField.GetValue(loadedChar) ?? "";
                        string currentVariant = (string)variantField.GetValue(loadedChar) ?? "Base";

                        var charChoices = config.Characters.Select(c => c.CharacterID).ToList();
                        var charDropdown = new DropdownField(charChoices, charChoices.Contains(currentCharId) ? currentCharId : (charChoices.FirstOrDefault() ?? ""));
                        charDropdown.style.flexGrow = 1;
                        charDropdown.RegisterValueChangedCallback(evt => {
                            charIdField.SetValue(loadedChar, evt.newValue);
                            var matchedChar = config.Characters.FirstOrDefault(c => c.CharacterID == evt.newValue);
                            variantField.SetValue(loadedChar, matchedChar?.Variants?.FirstOrDefault() ?? "Base");
                            if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                            UpdateInspector(entryNode); 
                        });
                        row.Add(charDropdown);

                        var currentConfigChar = config.Characters.FirstOrDefault(c => c.CharacterID == currentCharId);
                        var variantChoices = currentConfigChar?.Variants ?? new List<string> { "Base" };
                        var varDropdown = new DropdownField(variantChoices, variantChoices.Contains(currentVariant) ? currentVariant : "Base");
                        varDropdown.style.width = 90;
                        varDropdown.style.marginLeft = 4;
                        varDropdown.RegisterValueChangedCallback(evt => {
                            variantField.SetValue(loadedChar, evt.newValue);
                            if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                        });
                        row.Add(varDropdown);

                        var delBtn = new Button(() => {
                            entryNode.PreloadedCharacters.Remove(loadedChar);
                            if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                            UpdateInspector(entryNode);
                        }) { text = "❌" };
                        row.Add(delBtn);

                        _rightInspectorPanel.Add(row);
                    }

                    var addCharBtn = new Button(() => {
                        if (config.Characters.Count > 0) {
                            var first = config.Characters[0];
                            var newData = new LoadedCharacterData();
                            charIdField.SetValue(newData, first.CharacterID);
                            variantField.SetValue(newData, first.Variants.FirstOrDefault() ?? "Base");

                            entryNode.PreloadedCharacters.Add(newData);
                            if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                            UpdateInspector(entryNode);
                        }
                    }) { text = "➕ Добавить Персонажа", style = { marginTop = 5 } };
                    _rightInspectorPanel.Add(addCharBtn);

                    // Управление фонами
                    _rightInspectorPanel.Add(new Label("Доступные фоны сценария:") { style = { marginTop = 15, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 3 } });
                    if (entryNode.PreloadedBackgrounds == null) entryNode.PreloadedBackgrounds = new List<string>();

                    foreach (var bgVar in entryNode.PreloadedBackgrounds.ToList())
                    {
                        var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginTop = 3 } };
                        var bgDropdown = new DropdownField(config.BackgroundVariants, bgVar);
                        bgDropdown.style.flexGrow = 1;
                        bgDropdown.RegisterValueChangedCallback(evt => {
                            int idx = entryNode.PreloadedBackgrounds.IndexOf(bgVar);
                            if (idx != -1) entryNode.PreloadedBackgrounds[idx] = evt.newValue;
                            if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                        });
                        row.Add(bgDropdown);

                        var delBgBtn = new Button(() => {
                            entryNode.PreloadedBackgrounds.Remove(bgVar);
                            if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                            UpdateInspector(entryNode);
                        }) { text = "❌" };
                        row.Add(delBgBtn);

                        _rightInspectorPanel.Add(row);
                    }

                    var addBgBtn = new Button(() => {
                        if (config.BackgroundVariants.Count > 0) {
                            entryNode.PreloadedBackgrounds.Add(config.BackgroundVariants[0]);
                            if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                            UpdateInspector(entryNode);
                        }
                    }) { text = "➕ Добавить Вариант Фона", style = { marginTop = 5 } };
                    _rightInspectorPanel.Add(addBgBtn);
                }
                return;
            }

            // --- КЕЙС 2: ВЫБРАНА НОДА РЕПЛИКИ (RuntimePhraseNode) ---
            if (targetRuntimeObject is RuntimePhraseNode phraseNode)
            {
                _rightInspectorPanel.Add(new Label("ЛОКАЛЬНАЯ РЕПЛИКА") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 5, marginBottom = 5 } });

                var masterEntryNode = _currentAsset?.Data?.GlobalNodes?.FirstOrDefault(n => n is GlobalEntryNode) as GlobalEntryNode;
                if (masterEntryNode == null)
                {
                    _rightInspectorPanel.Add(new Label("❌ Ошибка: В сценарии не найдена глобальная нода Входа!") { style = { color = Color.red } });
                    return;
                }

                // Извлекаем чистые ID через рефлексию из приватных полей
                var allowedChars = masterEntryNode.PreloadedCharacters
                    .Select(c => (string)charIdField.GetValue(c))
                    .Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

                if (allowedChars.Count > 0)
                {
                    if (string.IsNullOrEmpty(phraseNode.SpeakerCharacterID) || !allowedChars.Contains(phraseNode.SpeakerCharacterID))
                    {
                        phraseNode.SpeakerCharacterID = allowedChars[0];
                    }

                    var charDropdown = new DropdownField("Кто говорит", allowedChars, phraseNode.SpeakerCharacterID);
                    charDropdown.RegisterValueChangedCallback(evt => {
                        phraseNode.SpeakerCharacterID = evt.newValue;
                        
                        // Ищем первый подходящий вариант для этого персонажа на Входе
                        var matchedSetup = masterEntryNode.PreloadedCharacters.FirstOrDefault(c => (string)charIdField.GetValue(c) == evt.newValue);
                        phraseNode.SpeakerVariant = matchedSetup != null ? (string)variantField.GetValue(masterEntryNode.PreloadedCharacters.First(c => (string)charIdField.GetValue(c) == evt.newValue)) : "Base";
                        
                        if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                        UpdateInspector(phraseNode); 
                    });
                    _rightInspectorPanel.Add(charDropdown);

                    var currentSetup = masterEntryNode.PreloadedCharacters.FirstOrDefault(c => (string)charIdField.GetValue(c) == phraseNode.SpeakerCharacterID);
                    phraseNode.SpeakerVariant = currentSetup != null ? (string)variantField.GetValue(currentSetup) : "Base";
                    _rightInspectorPanel.Add(new Label($"Вариант (задан на Входе): {phraseNode.SpeakerVariant}") { 
                        style = { opacity = 0.5f, fontSize = 11, marginLeft = 3, marginBottom = 8 } 
                    });
                }
                else
                {
                    _rightInspectorPanel.Add(new Label("⚠️ На ноде Входа не настроено ни одного персонажа!") { style = { color = Color.yellow, fontSize = 11, marginBottom = 8 } });
                }

                // Выбор Фона
                var allowedBgs = masterEntryNode.PreloadedBackgrounds;
                if (allowedBgs != null && allowedBgs.Count > 0)
                {
                    if (string.IsNullOrEmpty(phraseNode.ActiveBackgroundVariant) || !allowedBgs.Contains(phraseNode.ActiveBackgroundVariant))
                    {
                        phraseNode.ActiveBackgroundVariant = allowedBgs[0];
                    }

                    var bgDropdown = new DropdownField("Состояние фона", allowedBgs, phraseNode.ActiveBackgroundVariant);
                    bgDropdown.RegisterValueChangedCallback(evt => {
                        phraseNode.ActiveBackgroundVariant = evt.newValue;
                        if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                    });
                    _rightInspectorPanel.Add(bgDropdown);
                }

                var textField = new TextField("Текст реплики") { value = phraseNode.Text, multiline = true };
                textField.style.height = 70; textField.style.marginTop = 10; textField.style.marginBottom = 10;
                textField.RegisterValueChangedCallback(evt => {
                    phraseNode.Text = evt.newValue;
                    if (_currentAsset != null) EditorUtility.SetDirty(_currentAsset);
                });
                _rightInspectorPanel.Add(textField);

                // Отрисовка базовой оболочки для Phrase ноды (списки ISubCommand и т.д.)
                RenderDefaultFields(phraseNode);
                return;
            }

            // --- КЕЙС 3: ЛЮБЫЕ ДРУГИЕ СИСТЕМНЫЕ НОДЫ ПОДГРАФА ---
            if (targetRuntimeObject is RuntimeNode baseNode)
            {
                _rightInspectorPanel.Add(new Label($"Нода подграфа: {baseNode.GetType().Name}") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                _rightInspectorPanel.Add(new Label($"ID: {baseNode.ID}") { style = { fontSize = 9, opacity = 0.5f, marginTop = 4, marginBottom = 12 } });

                RenderDefaultFields(baseNode);
            }
        }

        /// <summary>
        /// Внутренний метод сквозной автоматической отрисовки полей через PropertyField
        /// </summary>
        private void RenderDefaultFields(RuntimeNode node)
        {
            if (_currentAsset == null) return;

            var serializedObject = new SerializedObject(_currentAsset);
            serializedObject.Update();
            var nodeProperty = FindPropertyForObject(serializedObject, node);

            if (nodeProperty != null)
            {
                var iterator = nodeProperty.Copy();
                var endProperty = iterator.GetEndProperty();

                if (iterator.NextVisible(true))
                {
                    while (!SerializedProperty.EqualContents(iterator, endProperty))
                    {
                        string name = iterator.name.ToLower();
                        // Фильтруем кастомные поля PhraseNode, чтобы они не дублировались снизу
                        bool isCustom = name == "id" || name == "text" || name == "speakercharacterid" || 
                                        name == "speakervariant" || name == "activebackgroundvariant";

                        if (!isCustom)
                        {
                            var propField = new PropertyField(iterator.Copy());
                            propField.Bind(serializedObject);
                            _rightInspectorPanel.Add(propField);
                        }
                        iterator.NextVisible(false);
                    }
                }
            }
        }

        private SerializedProperty FindPropertyForObject(SerializedObject serializedObject, object target)
        {
            if (target == null) return null;
            var prop = serializedObject.GetIterator();
            while (prop.Next(true))
            {
                if (prop.propertyType == SerializedPropertyType.ManagedReference && prop.managedReferenceValue == target) return prop.Copy();
                if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == target) return prop.Copy();
            }
            return null;
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            _assetLabel = new Label("Дважды кликните по PlotAsset для начала работы");
            _assetLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _assetLabel.style.paddingLeft = 5; _assetLabel.style.paddingRight = 15;
            toolbar.Add(_assetLabel);
            toolbar.Add(new Button(() => ShowGlobalNodeMenu()) { text = "➕ Добавить глобальный этап" });
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

            // Перед записью на жесткий диск вытягиваем из UI-нод нижнего окна актуальные экранные позиции
            if (_bottomGraphView != null)
            {
                _bottomGraphView.SyncAllNodePositions();
            }

            EditorUtility.SetDirty(_currentAsset);
            AssetDatabase.SaveAssets();
            Debug.Log("[Editor] Сценарий успешно сохранен, координаты всех нод обновлены!");
        }
    }
}