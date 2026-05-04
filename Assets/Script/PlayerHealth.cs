using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Game Over")]
    public GameObject gameOverCanvas;

    [SerializeField] private SceneLoader sceneLoader;

    void Start()
    {
        currentHealth = maxHealth;
        // 同步最大血量到 UI 系統
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.maxHitPoint = maxHealth;
            HealthSystem.Instance.hitPoint = maxHealth;
            HealthSystem.Instance.HealDamage(0); // 使用HealDamage(0)來強制更新一次UI
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("player health: " + currentHealth);

        // 通知 UI 系統扣血
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.TakeDamage(damage);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("player died");
        // 確保 UI 血量也歸零
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.TakeDamage(HealthSystem.Instance.hitPoint);
        }

        // 停用玩家並顯示 GameOver 畫面
        gameObject.SetActive(false);

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }
        else
        {
            // 若沒有設定 GameOver Canvas，則退回原本的跳場景邏輯
            sceneLoader.LoadScene("HomeScene");
        }
    }
}