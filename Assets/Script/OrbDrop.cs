using UnityEngine;

public class OrbDrop : MonoBehaviour
{
    [Header("Orb Prefabs")]
    public GameObject healthOrbPrefab;

    [Header("Drop Chance")]
    [Range(0f, 1f)] public float healthDropChance = 0.15f;

    [Header("Spawn")]
    public float spawnHeight = 1f;

    public void TryDropOrb()
    {
        if (healthOrbPrefab != null && Random.value < healthDropChance)
            Spawn(healthOrbPrefab);
    }

    void Spawn(GameObject prefab)
    {
        Vector3 pos = transform.position + Vector3.up * spawnHeight;
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
