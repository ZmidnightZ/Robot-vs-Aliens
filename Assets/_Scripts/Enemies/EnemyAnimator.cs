using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator am;
    private Transform player;
    private SpriteRenderer sr;

    private void Start()
    {
        am = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Control animation (up / down)
        am.SetFloat("Y", direction.y);

        Flip(direction);
    }

    private void Flip(Vector2 direction)
    {
        if (Mathf.Approximately(direction.x, 0f))
        {
            return;
        }

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}