using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Spoon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float _followSpeed = 5f; // Скорость следования ложки

    private bool _isDragging = false;
    private Vector3 _targetPosition;
    private Rigidbody2D _rigidbody;

    private Vector3 _startPosition;
    private Tween _backTween;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!_isDragging) return;
        
        var smoothedPosition = Vector3.Lerp(transform.position, _targetPosition, _followSpeed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(smoothedPosition);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        _backTween.Kill();
        UpdateTargetPosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        _backTween = _rigidbody.DOMove(_startPosition, 0.55f)
            .SetEase(Ease.OutQuart);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateTargetPosition(eventData);
    }

    private void UpdateTargetPosition(PointerEventData eventData)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPosition.z = 0;
        _targetPosition = worldPosition;
    }
}
