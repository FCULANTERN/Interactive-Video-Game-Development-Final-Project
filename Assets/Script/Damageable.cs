using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Hit Effects")]
    public float knockbackForce = 3f;
    public float stunDuration = 0.2f;
    public GameObject hitParticlePrefab;

    [Header("Death")]
    public float tipDuration = 0.5f;
    public float groundDelay = 10f;
    public float fadeDuration = 0.8f;

    private int currentHealth;
    public bool isDead { get; private set; }
    private Rigidbody rb;
    private Renderer[] renderers;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();

        // 確保 Decal 在遠距離也可見
        var decal = GetComponentInChildren<DecalProjector>();
        if (decal != null) decal.drawDistance = 1000f;
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isDead) return;
        currentHealth -= damage;

        ApplyKnockback(hitDirection);
        SpawnHitParticle();

        if (currentHealth <= 0)
            Die();
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    void ApplyKnockback(Vector3 direction)
    {
        if (rb == null || direction == Vector3.zero) return;

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.Stun(stunDuration);

        rb.linearVelocity = Vector3.zero;
        direction.y = 0f;
        rb.AddForce(direction.normalized * knockbackForce, ForceMode.Impulse);
    }

    void SpawnHitParticle()
    {
        if (hitParticlePrefab == null) return;
        Vector3 pos = transform.position + Vector3.up;
        GameObject fx = Instantiate(hitParticlePrefab, pos, Quaternion.identity);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();
        Destroy(fx, 2f);
    }

void Die()
    {
        if (isDead) return;
        isDead = true;

        // 立即關掉嘴巴 Decal，避免倒地後投影到地板
        var decal = GetComponentInChildren<DecalProjector>();
        if (decal != null) decal.enabled = false;

        // 立即停用 EnemyAI，防止死亡後還能攻擊
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        // 擊殺敵人獲得分數
        if (ai != null && ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(10);

        // 掉落金幣
        GoldDrop goldDrop = GetComponent<GoldDrop>();
        if (goldDrop != null)
            goldDrop.DropGold();

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        StopCoroutine("BlinkEffect");
        foreach (var r in renderers)
            foreach (var m in r.materials)
                if (m.HasProperty("_EmissionColor"))
                {
                    m.SetColor("_EmissionColor", Color.black);
                    m.DisableKeyword("_EMISSION");
                }

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, 90f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tipDuration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, Mathf.Clamp01(t));
            yield return null;
        }

        yield return new WaitForSeconds(groundDelay);

        foreach (var r in renderers)
            foreach (var m in r.materials)
                MakeTransparent(m);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float alpha = 1f - Mathf.Clamp01(t);
            foreach (var r in renderers)
                foreach (var m in r.materials)
                {
                    if (m.HasProperty("_BaseColor"))
                    {
                        Color c = m.GetColor("_BaseColor");
                        c.a = alpha;
                        m.SetColor("_BaseColor", c);
                    }
                    else if (m.HasProperty("_Color"))
                    {
                        Color c = m.GetColor("_Color");
                        c.a = alpha;
                        m.SetColor("_Color", c);
                    }
                }
            yield return null;
        }

        Destroy(gameObject);
    }

    void MakeTransparent(Material m)
    {
        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.renderQueue = 3000;
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        else if (m.HasProperty("_Mode"))
        {
            m.SetFloat("_Mode", 2);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_ALPHAFADE_ON");
            m.renderQueue = 3000;
        }
    }
}