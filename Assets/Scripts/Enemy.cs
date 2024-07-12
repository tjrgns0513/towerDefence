using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float speed = 10f;
    public Transform target;       //이동할 타켓 위치
    public Vector3 basicPosition;   //기본 시작 위치
    public int wavepointIndex = 0;
    public int ID { get; private set; }
    public bool isDead = false;

    public GameObject deathEffect;

    public int maxHealth = 100;
    public int currentHealth;
    public Canvas healthBarCanvas;
    public Slider healthBarSlider;

    private void Start()
    {
        target = Waypoints.Instance.Points[0];
        basicPosition = new Vector3(0f, 2f, 0f);
        transform.position = basicPosition;
        currentHealth = maxHealth;
 
        UpdateHealthBar();
    }

    private void Update()
    {
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWayPoint();
        }
    }

    void GetNextWayPoint()
    {
        if (wavepointIndex >= Waypoints.Instance.Points.Length - 1)
        {
            WaveSpawner.Instance.EnemyDeathCount();
            PlayerManager.Instance.PlayerLife(1);
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolObjectType.Enemy);
            return;
        }

        wavepointIndex++;
        target = Waypoints.Instance.Points[wavepointIndex];
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        Debug.Log("currentHealth : " + currentHealth);

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
        GameObject effectObj = ObjectPoolManager.Instance.GetObjectFromPool(ObjectPoolManager.PoolObjectType.EnemyDeathEffect);
        EffectManager impactEffect = effectObj.GetComponent<EffectManager>();
        effectObj.transform.position = transform.position;
        effectObj.transform.rotation = transform.rotation;

        isDead = true;

        WaveSpawner.Instance.EnemyDeathCount();
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolObjectType.Enemy);
        RewardManager.Instance.AddGold(10);
    }

    public void SetID(int id)
    {
        ID = id;
    }
}
