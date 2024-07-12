using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    public enum PoolObjectType
    {
        Enemy,
        Bullet,
        ImpactEffect,
        EnemyDeathEffect,
    }

    [SerializeField] private GameObject enemyPrefab;
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

    private Dictionary<PoolObjectType, ObjectPool> objectPools = new Dictionary<PoolObjectType, ObjectPool>();

    private int enemyIDCounter = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // 프리팹과 부모를 Dictionary에 매핑하여 풀 초기화
        InitializePool(PoolObjectType.Enemy, enemyPrefab, enemyParent);
        InitializePool(PoolObjectType.Bullet, bulletPrefab, bulletsParent);
        InitializePool(PoolObjectType.ImpactEffect, impactEffectPrefab, impactEffectsParent);
        InitializePool(PoolObjectType.EnemyDeathEffect, enemyDeathEffectPrefab, enemyDeathEffectsParent);
    }

    private void InitializePool(PoolObjectType objType, GameObject prefab, Transform parent)
    {
        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(parent);
            objectPool.Enqueue(obj);
        }

        objectPools[objType] = new ObjectPool
        {
            Pool = objectPool,
            Prefab = prefab,
            Parent = parent
        };
    }

    public GameObject GetObjectFromPool(PoolObjectType objType)
    {
        if (!objectPools.ContainsKey(objType))
        {
            Debug.LogWarning($"Object type {objType} not found in pool");
            return null;
        }

        ObjectPool selectedPool = objectPools[objType];
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

        // 적 오브젝트에 새로운 ID 부여
        if (objType == PoolObjectType.Enemy)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetID(enemyIDCounter++);
                enemy.isDead = false;
            }
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnObjectToPool(GameObject obj, PoolObjectType objType)
    {
        if (!objectPools.ContainsKey(objType))
        {
            Debug.LogWarning($"Object type {objType} not found in pool");
            return;
        }

        obj.SetActive(false);
        objectPools[objType].Pool.Enqueue(obj);
    }
}
