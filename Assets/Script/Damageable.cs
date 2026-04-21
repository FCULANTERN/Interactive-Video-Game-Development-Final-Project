using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " ¨ü¨ì¶Ë®`¡A³Ñ¾l¦å¶q: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}