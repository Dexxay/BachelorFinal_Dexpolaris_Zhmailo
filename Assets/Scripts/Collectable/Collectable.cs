using UnityEngine;

public abstract class Collectable : MonoBehaviour
{
    public abstract void OnBeingCollectedBy(MainPlayer character);

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MainPlayer player = other.GetComponent<MainPlayer>();
            if (player != null)
            {
                OnBeingCollectedBy(player);
            }
        }
    }
}