using UnityEngine;

public class LaserBeamAutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
