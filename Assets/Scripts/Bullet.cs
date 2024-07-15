using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Enemy enemyTarget;

    [SerializeField]
    private float speed = 30f;
    [SerializeField]
    private int damage = 20;

    public void Init(Transform target)
    {
        SetTarget(target);
    }

    public void SetTarget(Transform target)
    {
        enemyTarget = target.GetComponent<Enemy>();
    }

    void Update()
    {
        //적이 죽었거나 null일때
        EnemyIsValid();
        Seek();
    }

    private void EnemyIsValid()
    {
        if (enemyTarget.isDead || enemyTarget.gameObject == null)
        {
            DisposeBullet();
            return;
        }
    }

    private void Seek()
    {
        Vector3 dir = enemyTarget.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(enemyTarget.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy hitEnemy = other.gameObject.GetComponent<Enemy>();
            if (hitEnemy != null && !hitEnemy.isDead)
            {
                HitTarget(hitEnemy);
            }
            else
            {
                DisposeBullet();
            }
        }
    }


    //총알이 타겟에 맞았을때
    void HitTarget(Enemy target)
    {
        //impactEffect 파티클위치에 총알위치를 받아서 실행
        GameObject effectObj = ObjectPoolManager.Instance.GetObjectFromPool(PoolObjectType.ImpactEffect);
        EffectManager impactEffect = effectObj.GetComponent<EffectManager>();
        effectObj.transform.position = transform.position;
        effectObj.transform.rotation = transform.rotation;

        if(!target.isDead)
        {
            target.TakeDamage(damage);
        }

        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, PoolObjectType.Bullet);
    }

    void DisposeBullet()
    {
        enemyTarget = null;
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, PoolObjectType.Bullet);
    }
}
