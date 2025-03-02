using UnityEngine;

public class ThrowingMechanic : MonoBehaviour
{
    [Header("Настройки объекта")]
    public GameObject objectPrefab;  // Префаб объекта, который будет бросаться
    public float throwHeight = 5f;     // Фиксированная высота во время перемещения
    public Vector2 horizontalBounds = new Vector2(-5f, 5f); // Ограничения по оси X

    [Header("Настройки Gizmos")]
    public Color boundaryColor = Color.green; // Цвет границ движения
    public Color trajectoryColor = Color.red; // Цвет линии траектории

    private GameObject currentObject;   // Текущий создаваемый объект
    private Vector3 initialPosition;    // Начальная позиция (точка броска)
    private Vector3 lastPosition;       // Последняя позиция объекта во время перетаскивания
    private bool isDragging = false;    // Флаг перетаскивания объекта

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Получаем позицию мыши в мировых координатах.
            Vector3 mousePos = Input.mousePosition;
            // Задаём z для корректного преобразования (расстояние до камеры)
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // Устанавливаем позицию броска с фиксированной высотой
            initialPosition = new Vector3(worldPos.x, throwHeight, worldPos.z);
            currentObject = Instantiate(objectPrefab, initialPosition, Quaternion.identity);
            lastPosition = initialPosition;
            isDragging = true;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            // Обновляем позицию объекта согласно движению мыши
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // Ограничиваем движение по оси X заданными границами
            float newX = Mathf.Clamp(worldPos.x, horizontalBounds.x, horizontalBounds.y);
            Vector3 newPos = new Vector3(newX, throwHeight, initialPosition.z);
            
            if (currentObject != null)
            {
                currentObject.transform.position = newPos;
                lastPosition = newPos;
            }
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            // При отпускании мыши включаем физику для объекта
            if (currentObject != null)
            {
                Rigidbody rb = currentObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = true;
                    // При необходимости можно добавить силу для броска:
                    // rb.AddForce(Vector3.down * сила, ForceMode.Impulse);
                }
            }
            isDragging = false;
        }
    }

    void OnDrawGizmos()
    {
        // Отрисовка ограничивающей линии (границ по оси X) на заданной высоте
        Gizmos.color = boundaryColor;
        Vector3 leftPoint = new Vector3(horizontalBounds.x, throwHeight, 0);
        Vector3 rightPoint = new Vector3(horizontalBounds.y, throwHeight, 0);
        Gizmos.DrawLine(leftPoint, rightPoint);

        // Отрисовка линии траектории движения объекта
        if (isDragging)
        {
            Gizmos.color = trajectoryColor;
            Gizmos.DrawLine(initialPosition, lastPosition);
        }
    }
}
