using UnityEngine;

public class VerticalPendulumMover : MonoBehaviour
{
    [Header("Налаштування руху")]
    [SerializeField] private float maxHeightOffset = 0.5f;
    [SerializeField] private float maxSpeed = 0.5f;

    private float baseY;
    private float offset;
    private float speed;
    private float phase;

    private void Start()
    {
        baseY = transform.position.y;

        offset = Random.Range(0.5f, maxHeightOffset);
        speed = Random.Range(0.5f, maxSpeed);

        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float newY = baseY + Mathf.Sin(Time.time * speed + phase) * offset;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
