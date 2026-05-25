using UnityEngine;

namespace RubbishPot.Core
{
    [CreateAssetMenu(fileName = "NewPlot", menuName = "RubbishPot/Plot Asset")]
    public class PlotAsset : ScriptableObject
    {
        public PlotData Data = new PlotData();
    }
}
