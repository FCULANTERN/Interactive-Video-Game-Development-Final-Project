using System.Collections.Generic;
using UnityEngine;

public class ZombieWaveSpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject zombiePrefab;

    [Header("Wave Settings")]
    public int startEnemyCount = 5;
    public int enemyIncreasePerWave = 2;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 0.5f;
    public bool autoStart = true;

    [Header("Spawn Area")]
    public bool useSpawnPoints = true;
    public Transform[] spawnPoints;
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(20f, 0f, 20f);

    [Header("Runtime Info")]
    public int currentWave = 0;
    public int aliveEnemyCount = 0;
    public bool isSpawningWave = false;
    public bool isWaitingNextWave = false;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private float waveTimer = 0f;
    private float spawnTimer = 0f;
    private int enemiesToSpawnThisWave = 0;
    private int enemiesSpawnedThisWave = 0;
    private bool waveStarted = false;

    void Start()
    {
        if (autoStart)
        {
            StartNextWave();
        }
    }

    void Update()
    {
        CleanupDestroyedEnemies();

        if (!waveStarted)
            return;

        if (isSpawningWave)
        {
            HandleSpawning();
            return;
        }

        if (aliveEnemyCount <= 0 && !isWaitingNextWave)
        {
            isWaitingNextWave = true;
            waveTimer = timeBetweenWaves;
        }

        if (isWaitingNextWave)
        {
            waveTimer -= Time.deltaTime;

            if (waveTimer <= 0f)
            {
                StartNextWave();
            }
        }
    }

    void HandleSpawning()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        if (enemiesSpawnedThisWave < enemiesToSpawnThisWave)
        {
            SpawnEnemy();
            enemiesSpawnedThisWave++;
            spawnTimer = timeBetweenSpawns;
        }

        if (enemiesSpawnedThisWave >= enemiesToSpawnThisWave)
        {
            isSpawningWave = false;
        }
    }

    public void StartNextWave()
    {
        currentWave++;
        enemiesToSpawnThisWave = startEnemyCount + (currentWave - 1) * enemyIncreasePerWave;
        enemiesSpawnedThisWave = 0;

        isWaitingNextWave = false;
        isSpawningWave = true;
        waveStarted = true;
        spawnTimer = 0f;

        Debug.Log("Wave " + currentWave + " started. Enemies: " + enemiesToSpawnThisWave);
    }

    void SpawnEnemy()
    {
        if (zombiePrefab == null)
        {
            Debug.LogWarning("ZombieWaveSpawner: zombiePrefab 沒有指定。");
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemy = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        aliveEnemies.Add(enemy);
        aliveEnemyCount = aliveEnemies.Count;
    }

    Vector3 GetSpawnPosition()
    {
        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex].position;
        }

        Vector3 randomPos = areaCenter;
        randomPos.x += Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        randomPos.y += Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
        randomPos.z += Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f);
        return randomPos;
    }

    void CleanupDestroyedEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }

        aliveEnemyCount = aliveEnemies.Count;
    }

    void OnDrawGizmosSelected()
    {
        if (!useSpawnPoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }
}