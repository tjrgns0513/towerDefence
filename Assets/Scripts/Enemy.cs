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
    public EnemyType enemyType;

    public void Init(EnemyType type)
    {
        wavepointIndex = 0;
        enemyType = type;
        speed = enemyType.speed;
        maxHealth = enemyType.maxHealth;
        currentHealth = maxHealth;

        if (MapManager.Instance != null && MapManager.Instance.waypointParent.childCount > 0)
        {
            targetTr = MapManager.Instance.waypointParent.GetChild(0);

            //targetTr = MapManager.Instance.Waypoints[0];
        }
    }

    private void Update()
    {
        MoveByPath(Time.deltaTime);
    }

    //적 이동경로
    private void MoveByPath(float deltaTime)
    {
        float moveDist = deltaTime * speed;

        while (moveDist > 0 && wavepointIndex >= 0 && wavepointIndex < MapManager.Instance.waypointParent.childCount)
        {
            if (MapManager.Instance.waypointParent.GetChild(wavepointIndex) == null)
            {
                // 웨이포인트가 유효하지 않으면 경로 이동을 멈춤
                Debug.Log("wavepointIndex : " + wavepointIndex);
                ReachEndOfPath();
                return;
            }

            Vector3 targetPos = MapManager.Instance.waypointParent.GetChild(wavepointIndex).position;
            Vector3 moveDir = (targetPos - transform.position).normalized;
            float distToCurTarget = Vector3.Distance(transform.position, targetPos);

            if (distToCurTarget < waypointThreshold)
            {
                wavepointIndex++;
                if (wavepointIndex >= MapManager.Instance.waypointParent.childCount)
                {
                    ReachEndOfPath();
                    return;
                }
                continue;
            }

            float moveDistToCurTarget = Mathf.Min(moveDist, distToCurTarget);
            transform.Translate(moveDir * moveDistToCurTarget, Space.World);

            moveDist -= moveDistToCurTarget;
        }
    }

    private void ReachEndOfPath()
    {
        WaveSpawner.Instance.EnemyDeathCount();
        PlayerManager.Instance.PlayerLife(1);
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, enemyType.enemyName);
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
        GameObject effectObj = ObjectPoolManager.Instance.GetObjectFromPool("EnemyDeathEffect");
        EffectManager impactEffect = effectObj.GetComponent<EffectManager>();
        effectObj.transform.position = transform.position;
        effectObj.transform.rotation = transform.rotation;

        isDead = true;

        WaveSpawner.Instance.EnemyDeathCount();
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, enemyType.enemyName);
        RewardManager.Instance.AddGold(enemyType.rewardGold);
    }
}
