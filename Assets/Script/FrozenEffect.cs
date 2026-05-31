using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Temporarily "freezes" an enemy: halts its AI and physics and tints it ice-blue,
/// then restores everything when the duration runs out. Added at runtime by the
/// Ice spell (via Magic_Manager / MagicAttacks_Projectile) when its projectile hits.
/// Works with any enemy type by disabling whichever AI components are present.
/// </summary>
public class FrozenEffect : MonoBehaviour
{
    public static readonly Color IceTint = new Color(0.45f, 0.75f, 1f);

    private float timer;
    private bool active;

    private readonly List<MonoBehaviour> disabledBehaviours = new List<MonoBehaviour>();
    private readonly List<Material> tintedMaterials = new List<Material>();
    private readonly List<Color> originalEmission = new List<Color>();
    private Rigidbody rb;
    private bool prevKinematic;

    /// <summary>Freeze the target for the given duration. Re-hitting refreshes the timer.</summary>
    public static void Apply(GameObject target, float duration)
    {
        if (target == null || duration <= 0f) return;
        FrozenEffect fe = target.GetComponent<FrozenEffect>();
        if (fe == null) fe = target.AddComponent<FrozenEffect>();
        fe.Begin(duration);
    }

    void Begin(float duration)
    {
        // Refresh / extend the freeze if already frozen.
        timer = Mathf.Max(timer, duration);
        if (active) return;
        active = true;

        // Halt every AI controller this enemy has.
        DisableIfPresent(GetComponent<EnemyAI>());
        DisableIfPresent(GetComponent<RangedEnemyAI>());
        DisableIfPresent(GetComponent<ExplodingEnemyAI>());

        // Freeze physics so it stops in place.
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            prevKinematic = rb.isKinematic;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Ice-blue glow.
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            foreach (var m in r.materials)
            {
                if (m.HasProperty("_EmissionColor"))
                {
                    tintedMaterials.Add(m);
                    originalEmission.Add(m.GetColor("_EmissionColor"));
                    m.SetColor("_EmissionColor", IceTint);
                    m.EnableKeyword("_EMISSION");
                }
            }
        }
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

        // Re-enable AI (the AI scripts self-disable again if the enemy died meanwhile).
        foreach (var mb in disabledBehaviours)
            if (mb != null) mb.enabled = true;
        disabledBehaviours.Clear();

        // Restore physics unless the enemy died (its death sequence owns the body now).
        if (rb != null && !dead)
            rb.isKinematic = prevKinematic;

        // Restore emission colours.
        for (int i = 0; i < tintedMaterials.Count; i++)
            if (tintedMaterials[i] != null)
                tintedMaterials[i].SetColor("_EmissionColor", originalEmission[i]);
        tintedMaterials.Clear();
        originalEmission.Clear();

        Destroy(this);
    }
}
