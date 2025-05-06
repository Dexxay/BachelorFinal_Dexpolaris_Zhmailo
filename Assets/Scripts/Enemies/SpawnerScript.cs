using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [Space]
    [SerializeField] private float spawnRange;
    [SerializeField] private float spawnDelay;
    [SerializeField] private float waveDelay;
    [Space]
    [SerializeField] private int initialAmount;
    [SerializeField] private int maxAmount;
    [SerializeField] private int stepForIncrease;
    [Space]
    [SerializeField] private int maxAliveAmount;

    private int currentEnemiesAmount;
    private int currentSpawnedAmount;
    private int waveCount;
    private int aliveObjectsCount;

    public int WaveCount => waveCount;

    void Start()
    {
        currentEnemiesAmount = initialAmount;
        waveCount = 0;
        StartNewWave();
    }

    private void StartNewWave()
    {
        waveCount++;
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (currentSpawnedAmount = 0; currentSpawnedAmount < currentEnemiesAmount; currentSpawnedAmount++)
        {
            if (aliveObjectsCount < maxAliveAmount)
                SpawnNewObject();

            yield return new WaitForSeconds(spawnDelay);
        }

        yield return new WaitForSeconds(waveDelay);

        waveCount++;
        IncreaseAmount();
        StartNewWave();
    }

    private void SpawnNewObject()
    {
        aliveObjectsCount++;

        Vector3 spawnOffset = new Vector3(UnityEngine.Random.Range(0, spawnRange), 0f, UnityEngine.Random.Range(0, spawnRange));
        Vector3 spawnPoint = transform.position + spawnOffset;

        GameObject newObject = Instantiate(enemy, spawnPoint, Quaternion.identity);

        if (newObject.TryGetComponent(out IEnemy enemyComponent))
        {
            enemyComponent.EnemyDied += OnEnemyDied;
        }
    }

    private void OnEnemyDied(IEnemy obj)
    {
        aliveObjectsCount--;    
    }

    private void IncreaseAmount()
    {
        currentEnemiesAmount += stepForIncrease;
        if (currentEnemiesAmount > maxAmount)
            currentEnemiesAmount = maxAmount;

    }
}
