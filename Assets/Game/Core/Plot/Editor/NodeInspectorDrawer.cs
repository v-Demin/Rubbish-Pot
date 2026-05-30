using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RubbishPot.Core
{
    public class NodeInspectorDrawer
    {
        private StoryConfig _config; // Ссылка на наш ScriptableObject конфиг

        /// <summary>
        /// Отрисовка инспектора локальных нод. 
        /// Теперь вместо activeSubState передаем activeGlobalNode, так как ресурсы лежат в нём!
        /// </summary>
        public void DrawNodeInspector(RuntimeNode selectedNode, RuntimeGlobalNode activeGlobalNode)
        {
            if (_config == null)
            {
                // Ищем конфиг в проекте, если потеряли
                string[] guids = AssetDatabase.FindAssets("t:StoryConfig");
                if (guids.Length > 0) _config = AssetDatabase.LoadAssetAtPath<StoryConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            if (activeGlobalNode == null)
            {
                EditorGUILayout.HelpBox("Критическая ошибка: Не выбран родительский Глобальный Этап!", MessageType.Error);
                return;
            }

            // 1. Отрисовка ноды фразы (Фильтруем жестко по родительской ГЛОБАЛЬНОЙ ноде)
            if (selectedNode is RuntimePhraseNode phraseNode)
            {
                EditorGUILayout.LabelField($"Настройка фразы (Доступно из этапа: {activeGlobalNode.Name})", EditorStyles.boldLabel);

                // --- Фильтр персонажей ---
                // Берем только ID тех персонажей, которые добавлены в ГЛОБАЛЬНЫЙ этап
                var allowedCharacters = activeGlobalNode.PreloadedCharacters.Select(c => c.CharacterID).ToArray();
                
                if (allowedCharacters.Length > 0)
                {
                    int currentIndex = Mathf.Max(0, System.Array.IndexOf(allowedCharacters, phraseNode.SpeakerCharacterID));
                    int nextIndex = EditorGUILayout.Popup("Кто говорит:", currentIndex, allowedCharacters);
                    phraseNode.SpeakerCharacterID = allowedCharacters[nextIndex];

                    // Фильтруем варианты: только тот вариант, который для этого перса ЗАДАН на Глобальном этапе
                    var chosenSetup = activeGlobalNode.PreloadedCharacters.FirstOrDefault(c => c.CharacterID == phraseNode.SpeakerCharacterID);
                    if (chosenSetup != null)
                    {
                        phraseNode.SpeakerVariant = chosenSetup.SelectedVariant;
                        EditorGUILayout.LabelField($"Вариант персонажа (задан на Гл. Этапе): {phraseNode.SpeakerVariant}", EditorStyles.miniLabel);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("На Глобальном Этапе не добавлено ни одного персонажа!", MessageType.Warning);
                }

                // --- Фильтр фонов ---
                // Тянем фоны напрямую из ГЛОБАЛЬНОГО этапа
                var allowedBackgrounds = activeGlobalNode.PreloadedBackgrounds.ToArray();
                if (allowedBackgrounds.Length > 0)
                {
                    int currentBgIdx = Mathf.Max(0, System.Array.IndexOf(allowedBackgrounds, phraseNode.ActiveBackgroundVariant));
                    int nextBgIdx = EditorGUILayout.Popup("Сменить фон на:", currentBgIdx, allowedBackgrounds);
                    phraseNode.ActiveBackgroundVariant = allowedBackgrounds[nextBgIdx];
                }
                else
                {
                    EditorGUILayout.HelpBox("На Глобальном Этапе не настроены варианты фонов!", MessageType.Warning);
                }

                // Обычный текст
                phraseNode.Text = EditorGUILayout.TextArea(phraseNode.Text, GUILayout.Height(60));
            }
            // 2. Для остальных базовых нод подграфа (Enter, Exit), где ресурсов больше нет
            else if (selectedNode is RuntimeNode baseNode)
            {
                EditorGUILayout.LabelField($"Системная нода подграфа: {baseNode.GetType().Name}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"ID: {baseNode.ID}", EditorStyles.miniLabel);
            }
        }

        /// <summary>
        /// Отдельный метод для отрисовки инспектора самой ГЛОБАЛЬНОЙ ноды
        /// </summary>
        public void DrawGlobalNodeInspector(RuntimeGlobalNode globalNode)
        {
            if (_config == null) return;

            globalNode.Name = EditorGUILayout.TextField("Название этапа", globalNode.Name);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("ОБЕСПЕЧЕНИЕ ЭТАПА РЕСУРСАМИ", EditorStyles.boldLabel);
            
            // Здесь будет твоя старая IMGUI отрисовка добавления/удаления элементов 
            // в списки globalNode.PreloadedCharacters и globalNode.PreloadedBackgrounds
        }
    }
}