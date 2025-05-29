using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [SerializeField] private float mouseXSensetivity = 500f;
    [SerializeField] private float mouseYSensetivity = 500f;
    [SerializeField] bool invertMouse = true;
    [Space]
    [SerializeField] private float topClamp = -90f;
    [SerializeField] private float bottomClamp = 90f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        xRotation = Camera.main.transform.localEulerAngles.x;
        if (xRotation > 180) xRotation -= 360;

        yRotation = transform.localEulerAngles.y;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseXSensetivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseYSensetivity * Time.deltaTime;

        if (invertMouse)
        {
            mouseY *= -1;
        }

        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);

        yRotation += mouseX;

        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public void ForceLookRotation(Quaternion cameraRotation, Quaternion bodyRotation)
    {
        Camera.main.transform.rotation = cameraRotation;
        transform.rotation = bodyRotation;

        Vector3 camEuler = Camera.main.transform.localEulerAngles;
        xRotation = camEuler.x;
        if (xRotation > 180) xRotation -= 360;

        yRotation = transform.localEulerAngles.y;
    }
}
