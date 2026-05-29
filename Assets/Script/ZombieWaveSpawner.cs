using System;
using System.Collections.Generic;
using UnityEngine;

public class ZombieWaveSpawner : MonoBehaviour
{
    public static ZombieWaveSpawner Instance { get; private set; }
    public event Action<int> OnWaveChanged;
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
    public float minSpawnDistanceFromPlayer = 5f;
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

    void Awake()
    {
        Instance = this;
    }

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

        OnWaveChanged?.Invoke(currentWave);
        Debug.Log("Wave " + currentWave + " started. Enemies: " + enemiesToSpawnThisWave);
    }

    void SpawnEnemy()
    {
        if (zombiePrefab == null)
        {
            Debug.LogWarning("ZombieWaveSpawner: zombiePrefab �S�����w�C");
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemy = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        aliveEnemies.Add(enemy);
        aliveEnemyCount = aliveEnemies.Count;
    }

    Vector3 GetSpawnPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            // 從所有足夠遠的生成點中隨機選一個
            List<Transform> validPoints = new List<Transform>();
            foreach (var sp in spawnPoints)
            {
                if (sp != null && Vector3.Distance(sp.position, playerPos) >= minSpawnDistanceFromPlayer)
                    validPoints.Add(sp);
            }

            if (validPoints.Count > 0)
                return validPoints[UnityEngine.Random.Range(0, validPoints.Count)].position;

            // 如果所有點都太近，選最遠的
            Transform farthest = spawnPoints[0];
            float maxDist = 0f;
            foreach (var sp in spawnPoints)
            {
                if (sp == null) continue;
                float d = Vector3.Distance(sp.position, playerPos);
                if (d > maxDist) { maxDist = d; farthest = sp; }
            }
            return farthest.position;
        }

        Vector3 randomPos = areaCenter;
        randomPos.x += UnityEngine.Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        randomPos.y += UnityEngine.Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
        randomPos.z += UnityEngine.Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f);
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