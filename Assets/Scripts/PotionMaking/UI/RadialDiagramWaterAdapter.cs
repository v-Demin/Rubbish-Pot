using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RadialDiagramWaterAdapter : MonoBehaviour
{
    [SerializeField] private Water _water;
    [SerializeField] private RadialDiagram _radialDiagram;

    [SerializeField] private List<DiagramColorsInfo> _colorsInfos;
    
    private Dictionary<Color, float> _proportions = new ();

    private void Start()
    {
        _water.OnValueProportionChanged += ValueProportionsChanged;
    }

    private void ValueProportionsChanged(List<IReactionPart> reactions)
    {
        var totalVolume = reactions.Sum(r => r.Volume);
        var groups = reactions.GroupBy(r => r.Config);
        
        _proportions.Clear();
        
        foreach (var reactionParts in groups)
        {
            var currentValue = _proportions.Sum(p => p.Value); 
            _proportions[GetColor(reactionParts.Key)] = currentValue + (reactionParts.Sum(r => r.Volume) / totalVolume);
        }
        
        foreach (var pair in _proportions)
        {
            if (!_radialDiagram.IsContains(pair.Key))
            {
                _radialDiagram.AddSlice(pair.Key, pair.Key, () => GetValue(pair.Key));
            }
        }
        
        Refresh();
    }

    private void Refresh()
    {
        _radialDiagram.RefreshDiagram();
    }

    private Color GetColor(ReactionConfig config)
    {
        var info = _colorsInfos.FirstOrDefault(i => i.Config == config);

        return info?.Color ?? Color.gray;
    }

    private float GetValue(Color color)
    {
        return _proportions[color];
    }
    
    [System.Serializable]
    private class DiagramColorsInfo
    {
        public ReactionConfig Config;
        public Color Color;
    }
}
