using UnityEngine;

public class ObjectRadius : MonoBehaviour
{
    [SerializeField] public float radius = 5.0f;
    void OnDrawGizmosSelected()

    {

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, radius);

    }
}