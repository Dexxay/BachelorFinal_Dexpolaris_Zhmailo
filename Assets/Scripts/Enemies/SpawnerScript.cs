using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 500;
    private int currentHealth;

    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemy;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float spawnAltitude = 30f;
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float waveDelay = 5f;
    [SerializeField] private float playerActivationDistance = 150f;

    [Header("Enemies amount settings")]
    [SerializeField] private int initialAmount = 5;
    [SerializeField] private int maxAmount = 20;
    [SerializeField] private int stepForIncrease = 2;
    [SerializeField] private int maxAliveAmount = 10;

    [Header("Effects")]
    [SerializeField] private ParticleSystem explosion;
    [SerializeField] private ParticleSystem spawnFlash;
    [SerializeField] private float destroyTime = 0.1f;

    [Header("Score")]
    [SerializeField] private int score = 150;

    private MainPlayer player;

    private int currentEnemiesAmount;
    private int currentSpawnedAmount;
    private int waveCount;
    private int aliveObjectsCount;
    private bool canSpawn = true;
    private bool isPlayerInRange = false;

    public int WaveCount => waveCount;

    void Start()
    {
        currentHealth = maxHealth;
        player = FindFirstObjectByType<MainPlayer>();

        if (player == null)
        {
            Debug.LogWarning("MainPlayer not found! Spawner will not activate.");
            canSpawn = false;
            return;
        }

        currentEnemiesAmount = initialAmount;
        waveCount = 0;
    }

    void Update()
    {
        if (!isPlayerInRange && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= playerActivationDistance)
            {
                isPlayerInRange = true;
                Debug.Log("Player entered spawn range. Starting spawn.");
                StartNewWave();
            }
        }
    }

    private void StartNewWave()
    {
        if (!canSpawn) return;

        waveCount++;
        Debug.Log($"Starting Wave {waveCount}. Spawning {currentEnemiesAmount} enemies.");
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        currentSpawnedAmount = 0;
        while (currentSpawnedAmount < currentEnemiesAmount)
        {
            if (canSpawn && aliveObjectsCount < maxAliveAmount && player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer <= playerActivationDistance)
                {
                    SpawnNewObject();
                    currentSpawnedAmount++;

                    if (spawnFlash != null)
                        spawnFlash.Play();

                    yield return new WaitForSeconds(spawnDelay);
                }
                else
                {
                    Debug.Log("Player left activation range during spawn wave. Pausing spawn.");
                    isPlayerInRange = false;
                    yield break;
                }
            }
            else if (!canSpawn)
            {
                Debug.Log("Spawner is inactive. Stopping spawn wave.");
                yield break;
            }
            else if (aliveObjectsCount >= maxAliveAmount)
            {
                yield return null;
            }
            else
            {
                Debug.LogWarning("Cannot spawn: Conditions not met (player null or other issue).");
                yield break;
            }
        }

        Debug.Log($"Wave {waveCount} spawn finished. Waiting for wave delay.");

        yield return new WaitForSeconds(waveDelay);

        IncreaseAmount();
        StartNewWave();
    }

    private void SpawnNewObject()
    {
        aliveObjectsCount++;

        Vector2 randomCirclePoint = UnityEngine.Random.insideUnitCircle * spawnRadius;

        Vector3 spawnPoint = new Vector3(
            transform.position.x + randomCirclePoint.x,
            spawnAltitude,
            transform.position.z + randomCirclePoint.y
        );

        GameObject newObject = Instantiate(enemy, spawnPoint, Quaternion.identity);

        if (newObject.TryGetComponent(out IEnemy enemyComponent))
        {
            enemyComponent.EnemyDied += OnEnemyDied;
        }
    }

    private void OnEnemyDied(IEnemy obj)
    {
        aliveObjectsCount--;
        obj.EnemyDied -= OnEnemyDied;
    }

    private void IncreaseAmount()
    {
        currentEnemiesAmount += stepForIncrease;
        if (currentEnemiesAmount > maxAmount)
            currentEnemiesAmount = maxAmount;
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
        if (currentHealth <= 0 && canSpawn)
        {
            canSpawn = false;

            Debug.Log(gameObject.name + " has been destroyed!");

            if (explosion != null)
            {
                explosion.Play();
            }

            if (player != null && player.ScoreManager != null)
            {
                player.ScoreManager.AddScore(score);
            }
            else
            {
                Debug.LogWarning("Cannot add score: player or ScoreManager is null.");
            }

            StopAllCoroutines();

            Destroy(gameObject, destroyTime);
        }
    }
}