using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;
    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogWarning("BossBullet missing Rigidbody2D component.", this);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // Try to apply damage to the player if they have a health controller
            var health = col.GetComponent<PlayerHealthController>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }

    void Start()
    {
        // schedule destruction after lifetime
        Destroy(gameObject, 5f);
    }
}