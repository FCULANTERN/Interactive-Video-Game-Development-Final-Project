using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    public int maxHealth = 10;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("玩家受到傷害，剩餘血量: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("玩家死亡");
        // 先簡單關閉玩家，之後可改成播放死亡動畫
        gameObject.SetActive(false);
    }
}