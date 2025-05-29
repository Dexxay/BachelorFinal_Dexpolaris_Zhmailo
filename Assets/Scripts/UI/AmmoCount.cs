using TMPro;
using UnityEngine;

public class AmmoCount : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text weaponType;


    void Start()
    {
        if (weaponManager.SelectedWeapon != null)
        {
            UpdateWeaponUI(weaponManager.SelectedWeapon);
        }
        weaponManager.WeaponChanged += UpdateWeaponUI;
    }

    private void UpdateWeaponUI(WeaponScript newWeapon)
    {
        weaponType.text = newWeapon.WeaponType.ToString();
        ammoText.text = $"{newWeapon.BulletsLeftInMagazine}/{newWeapon.BulletsAmount}";
        weaponManager.SelectedWeapon.AmmoChanged += UpdateAmmoUI;
    }

    private void UpdateAmmoUI(WeaponScript newWeapon)
    {
        if (newWeapon == weaponManager.SelectedWeapon)
        {
            ammoText.text = $"{newWeapon.BulletsLeftInMagazine}/{newWeapon.BulletsAmount}";
        }
    }
}
