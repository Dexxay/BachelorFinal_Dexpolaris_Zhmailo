using UnityEngine;

public class BlackHoleMover : MonoBehaviour
{
    [Header("Налаштування руху")]
    [SerializeField] private float targetYPosition;
    [SerializeField] public float moveDurationInSeconds = 60f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 direction;
    private float speed;

    void Start()
    {
        startPosition = transform.position;
        endPosition = new Vector3(startPosition.x, targetYPosition, startPosition.z);

        direction = (endPosition - startPosition).normalized;
        float distance = Vector3.Distance(startPosition, endPosition);
        speed = distance / moveDurationInSeconds;
    }

    void Update()
    {
        if (Vector3.Dot(direction, endPosition - transform.position) > 0f)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            transform.position = endPosition;
        }
    }
}
