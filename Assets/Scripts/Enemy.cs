using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private Transform targetTr;
    [SerializeField]
    private int wavepointIndex = 0;
    public bool isDead = false;

    public Canvas healthBarCanvas;
    public Slider healthBarSlider;

    public float speed = 10f;
    public int maxHealth = 100;
    public int currentHealth;

    public float waypointThreshold = 0.1f;

    public void Init()
    {
        wavepointIndex = 0;
        targetTr = Waypoints.Instance.Points[0];
        transform.position = new Vector3(0f, 2f, 0f);
    }

    private void Update()
    {
        //Vector3 dir = targetTr.position - transform.position;
        //transform.Translate(dir.normalized * speed * Time.deltaTime);

        MoveByPath(Time.deltaTime);
    }

    private void MoveByPath(float deltaTime)
    {
        float moveDist = deltaTime * speed;

        while (moveDist > 0 && wavepointIndex >= 0 && wavepointIndex < Waypoints.Instance.Points.Length)
        {
            Vector3 targetPos = Waypoints.Instance.Points[wavepointIndex].position;
            Vector3 moveDir = (targetPos - transform.position).normalized;
            float distToCurTarget = Vector3.Distance(transform.position, targetPos);

            if (distToCurTarget < waypointThreshold)
            {
                wavepointIndex++;
                if (wavepointIndex >= Waypoints.Instance.Points.Length)
                {
                    OnReachEndOfPath();
                    return;
                }
                continue;
            }

            float moveDistToCurTarget = Mathf.Min(moveDist, distToCurTarget);
            transform.Translate(moveDir * moveDistToCurTarget, Space.World);

            moveDist -= moveDistToCurTarget;
        }
    }

    private void OnReachEndOfPath()
    {
        WaveSpawner.Instance.EnemyDeathCount();
        PlayerManager.Instance.PlayerLife(1);
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, PoolObjectType.Enemy);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        healthBarSlider.value = (float)currentHealth / maxHealth;
    }

    public void Die()
    {
        GameObject effectObj = ObjectPoolManager.Instance.GetObjectFromPool(PoolObjectType.EnemyDeathEffect);
        EffectManager impactEffect = effectObj.GetComponent<EffectManager>();
        effectObj.transform.position = transform.position;
        effectObj.transform.rotation = transform.rotation;

        isDead = true;

        WaveSpawner.Instance.EnemyDeathCount();
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, PoolObjectType.Enemy);
        RewardManager.Instance.AddGold(10);
    }
}
