using UnityEngine;
using UnityEngine.UI;

// Shows a small health bar above an enemy's head.
// Created automatically by Damageable and generated entirely in code, so no prefab
// needs to be wired up in the editor beforehand.
// Note: the enemy itself may be scaled (e.g. 10x), so the bar's WORLD position and
//       WORLD scale are both overwritten in LateUpdate to avoid being distorted by
//       the parent's scale.
[DisallowMultipleComponent]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Appearance")]
    [Tooltip("Extra lift above the top of the enemy (world units)")]
    public float extraHeight = 0.4f;
    [Tooltip("Bar width and height (world units)")]
    public Vector2 worldSize = new Vector2(1.0f, 0.16f);
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    public Color highColor = new Color(0.25f, 0.85f, 0.25f, 1f);
    public Color lowColor = new Color(0.85f, 0.2f, 0.2f, 1f);

    [Tooltip("Hide the bar while the enemy is at full health")]
    public bool hideWhenFull = false;

    // Measured internally in pixels, then multiplied by 1/PixelsPerUnit to convert to
    // world units, which makes the border proportions easy to control.
    const float PixelsPerUnit = 100f;

    private Damageable damageable;
    private Transform barRoot;
    private Image fillImage;
    private CanvasGroup canvasGroup;
    private Transform cam;
    private float topOffset;   // World distance from the enemy pivot to its top (measured once in Build)
    private static Sprite whiteSprite;

    // Called by Damageable to attach and configure the bar
    public static EnemyHealthBar Attach(Damageable owner, float extraHeight)
    {
        if (owner == null) return null;
        EnemyHealthBar bar = owner.GetComponent<EnemyHealthBar>();
        if (bar == null) bar = owner.gameObject.AddComponent<EnemyHealthBar>();
        bar.extraHeight = extraHeight;
        bar.damageable = owner;
        bar.Build();
        return bar;
    }

    void Awake()
    {
        if (damageable == null) damageable = GetComponent<Damageable>();
    }

    void Start()
    {
        if (barRoot == null) Build();
    }

    void Build()
    {
        if (barRoot != null) return;
        if (damageable == null) damageable = GetComponent<Damageable>();

        EnsureSprite();
        MeasureTopOffset();

        // World-space Canvas (not parented to the enemy, to avoid being scaled; it follows manually instead)
        GameObject canvasGO = new GameObject(name + "_HealthBar");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        canvasRT.sizeDelta = worldSize * PixelsPerUnit;

        // Background
        Image bg = CreateImage("Background", canvasRT, backgroundColor);
        Stretch(bg.rectTransform);

        // Fill (red/green health)
        fillImage = CreateImage("Fill", canvasRT, highColor);
        RectTransform fillRT = fillImage.rectTransform;
        Stretch(fillRT);
        float pad = 0.06f * PixelsPerUnit; // border
        fillRT.offsetMin = new Vector2(pad, pad);
        fillRT.offsetMax = new Vector2(-pad, -pad);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        barRoot = canvasGO.transform;
        cam = Camera.main != null ? Camera.main.transform : null;

        UpdateTransform();
        Refresh();
    }

    void MeasureTopOffset()
    {
        topOffset = 1f; // fallback
        var renderers = GetComponentsInChildren<Renderer>();
        bool has = false;
        Bounds b = new Bounds(transform.position, Vector3.zero);
        foreach (var r in renderers)
        {
            // Skip things like particles that don't represent the body's silhouette
            if (r is ParticleSystemRenderer) continue;
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        if (has) topOffset = b.max.y - transform.position.y;
    }

    void LateUpdate()
    {
        if (barRoot == null || damageable == null) return;

        if (damageable.isDead)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            return;
        }

        Refresh();
        UpdateTransform();
    }

    void UpdateTransform()
    {
        // World position: top of the enemy plus a little extra height
        barRoot.position = transform.position + Vector3.up * (topOffset + extraHeight);

        // Fixed world scale (unaffected by the enemy's scale): 100px -> the world units given by worldSize
        float s = 1f / PixelsPerUnit;
        barRoot.localScale = new Vector3(s, s, s);

        // Always face the camera (billboard)
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (cam != null)
            barRoot.rotation = Quaternion.LookRotation(barRoot.position - cam.position, Vector3.up);
    }

    void Refresh()
    {
        float ratio = damageable.HealthRatio;

        if (fillImage != null)
        {
            fillImage.fillAmount = ratio;
            fillImage.color = Color.Lerp(lowColor, highColor, ratio);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = (hideWhenFull && ratio >= 0.999f) ? 0f : 1f;
    }

    void OnDestroy()
    {
        // The bar is not a child of the enemy, so destroy it manually when the enemy is destroyed
        if (barRoot != null) Destroy(barRoot.gameObject);
    }

    // --- helpers ---

    static void EnsureSprite()
    {
        if (whiteSprite != null) return;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        whiteSprite.name = "EnemyHealthBarWhite";
    }

    static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.sprite = whiteSprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}
