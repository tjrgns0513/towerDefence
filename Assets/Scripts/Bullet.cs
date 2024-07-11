using System.Collections;
using UnityEngine;
using static ObjectPoolManager;
using static UnityEngine.GraphicsBuffer;

public class Bullet : MonoBehaviour
{
    private Transform targetTr;
    Enemy enemyTarget;
    public float speed = 70f;
    private int enemyId = -1;

    private void Start()
    {
        enemyTarget = targetTr.GetComponent<Enemy>();
    }

    public void Seek(Transform _target)
    {
        targetTr = _target;
    }

    void Update()
    {
        if (targetTr == null || targetTr.gameObject == null)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolObjectType.Bullet);
            return;
        }

        Vector3 dir = targetTr.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    //총알이 타겟에 맞았을때
    void HitTarget()
    {
        //impactEffect 파티클위치에 총알위치를 받아서 실행
        GameObject effectObj = ObjectPoolManager.Instance.GetObjectFromPool(ObjectPoolManager.PoolObjectType.ImpactEffect);
        BulletImpactEffect impactEffect = effectObj.GetComponent<BulletImpactEffect>();
        effectObj.transform.position = transform.position;
        effectObj.transform.rotation = transform.rotation;

        if (targetTr != null)
        {
            Debug.Log("111111");
            enemyTarget.TakeDamage();
        }

        //if (enemyId == enemyTarget.ID)
        //{
        //    ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, PoolObjectType.Bullet);
        //    return;
        //}

        //enemyId = enemyTarget.ID;
        //enemyTarget.Die();
 
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolObjectType.Bullet);
    }
}
