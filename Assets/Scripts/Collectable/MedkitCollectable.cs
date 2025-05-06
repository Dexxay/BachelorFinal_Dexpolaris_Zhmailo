using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedkitCollectable : Collectable
{
    [SerializeField] private int healthCount;
    public override void OnBeingCollectedBy(MainPlayer character)
    {
        character.PlayerHealthManager.AddHealth(healthCount);
        Destroy(gameObject);
    }
}
