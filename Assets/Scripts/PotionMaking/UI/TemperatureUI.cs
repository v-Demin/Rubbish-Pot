using TMPro;
using UnityEngine;

public class TemperatureUI : MonoBehaviour, ITemperatureHandler
{
    [SerializeField] private Water _water;
    [SerializeField] private TextMeshProUGUI _temperatureLable;

    private void Start()
    {
        _water.EventBus.Subscribe(this);
    }

    public void HandleTemperatureChanged(float newTemperature)
    {
        _temperatureLable.text = string.Format("{0:F0}°", newTemperature);
    }
}
