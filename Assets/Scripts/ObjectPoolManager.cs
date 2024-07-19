using System.Collections.Generic;
using UnityEngine;
public enum PoolObjectType
{
    Bullet,
    ImpactEffect,
    EnemyDeathEffect,
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private EnemyType[] enemyTypes;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private GameObject enemyDeathEffectPrefab;
    [SerializeField] private int poolSize = 10;

    public Transform enemyParent;
    public Transform bulletsParent;
    public Transform impactEffectsParent;
    public Transform enemyDeathEffectsParent;

    private struct ObjectPool
    {
        public Queue<GameObject> Pool { get; set; }
        public GameObject Prefab { get; set; }
        public Transform Parent { get; set; }
    }

    private Dictionary<string, ObjectPool> objectPools = new Dictionary<string, ObjectPool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // 적 프리팹 풀 초기화
        foreach (var enemyType in enemyTypes)
        {
            InitializePool(enemyType.enemyName, enemyType.prefab, enemyParent);
        }

        // 프리팹과 부모를 Dictionary에 매핑하여 풀 초기화
        InitializePool("Bullet", bulletPrefab, bulletsParent);
        InitializePool("ImpactEffect", impactEffectPrefab, impactEffectsParent);
        InitializePool("EnemyDeathEffect", enemyDeathEffectPrefab, enemyDeathEffectsParent);
    }

    private void InitializePool(string objName, GameObject prefab, Transform parent)
    {
        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(parent);
            objectPool.Enqueue(obj);
        }

        objectPools[objName] = new ObjectPool
        {
            Pool = objectPool,
            Prefab = prefab,
            Parent = parent
        };
    }

    public GameObject GetObjectFromPool(string objName)
    {
        if (!objectPools.ContainsKey(objName))
        {
            Debug.LogWarning($"Object type {objName} not found in pool");
            return null;
        }

        ObjectPool selectedPool = objectPools[objName];
        GameObject obj;

        if (selectedPool.Pool.Count > 0)
        {
            obj = selectedPool.Pool.Dequeue();
        }
        else
        {
            // Prefab을 찾아서 새로 인스턴스화
            obj = Instantiate(selectedPool.Prefab);
            obj.transform.SetParent(selectedPool.Parent);
        }

        // 적 오브젝트 초기화
        if (objName.Contains("Enemy"))
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.isDead = false;
            }
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnObjectToPool(GameObject obj, string objName)
    {
        if (!objectPools.ContainsKey(objName))
        {
            Debug.LogWarning($"Object type {objName} not found in pool");
            return;
        }

        obj.SetActive(false);
        objectPools[objName].Pool.Enqueue(obj);
    }
}
