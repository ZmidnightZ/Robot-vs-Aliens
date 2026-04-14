using UnityEngine;

public class RifleBullet : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public float lifeTime = 3f;

    // Use this to correct your sprite's default forward direction.
    // If the bullet sprite points right (+X) in the texture, leave 0.
    // If it points up (+Y) set to -90, etc.
    public float rotationOffset = 0f;

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
            return;
        }

        direction = dir.normalized;

        // Calculate angle in degrees and apply offset so the "head" points to the target.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Use world space so rotation doesn't change movement direction.
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            other.GetComponent<BossController>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
