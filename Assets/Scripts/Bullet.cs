using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    Enemy enemyTarget;
    public float speed = 70f;
    private int enemyIndex = -1;

    private void Start()
    {
        enemyTarget = target.GetComponent<Enemy>();
    }

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        if (target == null || target.gameObject == null)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, "Bullet");
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        GameObject effectIns = ObjectPoolManager.Instance.GetObjectFromPool("ImpactEffect");
        BulletImpactEffect impactEffect = effectIns.GetComponent<BulletImpactEffect>();

        impactEffect.HitBulletImpactEffect();

        effectIns.transform.position = transform.position;
        effectIns.transform.rotation = transform.rotation;

        if (target != null)
        {
            Debug.Log("Index : " + enemyIndex + "  ID : " + enemyTarget.ID);
            if (enemyIndex == enemyTarget.ID)
            {
                ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, "Bullet");
                return;
            }

            enemyIndex = enemyTarget.ID;

            ObjectPoolManager.Instance.ReturnObjectToPool(target.gameObject, "Enemy");
        }

        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, "Bullet");
    }
}
