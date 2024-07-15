using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform targetTr;

    private float range = 15f;
    private float fireRate = 1f;
    private float fireCountdown = 0f;
    private float turnSpeed = 10f;
    private string enemyTag = "Enemy";

    public Transform partToRotate;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public void Init()
    {
        fireCountdown = 1f / fireRate;
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistnace = Mathf.Infinity;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if(distanceToEnemy < shortestDistnace)
            {
                shortestDistnace = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistnace <= range)
        {
            targetTr = nearestEnemy.transform;
        }
        else
        {
            targetTr = null;
        }
    }

    void Update()
    {
        if (targetTr == null)
            return;

        LockOnTarget();

        if(fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void LockOnTarget()
    {
        Vector3 dir = targetTr.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void Shoot()
    {
        GameObject bulletGO = ObjectPoolManager.Instance.GetObjectFromPool(PoolObjectType.Bullet);
        bulletGO.transform.position = firePoint.position;
        bulletGO.transform.rotation = firePoint.rotation;
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        bullet.Init(targetTr);

        if (bullet != null)
        {
            bullet.SetTarget(targetTr);
        }
    }

    //공격범위
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
