using UnityEngine;

/// <summary>
/// 爆炸型敵人：靠近玩家後開始閃爍倒數，時間到造成範圍傷害後自毀（苦力怕風格）。
/// 需搭配 Damageable、Rigidbody、Collider。可加上 GoldDrop 讓爆炸也掉金幣。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ExplodingEnemyAI : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 2f;
    public float rotateSpeed = 8f;

    [Header("Fuse")]
    [Tooltip("距離玩家多近時開始引信倒數")]
    public float fuseDistance = 2.5f;
    [Tooltip("引信時間（秒），時間到爆炸")]
    public float fuseTime = 1.5f;

    [Header("Explosion")]
    [Tooltip("爆炸傷害範圍")]
    public float explosionRadius = 3f;
    [Tooltip("爆炸對玩家的傷害")]
    public int explosionDamage = 3;

    [Header("Death Reward")]
    public int scoreReward = 15;
    public int goldReward = 15;

    // ── 內部狀態 ──────────────────────────────────────────────
    private Rigidbody rb;
    private Transform target;
    private PlayerHealth playerHealth;
    private Damageable selfDamageable;
    private Renderer[] renderers;

    private bool isFusing = false;
    private float fuseTimer;
    private bool exploded = false;

    void Start()
    {
        rb               = GetComponent<Rigidbody>();
        selfDamageable   = GetComponent<Damageable>();
        renderers        = GetComponentsInChildren<Renderer>();
        fuseTimer        = fuseTime;
        FindPlayer();
    }

    void FixedUpdate()
    {
        // 玩家已死，或自身已死（被打死而非爆炸），停止一切
        if (playerHealth != null && playerHealth.IsDead) return;
        if (selfDamageable != null && selfDamageable.isDead) { enabled = false; return; }
        if (exploded) return;

        if (target == null) { FindPlayer(); return; }

        float distance = Vector3.Distance(transform.position, target.position);

        // 進入引爆範圍 → 開始倒數
        if (!isFusing && distance <= fuseDistance)
        {
            isFusing  = true;
            fuseTimer = fuseTime;
        }

        if (isFusing)
        {
            fuseTimer -= Time.fixedDeltaTime;
            // 倒數時原地不動，只閃爍
            if (fuseTimer <= 0f)
            {
                Explode();
            }
            return;
        }

        // 普通追蹤移動
        MoveTowardPlayer();
    }

    void Update()
    {
        // 閃爍效果放在 Update 讓動畫更流暢
        if (isFusing && !exploded)
            ApplyFuseEffect();
    }

    // ── 追蹤移動 ─────────────────────────────────────────────
    void MoveTowardPlayer()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.01f) return;

        Vector3 dir = toTarget.normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);

        Quaternion targetRot = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
    }

    // ── 引信閃爍（顏色在紅色和白色之間快速閃動，越接近爆炸越快）───
    void ApplyFuseEffect()
    {
        float progress  = 1f - Mathf.Clamp01(fuseTimer / fuseTime);   // 0→1
        float blinkRate = Mathf.Lerp(4f, 20f, progress);               // 越到後期越快
        float intensity = Mathf.Abs(Mathf.Sin(Time.time * blinkRate)) * 2f;
        Color emitColor = Color.Lerp(Color.red, Color.white, intensity * 0.5f) * intensity;

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_EmissionColor"))
                {
                    m.SetColor("_EmissionColor", emitColor);
                    m.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    // ── 爆炸 ─────────────────────────────────────────────────
    void Explode()
    {
        if (exploded) return;
        exploded = true;

        // 對範圍內玩家造成傷害
        if (target != null && Vector3.Distance(transform.position, target.position) <= explosionRadius)
        {
            if (playerHealth != null)
                playerHealth.TakeDamage(explosionDamage);
        }

        // 給分
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreReward);

        // 掉金幣
        GoldDrop goldDrop = GetComponent<GoldDrop>();
        if (goldDrop != null)
            goldDrop.DropGold();

        Destroy(gameObject);
    }

    // ── 工具 ─────────────────────────────────────────────────
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target       = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }
}
