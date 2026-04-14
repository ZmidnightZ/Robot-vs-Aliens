using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 10;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Public API used by other scripts
    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"Player healed {amount}. HP: {currentHealth}/{maxHealth}");
    }

    void Die()
    {
        Debug.Log("Player died.");
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        // If an enemy collider touches player -> small damage
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            return;
        }

        // If the collider is a boss projectile, prefer calling its damage value.
        // BossBullet is declared in the global namespace, so reference the type directly.
        var bossBullet = other.GetComponent<BossBullet>();
        if (bossBullet != null)
        {
            TakeDamage(bossBullet.damage);
            return;
        }

        var genericBullet = other.GetComponentInParent<RifleBullet>(); // example fallback
        if (genericBullet != null)
        {
            TakeDamage(genericBullet.damage);
        }
    }
}
