using System;
using UnityEngine;
using TMPro;

public class WeaponScript : MonoBehaviour
{
    [SerializeField] private WeaponType weaponType;

    [Header("Damage & Ammo")]
    [SerializeField] private int damage;
    [SerializeField] private int maxAmmoAmount;
    [SerializeField] private int magazineSize;
    [SerializeField] private int bulletsPerTap;
    [SerializeField] private bool allowButtonHold;

    [Header("Timing & Spread")]
    [SerializeField] private float timeBetweenShooting;
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private float spreadX, spreadY;
    [SerializeField] private float range;
    [SerializeField] private float reloadTime;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private Animator animator;

    [Header("Raycast")]
    [SerializeField] private RaycastHit rayHit;
    [SerializeField] private LayerMask ignoreLayers;

    private int bulletsAmount;
    private int bulletsLeftInMagazine;
    private int bulletsPerShotLeft;

    private bool shooting;
    private bool readyToShoot = true;
    private bool reloading = false;

    public event Action<WeaponScript> AmmoChanged;

    public int BulletsAmount => bulletsAmount;
    public int BulletsLeftInMagazine => bulletsLeftInMagazine;
    public int MagazineSize => magazineSize;
    public WeaponType WeaponType => weaponType;
    public bool Reloading => reloading;

    private void Awake()
    {
        bulletsLeftInMagazine = magazineSize;
        bulletsAmount = 0;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeftInMagazine < magazineSize && !reloading)
        {
            Reload();
        }

        if (bulletsLeftInMagazine <= 0 && !reloading)
        {
            Reload();
        }

        if (readyToShoot && shooting && !reloading)
        {
            if (bulletsLeftInMagazine > 0)
            {
                bulletsPerShotLeft = bulletsPerTap;
                Shoot();
            }
            else
            {
                Reload();
            }
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        muzzleFlash.Play();
        audioSource.PlayOneShot(shotSound);
        animator.SetTrigger("RECOIL");

        float x = UnityEngine.Random.Range(-spreadX, spreadX);
        float y = UnityEngine.Random.Range(-spreadY, spreadY);
        Vector3 direction = Camera.main.transform.forward + new Vector3(x, y, 0);

        if (Physics.Raycast(Camera.main.transform.position, direction, out rayHit, range, ~ignoreLayers))
        {
            Debug.Log(rayHit.collider.name);
            IDamagable target = rayHit.transform.GetComponent<IDamagable>();
            if (target != null)
            {
                audioSource.PlayOneShot(hitSound);
                target.TakeDamage(damage);
            }
            else
            {
                Vector3 position = rayHit.point + rayHit.normal * 0.005f;
                Quaternion rotation = Quaternion.LookRotation(rayHit.normal);

                GameObject bulletHole = EffectsManager.Instance.SpawnBulletHole(position, rotation);
                bulletHole.transform.SetParent(rayHit.transform, true);
            }
        }

        bulletsLeftInMagazine--;
        bulletsPerShotLeft--;

        AmmoChanged?.Invoke(this);

        Invoke(nameof(ResetShot), timeBetweenShooting);

        if (bulletsPerShotLeft > 0 && bulletsLeftInMagazine > 0)
        {
            Invoke(nameof(Shoot), timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        reloading = true;
        int bulletsToReload = magazineSize - bulletsLeftInMagazine;

        if (bulletsAmount >= bulletsToReload)
        {
            bulletsAmount -= bulletsToReload;
            bulletsLeftInMagazine += bulletsToReload;
        }
        else if (bulletsAmount > 0)
        {
            bulletsLeftInMagazine += bulletsAmount;
            bulletsAmount = 0;
        }
        else
        {
            ReloadFinished();
            return;
        }

        animator.SetTrigger("RELOAD");
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        reloading = false;
        AmmoChanged?.Invoke(this);
    }

    public void AddBullets(int amount)
    {
        if (amount <= 0) return;

        bulletsAmount += amount;
        if (bulletsAmount > maxAmmoAmount)
            bulletsAmount = maxAmmoAmount;

        AmmoChanged?.Invoke(this);
    }
}
