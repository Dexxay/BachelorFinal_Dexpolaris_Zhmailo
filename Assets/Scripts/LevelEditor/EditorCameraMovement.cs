using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EditorCameraMovement : MonoBehaviour
{
    [Header("Налаштування камери")]
    public float wasdMoveSpeed = 20f;
    public float cameraZoomSpeed = 20f;
    public float cameraRotationSpeed = 100f;
    public float maxCameraDistance = 100f;
    public float zoomFactor = 4f; 
    public float minZoomOffset = 0.1f;

    private Camera editorCamera;
    private Collider editorPlaneCollider;
    private Vector3 initialCameraPosition;
    private float initialCameraY;

    private const float MinPitch = -45f;
    private const float MaxPitch = 75f;

    public void Init(Camera camera, Collider planeCollider)
    {
        editorCamera = camera;
        editorPlaneCollider = planeCollider;

        if (editorCamera == null)
        {
            Debug.LogError("No editor camera for EditorCameraMovement found!");
            enabled = false;
            return;
        }

        initialCameraPosition = editorCamera.transform.position;
        initialCameraY = initialCameraPosition.y;

        Vector3 initialEuler = editorCamera.transform.rotation.eulerAngles;
        editorCamera.transform.rotation = Quaternion.Euler(initialEuler.x, initialEuler.y, 0);
    }

    public void HandleInput()
    {
        if (editorCamera == null) return;

        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            float deltaYaw = mouseX * cameraRotationSpeed * Time.unscaledDeltaTime;
            float deltaPitch = -mouseY * cameraRotationSpeed * Time.unscaledDeltaTime;

            editorCamera.transform.Rotate(Vector3.up, deltaYaw, Space.World);
            editorCamera.transform.Rotate(editorCamera.transform.right, deltaPitch, Space.Self);

            ClampPitch();
        }

        HandleWASDMovement();
        HandleScrollZoom();
        ApplyPositionClamping();
    }

    private void HandleWASDMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection += editorCamera.transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= editorCamera.transform.forward;
        if (Input.GetKey(KeyCode.D)) moveDirection += editorCamera.transform.right;
        if (Input.GetKey(KeyCode.A)) moveDirection -= editorCamera.transform.right;

        if (moveDirection.magnitude > 0)
        {
            Vector3 horizontalMoveDirection = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;

            editorCamera.transform.position += horizontalMoveDirection * wasdMoveSpeed * Time.unscaledDeltaTime;
        }
    }

    private void HandleScrollZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            Vector3 currentPosition = editorCamera.transform.position;
            if (editorCamera.orthographic)
            {
                editorCamera.orthographicSize = Mathf.Clamp(editorCamera.orthographicSize - scrollInput * cameraZoomSpeed * Time.unscaledDeltaTime * 5f, initialCameraY / zoomFactor, initialCameraY);
            }
            else
            {
                Vector3 zoomDirection = editorCamera.transform.forward;
                currentPosition += zoomDirection * scrollInput * cameraZoomSpeed * Time.unscaledDeltaTime * 10f;
                editorCamera.transform.position = currentPosition;
            }
        }
    }


    private void ClampPitch()
    {
        Vector3 currentEuler = editorCamera.transform.rotation.eulerAngles;
        float pitch = currentEuler.x;

        if (pitch > 180) pitch -= 360;

        pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);

        editorCamera.transform.rotation = Quaternion.Euler(pitch, currentEuler.y, 0);
    }

    private void ApplyPositionClamping()
    {
        if (editorCamera == null) return;

        Vector3 currentPosition = editorCamera.transform.position;

        Vector2 currentXZ = new Vector2(currentPosition.x, currentPosition.z);
        if (currentXZ.magnitude > maxCameraDistance)
        {
            currentXZ = currentXZ.normalized * maxCameraDistance;
            currentPosition.x = currentXZ.x;
            currentPosition.z = currentXZ.y;
        }

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