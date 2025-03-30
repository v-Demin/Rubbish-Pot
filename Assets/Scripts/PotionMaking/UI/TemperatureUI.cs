using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureUI : MonoBehaviour, ITemperatureHandler
{
    [SerializeField] private Water _water;
    [SerializeField] private TextMeshProUGUI _temperatureLable;
    [SerializeField] private Slider _baseSlider;
    [SerializeField] private Slider _dependentSlider;

    private void Start()
    {
        _water.EventBus.Subscribe(this);
    }

    public void HandleTemperatureChanged(float newTemperature)
    {
        _temperatureLable.text = string.Format("{0:F0}°", newTemperature);
    }

    public void OnBlueSliderValueChanged()
    {
        DOVirtual.Float(_dependentSlider.value, _baseSlider.value, 0.35f, value => _dependentSlider.value = value);
    }
}
