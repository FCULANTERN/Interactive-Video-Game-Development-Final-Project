using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 暫時「冰凍」敵人：停止 AI 與物理，並把外觀染成冰白色。
/// 使用 MaterialPropertyBlock，不修改材質實例，build 不會因為 shader variant
/// 被 strip 而失效。
/// </summary>
public class FrozenEffect : MonoBehaviour
{
    /// <summary>染色顏色（偏冷的雪白）</summary>
    public static readonly Color IceTint = new Color(0.75f, 0.92f, 1f);
    /// <summary>染色強度 0~1（1 = 完全變冰色）</summary>
    public const float TintBlend = 0.85f;

    private float timer;
    private bool active;

    private readonly List<MonoBehaviour> disabledBehaviours = new List<MonoBehaviour>();
    private readonly List<Renderer> tintedRenderers = new List<Renderer>();
    private Rigidbody rb;
    private bool prevKinematic;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    /// <summary>對目標敵人施加冰凍。重複呼叫會延長時間。</summary>
    public static void Apply(GameObject target, float duration)
    {
        if (target == null || duration <= 0f) return;
        FrozenEffect fe = target.GetComponent<FrozenEffect>();
        if (fe == null) fe = target.AddComponent<FrozenEffect>();
        fe.Begin(duration);
    }

    void Begin(float duration)
    {
        timer = Mathf.Max(timer, duration);
        if (active) return;
        active = true;

        // 停止 AI
        DisableIfPresent(GetComponent<EnemyAI>());
        DisableIfPresent(GetComponent<RangedEnemyAI>());
        DisableIfPresent(GetComponent<ExplodingEnemyAI>());
        DisableIfPresent(GetComponent<DashEnemyAI>());

        // 停止物理
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            prevKinematic = rb.isKinematic;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        ApplyTint();
    }

    void ApplyTint()
    {
        Color tinted = Color.Lerp(Color.white, IceTint, TintBlend);
        var mpb = new MaterialPropertyBlock();
        Texture2D white = Texture2D.whiteTexture;

        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (r == null) continue;
            if (r is ParticleSystemRenderer) continue;

            r.GetPropertyBlock(mpb);
            // 用全白貼圖蓋掉原本的紋理，這樣 _BaseColor 才能直接顯示成冰色
            mpb.SetTexture(BaseMapId, white);
            mpb.SetTexture(MainTexId, white);
            mpb.SetColor(BaseColorId, tinted);
            mpb.SetColor(ColorId, tinted);
            r.SetPropertyBlock(mpb);

            tintedRenderers.Add(r);
        }
    }

    void ClearTint()
    {
        foreach (var r in tintedRenderers)
        {
            if (r == null) continue;
            r.SetPropertyBlock(null);
        }
        tintedRenderers.Clear();
    }

    void DisableIfPresent(MonoBehaviour mb)
    {
        if (mb != null && mb.enabled)
        {
            mb.enabled = false;
            disabledBehaviours.Add(mb);
        }
    }

    void Update()
    {
        if (!active) return;
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        Restore();
    }

    void Restore()
    {
        active = false;

        Damageable dmg = GetComponent<Damageable>();
        bool dead = dmg != null && dmg.isDead;

        foreach (var mb in disabledBehaviours)
            if (mb != null) mb.enabled = true;
        disabledBehaviours.Clear();

        if (rb != null && !dead)
            rb.isKinematic = prevKinematic;

        ClearTint();
        Destroy(this);
    }

    void OnDestroy()
    {
        if (active)
            ClearTint();
    }
}
