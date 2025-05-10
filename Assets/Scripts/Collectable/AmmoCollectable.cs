using UnityEngine;

public class AmmoCollectable : Collectable
{
    [SerializeField] private WeaponType ammoType;
    [SerializeField] private int ammoCount;
    [SerializeField] private float dissapearTime;
    private float time;

    void Start()
    {
        time = 0;
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time >= dissapearTime)
        {
            Destroy(gameObject);
        }
    }

    public override void OnBeingCollectedBy(MainPlayer character)
    {
        if (character.WeaponManager.HasWeaponOfType(ammoType))
        {
            WeaponScript weaponAmmo = character.WeaponManager.GetWeaponOfType(ammoType);
            weaponAmmo.AddBullets(ammoCount);
            Destroy(gameObject);
        }
    }
}