using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] public List<WeaponScript> weapons;
    [SerializeField] private Transform spawnPoint;
    private List<WeaponScript> availableWeapons;
    private int index;
    private WeaponScript selectedWeapon;

    public event Action<WeaponScript> WeaponChanged;

    public WeaponScript SelectedWeapon => selectedWeapon;
    public IReadOnlyList<WeaponScript> AvailableWeapons => availableWeapons;

    void Start()
    {
        availableWeapons = new List<WeaponScript>();
        AddWeapon(WeaponType.LaserGun);
    }

    void Update()
    {
        WeaponScript previousWeapon = selectedWeapon;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (index >= availableWeapons.Count - 1)
                index = 0;
            else
                index++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (index <= 0)
                index = availableWeapons.Count - 1;
            else
                index--;
        }
        selectedWeapon = availableWeapons[index];

        if (selectedWeapon != previousWeapon)
        {
            SelectWeapon();
        }
    }

    public void AddWeapon(WeaponType newWeapon)
    {
        foreach (WeaponScript weapon in weapons)
        {
            if (weapon.WeaponType == newWeapon)
            {
                if (HasWeaponOfType(newWeapon))
                {
                    WeaponScript weaponAmmo = GetWeaponOfType(newWeapon);
                    weaponAmmo.AddBullets(weaponAmmo.MagazineSize);
                }
                else
                {
                    availableWeapons.Add(weapon);
                    if (selectedWeapon != null)
                        selectedWeapon.gameObject.SetActive(false);
                    index = availableWeapons.Count - 1;

                    selectedWeapon = availableWeapons[index];
                    selectedWeapon.gameObject.SetActive(true);
                    WeaponChanged?.Invoke(selectedWeapon);
                    return;
                }
            }
        }
    }

    private void SelectWeapon()
    {
        selectedWeapon = availableWeapons[index];
        foreach (WeaponScript weapon in availableWeapons)
        {
            if (weapon == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
        }

        WeaponChanged?.Invoke(selectedWeapon);

    }

    public bool HasWeaponOfType(WeaponType neededType)
    {
        foreach (WeaponScript weapon in availableWeapons)
        {
            if (weapon.WeaponType == neededType)
            {
                return true;
            }
        }
        return false;
    }

    public WeaponScript GetWeaponOfType(WeaponType neededType)
    {
        foreach (WeaponScript weapon in availableWeapons)
        {
            if (weapon.WeaponType == neededType)
            {
                return weapon;
            }
        }
        return null;
    }
}
