#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    private static readonly Dictionary<Type, Type[]> typesCache = new Dictionary<Type, Type[]>();
    private static readonly Dictionary<Type, string[]> typeNamesCache = new Dictionary<Type, string[]>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Базовая высота – одна строка для выпадающего списка
        float height = EditorGUIUtility.singleLineHeight;

        // Если значение не null и раскрыто, добавляем высоту для остальных полей
        if (property.managedReferenceValue != null && property.isExpanded)
        {
            height += EditorGUI.GetPropertyHeight(property, label, true);
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float clearButtonWidth = 50f;
        Rect mainRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect dropdownRect = new Rect(mainRect.x, mainRect.y, mainRect.width - clearButtonWidth, mainRect.height);
        Rect clearRect = new Rect(mainRect.x + mainRect.width - clearButtonWidth, mainRect.y, clearButtonWidth, mainRect.height);

        // Foldout для сворачивания/разворачивания инспектора выбранного объекта
        property.isExpanded = EditorGUI.Foldout(new Rect(dropdownRect.x, dropdownRect.y, 15, dropdownRect.height),
            property.isExpanded, GUIContent.none);

        // Отрисовка лейбла
        EditorGUI.LabelField(new Rect(dropdownRect.x + 15, dropdownRect.y, dropdownRect.width - 15, dropdownRect.height), label);

        // Определяем базовый тип. Если поле — список, то берём тип элементов
        Type baseType = fieldInfo.FieldType;
        if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(List<>))
        {
            baseType = baseType.GetGenericArguments()[0];
        }

        // Рисуем выпадающий список типов с возможностью сброса через "None"
        DrawTypePopup(new Rect(dropdownRect.x + 80, dropdownRect.y, dropdownRect.width - 80, dropdownRect.height), property, baseType);

        // Кнопка для очистки поля
        if (GUI.Button(clearRect, "Clear"))
        {
            property.managedReferenceValue = null;
        }

        // Если объект выбран и раскрыт – отрисовываем инспектор для его полей
        if (property.managedReferenceValue != null && property.isExpanded)
        {
            Rect childRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight,
                position.width, position.height - EditorGUIUtility.singleLineHeight);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(childRect, property, GUIContent.none, true);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawTypePopup(Rect rect, SerializedProperty property, Type baseType)
    {
        // Получаем список типов-наследников для данного базового типа (с кэшированием)
        if (!typesCache.TryGetValue(baseType, out Type[] types))
        {
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); } catch { return new Type[0]; }
                })
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
                .ToArray();
            typesCache[baseType] = types;
            typeNamesCache[baseType] = types.Select(t => t.Name).ToArray();
        }

        // Добавляем опцию "None" для возможности сброса значения
        string[] options = new string[typeNamesCache[baseType].Length + 1];
        options[0] = "None";
        for (int i = 0; i < typeNamesCache[baseType].Length; i++)
        {
            options[i + 1] = typeNamesCache[baseType][i];
        }

        // Определяем выбранный индекс: если значение null – выбираем "None"
        int currentIndex = 0;
        if (property.managedReferenceValue != null)
        {
            Type currentType = property.managedReferenceValue.GetType();
            int foundIndex = Array.IndexOf(types, currentType);
            if (foundIndex >= 0)
            {
                currentIndex = foundIndex + 1; // +1, т.к. 0 - это "None"
            }
        }

        int newIndex = EditorGUI.Popup(rect, currentIndex, options);

        // Если выбор изменился, устанавливаем новое значение через Activator
        if (newIndex != currentIndex)
        {
            if (newIndex == 0)
            {
                property.managedReferenceValue = null;
            }
            else
            {
                property.managedReferenceValue = Activator.CreateInstance(types[newIndex - 1]);
            }
        }
    }
}
#endif
