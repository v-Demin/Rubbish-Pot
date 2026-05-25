using System;
using Object = UnityEngine.Object;

namespace RubbishPot.Core
{
    public static class PlotFactory
    {
        public static IPlot Create(string plotName)
        {
            var asset = UnityEngine.Resources.Load<PlotAsset>(plotName);
            if (asset == null) throw new Exception($"[PlotFactory] Не найден ассет Resources/{plotName}");

            var runtimeInstance = Object.Instantiate(asset);
            var plot = new Plot(runtimeInstance.Data, plotName);
            return plot;
        }
    }
}
