using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurretBehaviour : MonoBehaviour, IEnemy
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 200;
    private int currentHealth;

    [Header("Weapon")]
    [SerializeField] private LaserWeapon laserWeapon;
    [SerializeField] private Transform firePoint;

    [Header("Detection")]
    [SerializeField] private float attackRange = 100f;
    [SerializeField] private float rotationSpeed = 5f;

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
    [SerializeField] private ParticleSystem explosion;
    [SerializeField] private float destroyTime = 0.1f;

    [Header("Editor")]
    [SerializeField] private ObjectRadius objectRadius;

    private MainPlayer player;
    private bool canAttack = true;
    private Coroutine attackRoutine;

    public event Action<IEnemy> EnemyDied;

    private void Start()
    {
        player = FindFirstObjectByType<MainPlayer>();

        if (laserWeapon == null)
        {
            laserWeapon = GetComponent<LaserWeapon>();
            if (laserWeapon == null)
            {
                Debug.LogError("LaserWeapon component not found on " + gameObject.name + "! Turret will not be able to shoot.");
            }
        }
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned on " + gameObject.name + "! Turret will not be able to shoot.");
        }

        objectRadius.SetRange(attackRange);

        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (player == null || laserWeapon == null || firePoint == null) return;

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
        if (horizontalBase != null)
        {
            horizontalBase.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
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
            float currentAngleX = currentEuler.x;
            if (currentAngleX > 180f) currentAngleX -= 360f;

            currentEuler.x = Mathf.LerpAngle(currentAngleX, angleX, Time.deltaTime * rotationSpeed);
            if (currentEuler.x < 0) currentEuler.x += 360;

            turretHead.localEulerAngles = currentEuler;
        }
    }


    public IEnumerator AttackCoroutine()
    {
        canAttack = false;

        if (laserWeapon != null && firePoint != null && player != null)
        {
            laserWeapon.FireLaser(firePoint.position, player.transform.position, player);
        }


        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        attackRoutine = null;
    }

    public void HitPlayer()
    {
        Debug.Log("LaserTurretBehaviour: HitPlayer called (damage applied by LaserWeapon)");
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0 || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (currentHealth <= 0)
        {
            canAttack = false;

            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            if (explosion != null)
            {
                explosion.Play();
            }


            if (laserWeapon != null)
            {
                laserWeapon.DestroyAllLasers();
            }


            if (player != null && player.ScoreManager != null)
            {
                player.ScoreManager.AddScore(score);
            }
            else
            {
                Debug.LogWarning("Cannot add score: player or ScoreManager is null.");
            }


            EnemyDied?.Invoke(this);

            Invoke("DeleteObject", destroyTime);
        }
    }

    void DeleteObject()
    {
        Destroy(gameObject);
    }
}