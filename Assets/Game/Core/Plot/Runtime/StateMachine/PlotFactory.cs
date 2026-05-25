using System;
using System.Linq;
using UnityEngine;

namespace RubbishPot.Core
{
    public static class PlotFactory
    {
        /// <summary>
        /// Загружает PlotAsset из папки Resources по имени файла и создает интерпретатор.
        /// Путь должен быть относительным внутри папки Resources.
        /// </summary>
        /// <param name="assetPath">Например: "Tutorial/Tutorial_Plot_1" (без .asset)</param>
        public static (GraphRuntimeInterpreter interpreter, GraphController controller, string entryNodeId) Create(string plotName)
        {
            var asset = UnityEngine.Resources.Load<PlotAsset>(plotName);
            if (asset == null) throw new Exception($"[PlotFactory] Не найден ассет Resources/{plotName}");

            var interpreter = new GraphRuntimeInterpreter(asset.Data);
            var controller = new GraphController(interpreter, asset.Data);
            
            var entryNode = asset.Data.Nodes.OfType<RuntimeEntryNode>().FirstOrDefault();
            string entryId = entryNode != null ? entryNode.ID : string.Empty;

            return (interpreter, controller, entryId);
        }
    }
}
