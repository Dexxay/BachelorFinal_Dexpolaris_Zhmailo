using UnityEngine;

[RequireComponent(typeof(Camera))] // Вимагає наявності компонента Camera на тому ж об'єкті
public class EditorCameraMovement : MonoBehaviour
{
    [Header("Налаштування камери")]
    public float cameraMoveSpeed = 30f;
    public float cameraZoomSpeed = 20f;
    public float maxCameraDistance = 100f;
    public float zoomFactor = 4f; // Скільки разів можна віддалитися від початкової позиції Y
    public float minZoomOffset = 0.1f; // Мінімальна відстань до площини для перспективної камери

    private Camera editorCamera;
    private Collider editorPlaneCollider; // Collider площини для визначення поверхні
    private Vector3 initialCameraPosition;
    private float initialCameraY;

    public void Init(Camera camera, Collider planeCollider)
    {
        editorCamera = camera;
        editorPlaneCollider = planeCollider;

        if (editorCamera == null)
        {
            Debug.LogError("Камера редактора не знайдена для EditorCameraMovement!");
            enabled = false; // Вимикаємо скрипт, якщо камера не знайдена
            return;
        }

        initialCameraPosition = editorCamera.transform.position;
        initialCameraY = initialCameraPosition.y;
    }

    public void HandleInput()
    {
        if (editorCamera == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Vector3 currentPosition = editorCamera.transform.position;
        float moveDirection = Input.GetKey(KeyCode.LeftShift) ? -1f : 1f; // Змінюємо напрямок руху по осі X при Shift

        // Рух по X (Shift + Mouse Scroll)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            float moveX = scrollInput * cameraMoveSpeed * Time.unscaledDeltaTime * moveDirection;
            currentPosition += Vector3.right * moveX;
        }
        // Рух по Z (Ctrl + Mouse Scroll)
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            float moveZ = scrollInput * cameraMoveSpeed * Time.unscaledDeltaTime;
            currentPosition += Vector3.forward * moveZ;
        }
        // Масштабування (просто Mouse Scroll)
        else if (scrollInput != 0)
        {
            if (editorCamera.orthographic)
            {
                // Масштабування для ортографічної камери
                editorCamera.orthographicSize = Mathf.Clamp(editorCamera.orthographicSize - scrollInput * cameraZoomSpeed * Time.unscaledDeltaTime * 5f, initialCameraY / zoomFactor, initialCameraY);
            }
            else
            {
                // Масштабування для перспективної камери (рух вперед/назад)
                Vector3 zoomDirection = editorCamera.transform.forward;
                currentPosition += zoomDirection * scrollInput * cameraZoomSpeed * Time.unscaledDeltaTime * 10f;
            }
        }

        // Обмеження руху по XZ площині
        Vector2 currentXZ = new Vector2(currentPosition.x, currentPosition.z);
        if (currentXZ.magnitude > maxCameraDistance)
        {
            currentXZ = currentXZ.normalized * maxCameraDistance;
            currentPosition.x = currentXZ.x;
            currentPosition.z = currentXZ.y;
        }

        // Обмеження масштабування по осі Y для перспективної камери
        if (!editorCamera.orthographic)
        {
            float planeY = editorPlaneCollider != null ? editorPlaneCollider.bounds.max.y : 0f;
            float minZoomY = planeY + initialCameraY / zoomFactor + minZoomOffset;
            float maxZoomY = planeY + initialCameraY;
            currentPosition.y = Mathf.Clamp(currentPosition.y, minZoomY, maxZoomY);
        }


        editorCamera.transform.position = currentPosition;
    }
}