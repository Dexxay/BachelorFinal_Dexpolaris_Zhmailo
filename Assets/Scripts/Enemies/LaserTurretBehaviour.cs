using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurretBehaviour : MonoBehaviour, IEnemy
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private Transform firePoint;
    private int currentHealth;

    [Header("Detection")]
    [SerializeField] private float attackRange = 100f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Laser Settings")]
    [SerializeField] private float laserSpeed = 50f;
    [SerializeField] private int laserDamage = 25;
    [SerializeField] private LineRenderer laserLinePrefab;
    [SerializeField] private LayerMask groundLayer;

    [Header("Timing")]
    [SerializeField] private float attackCooldown = 3f;

    [Header("Score")]
    [SerializeField] private int score = 50;

    [Header("Turret Parts")]
    [SerializeField] private Transform horizontalBase;
    [SerializeField] private Transform turretHead;

    [Header("Vertical Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -45f;
    [SerializeField] private float maxVerticalAngle = 45f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem explosion;
    [SerializeField] private float destroyTime = 0.1f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shotSound;

    private MainPlayer player;
    private bool canAttack = true;
    private Coroutine attackRoutine;

    private List<GameObject> activeLasers = new();

    public event Action<IEnemy> EnemyDied;

    private void Start()
    {
        player = FindFirstObjectByType<MainPlayer>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            Patrol();
        }
        else
        {
            RotateTowardsPlayer();
            if (canAttack && attackRoutine == null)
            {
                attackRoutine = StartCoroutine(AttackCoroutine());
            }
        }
    }

    private void Patrol()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector3 targetPos = player.transform.position;

        if (horizontalBase != null)
        {
            Vector3 flatDir = targetPos - horizontalBase.position;
            flatDir.y = 0f;

            if (flatDir != Vector3.zero)
            {
                Quaternion horizontalRot = Quaternion.LookRotation(flatDir);
                horizontalBase.rotation = Quaternion.Slerp(horizontalBase.rotation, horizontalRot, Time.deltaTime * rotationSpeed);
            }
        }

        if (turretHead != null)
        {
            Vector3 directionToPlayer = targetPos - turretHead.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

            float angleX = lookRotation.eulerAngles.x;
            if (angleX > 180f) angleX -= 360f;

            angleX = Mathf.Clamp(angleX, minVerticalAngle, maxVerticalAngle);

            Vector3 currentEuler = turretHead.localEulerAngles;
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, angleX, Time.deltaTime * rotationSpeed);
            turretHead.localEulerAngles = currentEuler;
        }
    }



    public IEnumerator AttackCoroutine()
    {
        canAttack = false;
        ShootLaser();
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        attackRoutine = null;
    }

    private void ShootLaser()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint is not assigned!");
            return;
        }

        muzzleFlash.Play();
        audioSource.PlayOneShot(shotSound);

        Vector3 startPosition = firePoint.position;
        Vector3 playerPosition = player.transform.position;

        Vector3 direction = (playerPosition - startPosition).normalized;

        float distanceToPlayer = Vector3.Distance(startPosition, playerPosition);
        float distanceMultiplier = 1.5f;

        Vector3 extendedTargetPosition = startPosition + direction * (distanceToPlayer * distanceMultiplier);

        GameObject laserGO = new GameObject("LaserBeam");
        LineRenderer lr = Instantiate(laserLinePrefab, laserGO.transform);
        lr.positionCount = 2;
        lr.SetPosition(0, startPosition);
        lr.SetPosition(1, startPosition);

        activeLasers.Add(laserGO);
        StartCoroutine(MoveLaser(lr, startPosition, extendedTargetPosition, laserGO));
    }


    private IEnumerator MoveLaser(LineRenderer lr, Vector3 start, Vector3 target, GameObject laserGO)
    {
        float distanceMultiplier = 1.5f;
        float elapsed = 0f;
        float distance = distanceMultiplier * Vector3.Distance(start, target);
        float travelTime =  distance / laserSpeed;
        bool hasHit = false;

        Color startColor = lr.startColor;
        startColor.a = 1f;
        lr.startColor = startColor;
        lr.endColor = startColor;

        lr.startWidth = 1f;
        lr.endWidth = 5f;

        while (elapsed < travelTime)
        {
            if (lr == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            Vector3 currentPosition = Vector3.Lerp(start, target, elapsed / travelTime);
            lr.SetPosition(1, currentPosition);

            float alpha = Mathf.Lerp(1f, 0f, elapsed / travelTime);
            Color currentColor = lr.startColor;
            currentColor.a = alpha;
            lr.startColor = currentColor;
            lr.endColor = currentColor;

            float width = Mathf.Lerp(0.2f, 0f, elapsed / travelTime);
            lr.startWidth = width;
            lr.endWidth = width;

            Vector3 rayDir = currentPosition - start;
            float rayDist = rayDir.magnitude;
            rayDir.Normalize();

            if (Physics.Raycast(start, rayDir, out RaycastHit hit, rayDist, groundLayer))
            {
                lr.SetPosition(1, hit.point);
                if (lr != null) Destroy(lr);

                Vector3 position = hit.point + hit.normal * 0.005f;
                Quaternion rotation = Quaternion.LookRotation(hit.normal);

                GameObject bulletHole = EffectsManager.Instance.SpawnBulletHole(position, rotation);
                bulletHole.transform.SetParent(hit.transform, true);

                break;
            }

            if (!hasHit && Vector3.Distance(currentPosition, player.transform.position) < 1.5f)
            {
                HitPlayer();
                hasHit = true;
                if (lr != null) Destroy(lr);
                break;
            }

            yield return null;
        }

        if (laserGO != null)
        {
            activeLasers.Remove(laserGO);
            Destroy(laserGO);
        }
    }


    public void HitPlayer()
    {
        player.PlayerHealthManager.ReduceHealth(laserDamage);
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0 || damageAmount <= 0) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        canAttack = false;
        explosion.Play();
        foreach (var laser in activeLasers)
        {
            if (laser != null)
            {
                Destroy(laser);
            }
        }
        activeLasers.Clear();
        player.ScoreManager.AddScore(score);

        EnemyDied?.Invoke(this);
        Invoke("DeleteObject", destroyTime);
    }

    void DeleteObject()
    {
        Destroy(gameObject);
    }
}