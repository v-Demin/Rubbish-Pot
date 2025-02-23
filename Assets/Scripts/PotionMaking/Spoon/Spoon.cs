using UnityEngine;
using UnityEngine.EventSystems;

public class Spoon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float followSpeed = 10f; // Скорость следования ложки
    public float pushForce = 5f; // Сила толчка объектов

    private bool isDragging = false;
    private Vector3 targetPosition;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
            rb.MovePosition(smoothedPosition);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        UpdateTargetPosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateTargetPosition(eventData);
    }

    private void UpdateTargetPosition(PointerEventData eventData)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPosition.z = 0;
        targetPosition = worldPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PotItem>(out var component))
        {
            Rigidbody2D ingredientRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (ingredientRb != null)
            {
                Vector2 forceDirection = (collision.transform.position - transform.position).normalized;
                ingredientRb.AddForce(forceDirection * pushForce, ForceMode2D.Impulse);
            }
        }
    }
}
