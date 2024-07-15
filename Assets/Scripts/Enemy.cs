using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private Transform targetTr;
    private int wavepointIndex = 0;
    public bool isDead = false;

    public Canvas healthBarCanvas;
    public Slider healthBarSlider;

    public float speed = 10f;
    public int maxHealth = 100;
    public int currentHealth;

    public void Init()
    {
        wavepointIndex = 0;
        targetTr = Waypoints.Instance.Points[0];
        transform.position = new Vector3(0f, 2f, 0f);
    }

    private void Update()
    {
        Vector3 dir = targetTr.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime);

        Vector3 pos = transform.position;
        Vector3 targetPos = targetTr.position;
        MoveCheck(dir.normalized, Vector3.right, pos.x >= targetPos.x);
        MoveCheck(dir.normalized, Vector3.left, pos.x <= targetPos.x);
        MoveCheck(dir.normalized, Vector3.forward, pos.z >= targetPos.z);
        MoveCheck(dir.normalized, Vector3.back, pos.z <= targetPos.z);
    }

    //Enemy가 이동포인트에 도착했는지 체크
    void MoveCheck(Vector3 dir, Vector3 checkDir, bool check)
    {
        if (dir != checkDir) return;
        if (!check) return;

        transform.position = targetTr.position;
        GetNextWayPoint();
    }

    void GetNextWayPoint()
    {
        if (wavepointIndex >= Waypoints.Instance.Points.Length - 1)
        {
            WaveSpawner.Instance.EnemyDeathCount();
            PlayerManager.Instance.PlayerLife(1);
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, PoolObjectType.Enemy);
            return;
        }

        wavepointIndex++;
        targetTr = Waypoints.Instance.Points[wavepointIndex];
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
        if (healthBarSlider != null)
        {
            healthBarSlider.value = (float)currentHealth / maxHealth;
        }
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
