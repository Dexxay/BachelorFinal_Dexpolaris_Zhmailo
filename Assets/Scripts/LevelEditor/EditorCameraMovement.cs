using UnityEngine;

[RequireComponent(typeof(Camera))] // ������ �������� ���������� Camera �� ���� � ��'���
public class EditorCameraMovement : MonoBehaviour
{
    [Header("������������ ������")]
    public float cameraMoveSpeed = 30f;
    public float cameraZoomSpeed = 20f;
    public float maxCameraDistance = 100f;
    public float zoomFactor = 4f; // ������ ���� ����� ���������� �� ��������� ������� Y
    public float minZoomOffset = 0.1f; // ̳������� ������� �� ������� ��� ������������ ������

    private Camera editorCamera;
    private Collider editorPlaneCollider; // Collider ������� ��� ���������� �������
    private Vector3 initialCameraPosition;
    private float initialCameraY;

    public void Init(Camera camera, Collider planeCollider)
    {
        editorCamera = camera;
        editorPlaneCollider = planeCollider;

        if (editorCamera == null)
        {
            Debug.LogError("������ ��������� �� �������� ��� EditorCameraMovement!");
            enabled = false; // �������� ������, ���� ������ �� ��������
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
        float moveDirection = Input.GetKey(KeyCode.LeftShift) ? -1f : 1f; // ������� �������� ���� �� �� X ��� Shift

        // ��� �� X (Shift + Mouse Scroll)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            float moveX = scrollInput * cameraMoveSpeed * Time.unscaledDeltaTime * moveDirection;
            currentPosition += Vector3.right * moveX;
        }
        // ��� �� Z (Ctrl + Mouse Scroll)
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            float moveZ = scrollInput * cameraMoveSpeed * Time.unscaledDeltaTime;
            currentPosition += Vector3.forward * moveZ;
        }
        // ������������� (������ Mouse Scroll)
        else if (scrollInput != 0)
        {
            if (editorCamera.orthographic)
            {
                // ������������� ��� ������������ ������
                editorCamera.orthographicSize = Mathf.Clamp(editorCamera.orthographicSize - scrollInput * cameraZoomSpeed * Time.unscaledDeltaTime * 5f, initialCameraY / zoomFactor, initialCameraY);
            }
            else
            {
                // ������������� ��� ������������ ������ (��� ������/�����)
                Vector3 zoomDirection = editorCamera.transform.forward;
                currentPosition += zoomDirection * scrollInput * cameraZoomSpeed * Time.unscaledDeltaTime * 10f;
            }
        }

        // ��������� ���� �� XZ ������
        Vector2 currentXZ = new Vector2(currentPosition.x, currentPosition.z);
        if (currentXZ.magnitude > maxCameraDistance)
        {
            currentXZ = currentXZ.normalized * maxCameraDistance;
            currentPosition.x = currentXZ.x;
            currentPosition.z = currentXZ.y;
        }

        // ��������� ������������� �� �� Y ��� ������������ ������
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