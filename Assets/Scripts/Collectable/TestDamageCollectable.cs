using UnityEngine;

public class TestDamageCollectable : Collectable
{
    [SerializeField] private int damageCount;
    public override void OnBeingCollectedBy(MainPlayer character)
    {
        character.PlayerHealthManager.ReduceHealth(damageCount);
        Destroy(gameObject);
    }
}
