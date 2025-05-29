using UnityEngine;

public class PendulumRotatorY : MonoBehaviour
{
    [Header("Налаштування обертання навколо вісі Y")]
    [SerializeField] private float maxAngle = 15f;
    [SerializeField] private float maxSpeed = 1f;

    private float angleAmplitude;
    private float speed;
    private float phase;
    private float baseYRotation;

    private void Start()
    {
        baseYRotation = transform.localEulerAngles.y;
        angleAmplitude = Random.Range(10f, maxAngle);
        speed = Random.Range(0.5f, maxSpeed);
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float yAngle = Mathf.Sin(Time.time * speed + phase) * angleAmplitude;
        Vector3 newEulerAngles = transform.localEulerAngles;
        newEulerAngles.y = baseYRotation + yAngle;
        transform.localEulerAngles = newEulerAngles;
    }
}
