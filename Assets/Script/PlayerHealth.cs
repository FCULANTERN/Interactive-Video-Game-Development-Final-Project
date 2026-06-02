using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private bool isDead = false;

    public bool IsDead => isDead;

    [Header("Game Over")]
    public GameObject gameOverCanvas;

    [SerializeField] private SceneLoader sceneLoader;

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.TakeDamage(damage);

            if (HealthSystem.Instance.hitPoint <= 0)
                Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (HealthSystem.Instance != null)
            HealthSystem.Instance.Regenerate = false;

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.RegisterPlayerDeath();

        gameObject.SetActive(false);

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);
        else
            sceneLoader.LoadScene("HomeScene");
    }
}