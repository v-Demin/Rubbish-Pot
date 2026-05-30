using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RubbishPot.Core
{
    [CustomPropertyDrawer(typeof(CharacterDropdownAttribute))]
    public class CharacterDropdownDrawer : PropertyDrawer
    {
        // Указываем, сколько места по высоте займет наш отрисовщик
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var charIdProp = property.FindPropertyRelative("_characterID");
            if (charIdProp == null) return EditorGUIUtility.singleLineHeight * 2; // Высота для ошибки
            
            var window = Resources.FindObjectsOfTypeAll<StateMachineEditorWindow>().FirstOrDefault();
            if (window == null || window.GetPreloadedCharacters().Count == 0)
            {
                return EditorGUIUtility.singleLineHeight * 3.5f; // Высота для фолбэка (предупреждение + 2 поля)
            }

            return EditorGUIUtility.singleLineHeight; // Высота для выпадающего списка
        }

        // Отрисовка через старый добрый IMGUI (Unity автоматически обернет его в UI Toolkit окно)
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var charIdProp = property.FindPropertyRelative("_characterID");
            var variantProp = property.FindPropertyRelative("_selectedVariant");

            // Проверка на ошибку типа
            if (charIdProp == null || variantProp == null)
            {
                EditorGUI.HelpBox(position, $"Ошибка: [CharacterDropdown] применим только к типу LoadedCharacterData", MessageType.Error);
                EditorGUI.EndProperty();
                return;
            }

            var window = Resources.FindObjectsOfTypeAll<StateMachineEditorWindow>().FirstOrDefault();
            List<string> choices = window != null ? window.GetPreloadedCharacters() : new List<string>();

            if (choices.Count > 0)
            {
                // Формируем текущее значение
                string currentFormId = $"{charIdProp.stringValue}_{variantProp.stringValue}";
                int currentIndex = choices.IndexOf(currentFormId);
                
                // Если значения нет в списке (например, команда только создана) — берем первое доступное
                if (currentIndex == -1) 
                {
                    currentIndex = 0;
                    ApplyValuesFromFormId(choices[currentIndex], charIdProp, variantProp, window);
                }

                // Рисуем выпадающий список
                Rect popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                int newIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, choices.ToArray());

                // Если пользователь выбрал другой пункт — обновляем данные
                if (newIndex != currentIndex)
                {
                    ApplyValuesFromFormId(choices[newIndex], charIdProp, variantProp, window);
                }
            }
            else
            {
                // Фолбэк: если персонажей на входе нет
                Rect warningRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(warningRect, label.text, "⚠️ На ноде Входа нет персонажей!");
                
                Rect idRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(idRect, charIdProp, new GUIContent("Character ID"));
                
                Rect varRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(varRect, variantProp, new GUIContent("Variant"));
            }

            EditorGUI.EndProperty();
        }

        private void ApplyValuesFromFormId(string formId, SerializedProperty charIdProp, SerializedProperty variantProp, StateMachineEditorWindow window)
        {
            if (window == null || window.CurrentAsset == null) return;

            var masterEntryNode = window.CurrentAsset.Data?.GlobalNodes?.FirstOrDefault(n => n is GlobalEntryNode) as GlobalEntryNode;
            if (masterEntryNode?.PreloadedCharacters == null) return;

            // Ищем объект-донор на ноде Входа
            var matched = masterEntryNode.PreloadedCharacters.FirstOrDefault(c => c.FormId() == formId);
            if (matched != null)
            {
                var type = typeof(LoadedCharacterData);
                var charIdField = type.GetField("_characterID", BindingFlags.NonPublic | BindingFlags.Instance);
                var variantField = type.GetField("_selectedVariant", BindingFlags.NonPublic | BindingFlags.Instance);

                if (charIdField != null && variantField != null)
                {
                    // Обновляем SerializeProperty, чтобы Unity увидела изменения
                    charIdProp.stringValue = (string)charIdField.GetValue(matched);
                    variantProp.stringValue = (string)variantField.GetValue(matched);
                    
                    // Применяем изменения к сериализованному объекту
                    charIdProp.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
