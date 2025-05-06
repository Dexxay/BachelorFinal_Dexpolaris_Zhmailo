using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallDetector : MonoBehaviour
{
    [SerializeField] private float fallThreshold = -50f;
    [SerializeField] private int fallDamage = 500;

    private PlayerHealthManager playerHealthManager;

    void Start()
    {
        playerHealthManager = GetComponent<PlayerHealthManager>();

        if (playerHealthManager == null)
        {
            Debug.LogError("PlayerHealthManager компонент не знайдений на об'єкті " + gameObject.name);
        }
    }

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            playerHealthManager?.ReduceHealth(fallDamage);
        }
    }
}
