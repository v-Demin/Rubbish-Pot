using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Water : MonoBehaviour, IEventBusKeeper
{
    public const float MIN_TEMPERATURE = 5f;
    public const float MAX_TEMPERATURE = 100f;
    private const float BASIC_WATER_COLOR_VOLUME = 2f;

    [SerializeField] private SpriteRenderer _waterSprite;
    [SerializeField] private List<WaterColorInfo> _infos;
    private Color _basicColor;
    private List<IReactionPart> _reactions = new ();

    public Action<List<IReactionPart>> OnValueProportionChanged;

    public EventBus EventBus { get; } = new ();

    private float _temperature = MIN_TEMPERATURE;
    public float Temperature => _temperature;

    private void Start()
    {
        _basicColor = _waterSprite.color;
        EventBus.RaiseEvent<ITemperatureHandler>(h => h.HandleTemperatureChanged(_temperature));
    }

    public void OnSliderUpdated(float sliderValue)
    {
        _temperature = Mathf.Lerp(MIN_TEMPERATURE, MAX_TEMPERATURE, sliderValue);
        EventBus.RaiseEvent<ITemperatureHandler>(h => h.HandleTemperatureChanged(_temperature));
    }

    public void NotifyDipIntoWater(IReactionPart reactionPart)
    {
        if (_reactions.Contains(reactionPart)) return;
        _reactions.Add(reactionPart);
        
        reactionPart.OnVolumeChanged += VolumeChanged;
        VolumeChanged(reactionPart.Volume);
        
        UpdateColor();
    }

    private void VolumeChanged(float volume)
    {
        OnValueProportionChanged?.Invoke(_reactions);
    }

    private void UpdateColor()
    {
        if (_reactions == null || _reactions.Count == 0) return;

        List<ColorVolume> colorVolumes = new ();
        
        foreach (var reaction in _reactions)
        {
            var info = _infos.FirstOrDefault(i => i.Config.Equals(reaction.Config));
            if (info == null) continue;
            
            colorVolumes.Add(new ColorVolume(info.Color, reaction.Volume));
        }
        
        if(colorVolumes.Count == 0) return;
        
        var totalVolume = BASIC_WATER_COLOR_VOLUME + colorVolumes.Sum(cv => cv.Volume);
        var groups = colorVolumes.GroupBy(cv => cv.Color);
        var mixedColor = _basicColor * (BASIC_WATER_COLOR_VOLUME / totalVolume);
        
        foreach (var group in groups)
        {
            var groupVolume = group.Sum(cv => cv.Volume);
            var weight = groupVolume / totalVolume;
            mixedColor += group.Key * weight;
        }
        
        _waterSprite.color = mixedColor;
    }

    [System.Serializable]
    private class WaterColorInfo
    {
        public ReactionConfig Config;
        public Color Color;
    }
    
    [System.Serializable]
    public class ColorVolume
    {
        public Color Color;
        public float Volume;
        
        public ColorVolume(Color color, float volume)
        {
            Color = color;
            Volume = volume;
        }
    }
}
