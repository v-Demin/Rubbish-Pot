using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RubbishPot.Core
{
    public static class PlotAssetOpener
    {
        [OnOpenAsset(1)] // Приоритет 1 перехватывает клик до стандартных систем Unity
        public static bool OnOpenGraphAsset(int instanceID, int line)
        {
            Object asset = EditorUtility.InstanceIDToObject(instanceID);

            // Если кликнули по PlotAsset — открываем ТВОЕ окно
            if (asset is PlotAsset plotAsset)
            {
                StateMachineEditorWindow window = EditorWindow.GetWindow<StateMachineEditorWindow>("State Machine");
                window.LoadAsset(plotAsset);
                return true; 
            }

            return false;
        }
    }
}
