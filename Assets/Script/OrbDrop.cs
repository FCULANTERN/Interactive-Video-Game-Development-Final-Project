using UnityEngine;

public class OrbDrop : MonoBehaviour
{
    [Header("Orb Prefabs")]
    public GameObject healthOrbPrefab;
    public GameObject manaOrbPrefab;

    [Header("Drop Chance")]
    [Range(0f, 1f)] public float healthDropChance = 0.15f;
    [Range(0f, 1f)] public float manaDropChance = 0.15f;

    [Header("Spawn")]
    public float spawnHeight = 1f;

    public void TryDropOrb()
    {
        bool chooseHealth = Random.value < 0.5f;

        GameObject prefab = chooseHealth ? healthOrbPrefab : manaOrbPrefab;
        float chance = chooseHealth ? healthDropChance : manaDropChance;

        if (prefab != null && Random.value < chance)
            Spawn(prefab);
    }

    void Spawn(GameObject prefab)
    {
        Vector3 pos = transform.position + Vector3.up * spawnHeight;
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
