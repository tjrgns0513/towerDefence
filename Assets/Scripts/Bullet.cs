using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Bullet : MonoBehaviour
{
    private Enemy enemyTarget;
    public Transform targetTr;
    public float speed = 30f;
    public int damage = 20;

    public void Init(Transform target)
    {
        SetTarget(target);
    }

    public void SetTarget(Transform target)
    {
        targetTr = target;
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
        Vector3 dir = targetTr.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(targetTr);
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
        GameObject effectObj = ObjectPoolManager.Instance.GetObjectFromPool("ImpactEffect");
        EffectManager impactEffect = effectObj.GetComponent<EffectManager>();
        effectObj.transform.position = transform.position;
        effectObj.transform.rotation = transform.rotation;

        if(!target.isDead)
        {
            target.TakeDamage(damage);
        }

        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, "Bullet");
    }

    void DisposeBullet()
    {
        enemyTarget = null;
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, "Bullet");
    }
}
