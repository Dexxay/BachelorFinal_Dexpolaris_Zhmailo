using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedkitCollectable : Collectable
{
    [SerializeField] private int healthCount;
    public override void OnBeingCollectedBy(MainPlayer character)
    {
        if (character.PlayerHealthManager.Health < character.PlayerHealthManager.MaxHealth)
        {
            character.PlayerHealthManager.AddHealth(healthCount);
            Destroy(gameObject);
        }
    }
}
