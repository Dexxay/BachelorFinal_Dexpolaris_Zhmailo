using UnityEngine;

public class LaserBeamAutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
