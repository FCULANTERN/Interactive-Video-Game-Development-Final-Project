//==============================================================
// HealthSystem
// HealthSystem.Instance.TakeDamage (float Damage);
// HealthSystem.Instance.HealDamage (float Heal);
// HealthSystem.Instance.UseMana (float Mana);
// HealthSystem.Instance.RestoreMana (float Mana);
// Attach to the Hero.
//==============================================================

using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
	public static HealthSystem Instance;

	public Slider healthSlider;
	public Text healthText;
	public float hitPoint = 100f;
	public float maxHitPoint = 100f;

	public Slider manaSlider;
	public Text manaText;
	public float manaPoint = 100f;
	public float maxManaPoint = 100f;

	//==============================================================
	// Regenerate Health & Mana
	//==============================================================
	public bool Regenerate = true;
	public float regen = 0.1f;
	private float timeleft = 0.0f;	// Left time for current interval
	public float regenUpdateInterval = 1f;

	// 升級系統加成（由 PlayerUpgradable 設定）
	[HideInInspector] public float hpRegenBonus = 0f;
	[HideInInspector] public float manaRegenBonus = 0f;

	public bool GodMode;

    //==============================================================
    // Awake
    //==============================================================
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    //==============================================================
    // Start
    //==============================================================
    void Start()
	{
		UpdateGraphics();
		timeleft = regenUpdateInterval;
	}

	//==============================================================
	// Update
	//==============================================================
	void Update ()
	{
		if (Regenerate)
			Regen();
	}

	//==============================================================
	// Regenerate Health & Mana
	//==============================================================
	private void Regen()
	{
		timeleft -= Time.deltaTime;

		if (timeleft <= 0.0) // Interval ended - update health & mana and start new interval
		{
			// Debug mode
			if (GodMode)
			{
				HealDamage(maxHitPoint);
				RestoreMana(maxManaPoint);
			}
			else
			{
				HealDamage(regen + hpRegenBonus);
				RestoreMana(regen + manaRegenBonus);
			}

			UpdateGraphics();

			timeleft = regenUpdateInterval;
		}
	}

	//==============================================================
	// Health Logic
	//==============================================================
	private void UpdateHealthBar()
	{
		float ratio = hitPoint / maxHitPoint;
		if (healthSlider != null)
			healthSlider.value = ratio;
		if (healthText != null)
			healthText.text = hitPoint.ToString("0") + "/" + maxHitPoint.ToString("0");
	}

	public void TakeDamage(float Damage)
	{
		hitPoint -= Damage;
        hitPoint = Mathf.Clamp(hitPoint, 0, maxHitPoint);

        UpdateGraphics();

		StartCoroutine(PlayerHurts());
	}

	public void HealDamage(float Heal)
	{
		hitPoint += Heal;
		if (hitPoint > maxHitPoint)
			hitPoint = maxHitPoint;

		UpdateGraphics();
	}

	public void SetMaxHealth(float max)
	{
		maxHitPoint += (int)(maxHitPoint * max / 100);

		UpdateGraphics();
	}

	//==============================================================
	// Mana Logic
	//==============================================================
	private void UpdateManaBar()
	{
		float ratio = manaPoint / maxManaPoint;
		if (manaSlider != null)
			manaSlider.value = ratio;
		if (manaText != null)
			manaText.text = manaPoint.ToString("0") + "/" + maxManaPoint.ToString("0");
	}

    public bool UseMana(float mana)
    {
        if (manaPoint < mana)
            return false;

        manaPoint -= mana;
        manaPoint = Mathf.Clamp(manaPoint, 0, maxManaPoint);

        UpdateGraphics();
        return true;
    }

    public void RestoreMana(float Mana)
	{
		manaPoint += Mana;
        manaPoint = Mathf.Clamp(manaPoint, 0, maxManaPoint);

        UpdateGraphics();
	}

	public void SetMaxMana(float max)
	{
		maxManaPoint += (int)(maxManaPoint * max / 100);

		UpdateGraphics();
	}

	//==============================================================
	// Update all Bars UI graphics
	//==============================================================
	private void UpdateGraphics()
	{
		UpdateHealthBar();
		UpdateManaBar();
	}

	//==============================================================
	// Coroutine Player Hurts
	//==============================================================
	IEnumerator PlayerHurts()
	{
		// Player gets hurt. Do stuff.. play anim, sound..

		if (PopupText.Instance != null)
			PopupText.Instance.Popup("Ouch!", 1f, 1f);

		if (hitPoint < 1) // Health is Zero!!
		{
			yield return StartCoroutine(PlayerDied()); // Hero is Dead
		}

		else
			yield return null;
	}

	//==============================================================
	// Hero is dead
	//==============================================================
	IEnumerator PlayerDied()
	{
		// Player is dead. Do stuff.. play anim, sound..
		if (PopupText.Instance != null)
			PopupText.Instance.Popup("You have died!", 1f, 1f);

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.RegisterPlayerDeath();

        yield return null;
	}
}
