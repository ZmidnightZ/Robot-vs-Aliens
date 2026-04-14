using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 5f;

    public float separationRadius = 0.25f;
    public float separationForce = 1f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        // Move directly toward the player (no obstacle avoidance)
        Vector2 direction = ((Vector2)(player.position - transform.position)).normalized;

        // Separation from nearby enemies
        Vector2 separation = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll((Vector2)transform.position, separationRadius);
        foreach (var col in nearby)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                separation += (Vector2)(transform.position - col.transform.position);
            }
        }

        Vector2 finalDir = (direction + separation * separationForce);
        if (finalDir.sqrMagnitude > 0f)
        {
            finalDir = finalDir.normalized;
            transform.position += (Vector3)(finalDir * speed * Time.deltaTime);
        }
    }
}
