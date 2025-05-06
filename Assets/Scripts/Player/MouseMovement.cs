using System.Collections;
using System.Collections.Generic;
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
}
