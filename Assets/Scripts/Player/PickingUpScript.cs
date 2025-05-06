using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickingUpScript : MonoBehaviour
{
    [SerializeField] private MainPlayer owner;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Entered object {other.name}");
        if (other.CompareTag("Collectable") && other.TryGetComponent(out Collectable collectable))
        {
            collectable.OnBeingCollectedBy(owner);
        }
    }
}
