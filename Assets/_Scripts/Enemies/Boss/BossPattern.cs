using System.Collections;
using UnityEngine;

public abstract class BossPattern
{
    public abstract IEnumerator Execute(BossController boss);
}

public class RadialPattern : BossPattern
{
    public override IEnumerator Execute(BossController boss)
    {
        int count = 12;

        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = i * (360f / count);
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;

                if (boss.bulletPrefab == null || boss.firePoint == null)
                    continue;

                GameObject bullet = GameObject.Instantiate(
                    boss.bulletPrefab,
                    boss.firePoint.position,
                    Quaternion.identity
                );

                var bb = bullet.GetComponent<BossBullet>();
                if (bb != null)
                    bb.SetDirection(dir);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}

public class SpiralPattern : BossPattern
{
    public override IEnumerator Execute(BossController boss)
    {
        float angle = 0f;

        for (int i = 0; i < 40; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;

            if (boss.bulletPrefab == null || boss.firePoint == null)
                yield return new WaitForSeconds(0.05f);
            else
            {
                GameObject bullet = GameObject.Instantiate(
                    boss.bulletPrefab,
                    boss.firePoint.position,
                    Quaternion.identity
                );

                var bb = bullet.GetComponent<BossBullet>();
                if (bb != null)
                    bb.SetDirection(dir);

                yield return new WaitForSeconds(0.05f);
            }

            angle += 15f;
        }
    }
}

public class DashPattern : BossPattern
{
    public override IEnumerator Execute(BossController boss)
    {
        if (boss == null || boss.player == null)
            yield break;

        Vector2 dir = (boss.player.position - boss.transform.position).normalized;

        // SHOW WARNING FIRST
        yield return boss.StartCoroutine(boss.ShowDashTelegraph(dir));

        // THEN DASH
        float speed = 10f;
        float time = 0.5f;
        float t = 0f;

        while (t < time)
        {   
            boss.transform.position += (Vector3)dir * speed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
    }
}

public class SummonPattern : BossPattern
{
    public override IEnumerator Execute(BossController boss)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 pos = (Vector2)boss.transform.position + Random.insideUnitCircle * 2f;

            if (boss.minionPrefab != null)
                GameObject.Instantiate(boss.minionPrefab, pos, Quaternion.identity);

            yield return new WaitForSeconds(0.3f);
        }
    }
}

public class BurstDashSummonPattern : BossPattern
{
    public override IEnumerator Execute(BossController boss)
    {
        if (boss == null)
            yield break;

        // Phase 1: small radial burst
        int burstCount = 8;
        for (int wave = 0; wave < 2; wave++)
        {
            for (int i = 0; i < burstCount; i++)
            {
                float angle = i * (360f / burstCount) + wave * 10f; // slight offset per wave
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;

                if (boss.bulletPrefab != null && boss.firePoint != null)
                {
                    GameObject bullet = GameObject.Instantiate(
                        boss.bulletPrefab,
                        boss.firePoint.position,
                        Quaternion.identity
                    );

                    var bb = bullet.GetComponent<BossBullet>();
                    if (bb != null)
                        bb.SetDirection(dir);
                }
            }

            yield return new WaitForSeconds(0.35f);
        }

        // Phase 2: use same telegraph + dash behavior as DashPattern, while firing a short spiral during the dash
        if (boss.player != null)
        {
            Vector2 dashDir = (boss.player.position - boss.transform.position).normalized;

            // match DashPattern: telegraph then dash with same speed/time
            yield return boss.StartCoroutine(boss.ShowDashTelegraph(dashDir));

            float dashDuration = 0.5f; // same as DashPattern
            float dashSpeed = 10f;     // same as DashPattern
            float t = 0f;

            float shootInterval = 0.08f;
            float shootTimer = 0f;
            float spiralAngle = 0f;

            while (t < dashDuration)
            {
                // move boss
                boss.transform.position += (Vector3)dashDir * dashSpeed * Time.deltaTime;

                // shoot spiral bullets at intervals
                shootTimer += Time.deltaTime;
                if (shootTimer >= shootInterval)
                {
                    shootTimer = 0f;
                    spiralAngle += 20f;

                    if (boss.bulletPrefab != null && boss.firePoint != null)
                    {
                        Vector2 dir = Quaternion.Euler(0, 0, spiralAngle) * Vector2.right;

                        GameObject bullet = GameObject.Instantiate(
                            boss.bulletPrefab,
                            boss.firePoint.position,
                            Quaternion.identity
                        );

                        var bb = bullet.GetComponent<BossBullet>();
                        if (bb != null)
                            bb.SetDirection(dir);
                    }
                }

                t += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // slight pause if no player to dash to
            yield return new WaitForSeconds(0.3f);
        }

        // Phase 3: summon a few minions around boss
        int summons = 3;
        for (int i = 0; i < summons; i++)
        {
            Vector2 pos = (Vector2)boss.transform.position + Random.insideUnitCircle * 1.8f;

            if (boss.minionPrefab != null)
                GameObject.Instantiate(boss.minionPrefab, pos, Quaternion.identity);

            yield return new WaitForSeconds(0.25f);
        }
    }
}