#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    private static readonly Dictionary<Type, Type[]> typesCache = new Dictionary<Type, Type[]>();
    private static readonly Dictionary<Type, string[]> typeNamesCache = new Dictionary<Type, string[]>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
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

        // Определяем базовый тип. Если поле – список, берём тип элементов
        Type baseType = fieldInfo.FieldType;
        if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(List<>))
        {
            baseType = baseType.GetGenericArguments()[0];
        }

        // Рисуем выпадающий список с учетом фильтрации
        DrawTypePopup(new Rect(dropdownRect.x + 80, dropdownRect.y, dropdownRect.width - 80, dropdownRect.height), property, baseType);

        // Кнопка для очистки поля
        if (GUI.Button(clearRect, "Clear"))
        {
            property.managedReferenceValue = null;
        }

        // Если значение не null и раскрыто — отрисовываем инспектор выбранного объекта
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
        // Получаем экземпляр нашего атрибута
        SubclassSelectorAttribute subclassAttr = attribute as SubclassSelectorAttribute;

        // Получаем список типов-наследников для базового типа (с кэшированием)
        if (!typesCache.TryGetValue(baseType, out Type[] types))
        {
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return new Type[0]; }
                })
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
                .ToArray();
            typesCache[baseType] = types;
            typeNamesCache[baseType] = types.Select(t => t.Name).ToArray();
        }

        // Если задан метод-фильтр, получаем родительский объект (например, StepRoot)
        if (!string.IsNullOrEmpty(subclassAttr.FilterMethodName))
        {
            object parent = GetParentObject(property);
            if (parent != null)
            {
                MethodInfo methodInfo = parent.GetType().GetMethod(subclassAttr.FilterMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (methodInfo != null)
                {
                    types = types.Where(t =>
                    {
                        object instance = Activator.CreateInstance(t);
                        object result = methodInfo.Invoke(parent, new object[] { instance });
                        return result is bool b && b;
                    }).ToArray();
                    typeNamesCache[baseType] = types.Select(t => t.Name).ToArray();
                }
            }
        }

        // Добавляем опцию "None" для сброса значения
        string[] options = new string[typeNamesCache[baseType].Length + 1];
        options[0] = "None";
        for (int i = 0; i < typeNamesCache[baseType].Length; i++)
        {
            options[i + 1] = typeNamesCache[baseType][i];
        }

        int currentIndex = 0;
        if (property.managedReferenceValue != null)
        {
            Type currentType = property.managedReferenceValue.GetType();
            int foundIndex = Array.IndexOf(types, currentType);
            if (foundIndex >= 0)
                currentIndex = foundIndex + 1; // +1, т.к. 0 соответствует "None"
        }

        int newIndex = EditorGUI.Popup(rect, currentIndex, options);
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

    /// <summary>
    /// Получает объект, в котором находится поле, используя propertyPath.
    /// </summary>
    private object GetParentObject(SerializedProperty property)
    {
        string path = property.propertyPath.Replace(".Array.data[", "[");
        object obj = property.serializedObject.targetObject;
        string[] elements = path.Split('.');
        // Последний элемент — само поле, его пропускаем
        for (int i = 0; i < elements.Length - 1; i++)
        {
            string element = elements[i];
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }
        return obj;
    }

    private object GetValue(object source, string name)
    {
        if (source == null)
            return null;
        Type type = source.GetType();
        FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field != null ? field.GetValue(source) : null;
    }

    private object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
        if (enumerable == null)
            return null;
        var enm = enumerable.GetEnumerator();
        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext())
                return null;
        }
        return enm.Current;
    }
}
#endif
