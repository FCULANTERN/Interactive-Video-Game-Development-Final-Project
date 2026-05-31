using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    [Tooltip("從第幾波開始生成此敵人（1 = 第一波起）")]
    public int minWave = 1;
}

public class ZombieWaveSpawner : MonoBehaviour
{
    public static ZombieWaveSpawner Instance { get; private set; }
    public event Action<int> OnWaveChanged;

    [Header("Enemy")]
    public EnemySpawnEntry[] enemyEntries;

    [Header("Wave Settings")]
    public int startEnemyCount = 5;
    public int enemyIncreasePerWave = 2;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 0.5f;
    public bool autoStart = true;

    [Header("Stat Scaling (per wave after wave 1)")]
    public float healthScalePerWave = 0.15f;
    public float damageScalePerWave = 0.10f;
    public float speedScalePerWave = 0.03f;
    public float maxSpeedMultiplier = 2f;

    [Header("Boss Waves")]
    public int bossWaveInterval = 5;
    public int bossCount = 1;
    public GameObject bossPrefab;
    public float bossHealthMultiplier = 8f;
    public float bossDamageMultiplier = 3f;
    public float bossSizeMultiplier = 2f;

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
    public bool isBossWave = false;

    [Header("Game Flow")]
    public int maxWaves = 30;
    public GameObject victoryPanel;
    public bool endlessMode = false;
    private bool gameFinished = false;

    public float WaveCountdown => waveTimer;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private readonly Queue<bool> spawnQueue = new Queue<bool>();
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

        if (gameFinished)
            return;

        Debug.Log(
            "WaveCheck -> currentWave: " + currentWave +
            " | maxWaves: " + maxWaves +
            " | endlessMode: " + endlessMode +
            " | aliveEnemyCount: " + aliveEnemyCount +
            " | isSpawningWave: " + isSpawningWave +
            " | isWaitingNextWave: " + isWaitingNextWave
        );

        if (!endlessMode && currentWave >= maxWaves && isWaitingNextWave && aliveEnemyCount <= 0)
        {
            ShowVictory();
            return;
        }

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

        if (enemiesSpawnedThisWave < enemiesToSpawnThisWave && spawnQueue.Count > 0)
        {
            SpawnEnemy(spawnQueue.Dequeue());
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
        if (!endlessMode && currentWave >= maxWaves)
            return;

        currentWave++;
        isBossWave = bossWaveInterval > 0 && currentWave % bossWaveInterval == 0;

        spawnQueue.Clear();
        int regularCount = startEnemyCount + (currentWave - 1) * enemyIncreasePerWave;
        for (int i = 0; i < regularCount; i++)
            spawnQueue.Enqueue(false);
        if (isBossWave)
            for (int i = 0; i < bossCount; i++)
                spawnQueue.Enqueue(true);

        enemiesToSpawnThisWave = spawnQueue.Count;
        enemiesSpawnedThisWave = 0;

        isWaitingNextWave = false;
        isSpawningWave = true;
        waveStarted = true;
        spawnTimer = 0f;

        OnWaveChanged?.Invoke(currentWave);

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnNewWave(currentWave);

        Debug.Log("Wave " + currentWave + " started. Enemies: " + enemiesToSpawnThisWave + (isBossWave ? " (BOSS WAVE)" : ""));
    }

    void SpawnEnemy(bool isBoss)
    {
        GameObject prefabToSpawn;

        if (isBoss && bossPrefab != null)
        {
            prefabToSpawn = bossPrefab;
        }
        else
        {
            List<GameObject> eligible = new List<GameObject>();
            if (enemyEntries != null)
            {
                foreach (var entry in enemyEntries)
                {
                    if (entry.prefab != null && currentWave >= entry.minWave)
                        eligible.Add(entry.prefab);
                }
            }

            if (eligible.Count == 0)
            {
                Debug.LogWarning("ZombieWaveSpawner: 目前波數沒有可用的敵人 prefab，請確認 enemyEntries 設定。");
                return;
            }

            prefabToSpawn = eligible[UnityEngine.Random.Range(0, eligible.Count)];
        }

        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        ApplyScaling(enemy, isBoss);
        aliveEnemies.Add(enemy);
        aliveEnemyCount = aliveEnemies.Count;
    }

    void ApplyScaling(GameObject enemy, bool isBoss)
    {
        float healthMult = 1f + (currentWave - 1) * healthScalePerWave;
        float damageMult = 1f + (currentWave - 1) * damageScalePerWave;
        float speedMult = Mathf.Min(1f + (currentWave - 1) * speedScalePerWave, maxSpeedMultiplier);

        if (isBoss)
        {
            healthMult *= bossHealthMultiplier;
            damageMult *= bossDamageMultiplier;
            enemy.transform.localScale *= bossSizeMultiplier;
        }

        Damageable dmg = enemy.GetComponent<Damageable>();
        if (dmg != null)
            dmg.maxHealth = Mathf.Max(1, Mathf.RoundToInt(dmg.maxHealth * healthMult));

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.attackDamage = Mathf.Max(1, Mathf.RoundToInt(ai.attackDamage * damageMult));
            ai.moveSpeed *= speedMult;
        }
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
        int living = 0;
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = aliveEnemies[i];
            if (enemy == null)
            {
                aliveEnemies.RemoveAt(i);
                continue;
            }

            Damageable dmg = enemy.GetComponent<Damageable>();
            if (dmg == null || !dmg.isDead)
                living++;
        }

        aliveEnemyCount = living;
    }

    void OnDrawGizmosSelected()
    {
        if (!useSpawnPoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }

    void ShowVictory()
    {
        gameFinished = true;
        Time.timeScale = 0f;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void StartEndlessMode()
    {
        endlessMode = true;

        gameFinished = false;

        Time.timeScale = 1f;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }
}