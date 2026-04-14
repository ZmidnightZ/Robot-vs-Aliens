using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public Transform player;

    [Header("Stats")]
    public float maxHealth = 10000;
    private float currentHealth;

    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject minionPrefab;
    public Transform firePoint;

    [Header("Pattern")]
    public float delayBetweenPatterns = 1.5f;

    [Header("Telegraph")]
    public LineRenderer lineRenderer;
    public float telegraphTime = 1f;

    private bool isAttacking = false;
    private int phase = 1;

    private List<BossPattern> phase1;
    private List<BossPattern> phase2;
    private List<BossPattern> phase3;

    // Prevent taking damage during the intro delay
    private bool canTakeDamage = false;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player object not found with tag 'Player'. Boss will not move toward player.");

        currentHealth = maxHealth;

        phase1 = new List<BossPattern>()
        {
            new RadialPattern(),
            new BurstDashSummonPattern()
        };

        phase2 = new List<BossPattern>()
        {
            new RadialPattern(),
            new SpiralPattern(),
            new SummonPattern(),
            new BurstDashSummonPattern()
        };

        phase3 = new List<BossPattern>()
        {
            new SpiralPattern(),
            new DashPattern(),
            new BurstDashSummonPattern()
        };

        canTakeDamage = false;
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        Debug.Log("Boss will start the fight in 5 seconds...");
        yield return new WaitForSeconds(5f);

        canTakeDamage = true;
        Debug.Log("Boss fight started. Boss is now vulnerable.");

        StartCoroutine(PatternLoop());
    }

    IEnumerator PatternLoop()
    {
        while (true)
        {
            if (!isAttacking)
            {
                isAttacking = true;

                var list = GetCurrentPhaseList();
                if (list == null || list.Count == 0)
                {
                    Debug.LogWarning("No patterns available for current phase.");
                }
                else
                {
                    BossPattern pattern = GetPattern();
                    Debug.Log($"Boss uses pattern: {pattern.GetType().Name}");
                    yield return StartCoroutine(pattern.Execute(this));
                }

                yield return new WaitForSeconds(delayBetweenPatterns);
                isAttacking = false;
            }

            yield return null;
        }
    }

    public IEnumerator ShowDashTelegraph(Vector2 direction)
    {
        if (lineRenderer == null || firePoint == null)
            yield break;

        lineRenderer.enabled = true;
        // ensure positions count and world space so positions match Transform world coords
        if (lineRenderer.positionCount < 2)
            lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        float t = 0f;
        while (t < telegraphTime)
        {
            Vector3 start = firePoint.position;
            Vector3 end = start + (Vector3)direction * 5f;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            float width = Mathf.Lerp(0.25f, 1.25f, t / telegraphTime);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            t += Time.deltaTime;
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    BossPattern GetPattern()
    {
        List<BossPattern> list = GetCurrentPhaseList();
        return list[Random.Range(0, list.Count)];
    }

    List<BossPattern> GetCurrentPhaseList()
    {
        if (phase == 1) return phase1;
        if (phase == 2) return phase2;
        return phase3;
    }

    void Update()
    {
        CheckPhase();
        MoveToPlayer();
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(100);
        }
    }

    void MoveToPlayer()
    {
        if (player == null)
            return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * 0.5f * Time.deltaTime;
    }

    void CheckPhase()
    {
        float hp = currentHealth / maxHealth;

        if (hp <= 0.75f && phase == 1)
            phase = 2;

        if (hp <= 0.5f && phase == 2)
            phase = 3;
    }

    public void TakeDamage(float dmg)
    {
        if (!canTakeDamage)
        {
            Debug.Log("Boss is invulnerable during delayed start.");
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - dmg);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public float GetHPPercent()
    {
        return Mathf.Clamp01(currentHealth / maxHealth);
    }
}