using System;
using UnityEngine;

public class ThrowingMechanic : MonoBehaviour
{
    [SerializeField] private Vector2 _horizontalBounds = new Vector2(3f, 3f);
    [SerializeField] private float _throwingHeight = 3f;

    private Func<ThrowingObject> _objectGetter;
    private Func<bool> _isDraggingAvailable;
    private Action _onObjectDropped;

    private ThrowingObject _currentThrowingObject;
    
    public void Init(Func<ThrowingObject> objectGetter, Func<bool> isDraggingAvailable, Action onObjectDropped)
    {
        _objectGetter = objectGetter;
        _isDraggingAvailable = isDraggingAvailable;
        _onObjectDropped = onObjectDropped;
    }

    private void Update()
    {
        if (_isDraggingAvailable == null || !_isDraggingAvailable()) return;
        if (_objectGetter == null) return;
        
        if (_currentThrowingObject == null)
        {
            CreateNewThrowingObject();
            _currentThrowingObject.Grab();
        }

        if (Input.GetMouseButton(0))
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var positionX = Mathf.Clamp(mousePosition.x, _horizontalBounds.x, _horizontalBounds.y);
            _currentThrowingObject.transform.position = new Vector3(positionX, _throwingHeight, -20f);
        }
        else
        {
            _currentThrowingObject.Drop();
            _currentThrowingObject = null;
            _onObjectDropped?.Invoke();
        }
    }

    private void CreateNewThrowingObject()
    {
        _currentThrowingObject = _objectGetter();
    }
}
