using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUIItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private const float VALUE_TO_DRAG = 1f;
    
    [SerializeField] private TextMeshProUGUI _valueLabel;
    [SerializeField] private Transform _previewParent;
    public Transform PreviewParent => _previewParent;

    private Func<int> _numberGetter;
    private Action<InventoryUIItem> _itemChosenCallback;

    private Vector2? _mousePosition;
    
    public void Init(RectTransform preview, Func<int> numberGetter, Action<InventoryUIItem> itemChosenCallback)
    {
        _numberGetter = numberGetter;
        preview.SetParent(_previewParent);
        preview.localPosition = Vector3.zero;
        _itemChosenCallback = itemChosenCallback;
    }

    public void Refresh()
    {
        _valueLabel.text = _numberGetter().ToString();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _mousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(_mousePosition == null) return;
        
        if (Vector2.Distance(_mousePosition.Value, eventData.position) > VALUE_TO_DRAG)
        {
            _itemChosenCallback?.Invoke(this);
            _mousePosition = null;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _mousePosition = null;
    }
}
