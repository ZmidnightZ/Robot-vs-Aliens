using UnityEngine;

public class RifleWeapon : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 0.3f;
    public float range = 10f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            Transform target = FindNearestEnemy();

            if (target != null)
            {
                Shoot(target);
            }

            timer = 0;
        }
    }

    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float closestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);

            if (dist < closestDist && dist <= range)
            {
                closestDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    void Shoot(Transform target)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        Vector2 dir = (target.position - transform.position).normalized;

        bullet.GetComponent<RifleBullet>().SetDirection(dir);
    }
}
