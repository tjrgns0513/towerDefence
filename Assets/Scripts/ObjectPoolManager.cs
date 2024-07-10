using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private int poolSize = 10;

    public Transform enemyParent;
    public Transform bulletsParent;
    public Transform impactEffectsParent;

    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private Queue<GameObject> impactEffectPool = new Queue<GameObject>();

    private int enemyIDCounter = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab);
            enemyObj.SetActive(false);
            enemyObj.transform.SetParent(enemyParent);
            enemyPool.Enqueue(enemyObj);

            GameObject bulletObj = Instantiate(bulletPrefab);
            bulletObj.SetActive(false);
            bulletObj.transform.SetParent(bulletsParent);
            bulletPool.Enqueue(bulletObj);

            GameObject impactEffectObj = Instantiate(impactEffectPrefab);
            impactEffectObj.SetActive(false);
            impactEffectObj.transform.SetParent(impactEffectsParent);
            impactEffectPool.Enqueue(impactEffectObj);
        }
    }

    public GameObject GetObjectFromPool(string objType)
    {
        Queue<GameObject> selectedPool = null;
        GameObject prefab = null;
        Transform parent = null;

        switch (objType)
        {
            case "Enemy":
                selectedPool = enemyPool;
                prefab = enemyPrefab;
                parent = enemyParent;
                break;
            case "Bullet":
                selectedPool = bulletPool;
                prefab = bulletPrefab;
                parent = bulletsParent;
                break;
            case "ImpactEffect":
                selectedPool = impactEffectPool;
                prefab = impactEffectPrefab;
                parent = impactEffectsParent;
                break;
        }

        GameObject obj;

        if (selectedPool.Count > 0)
        {
            obj = selectedPool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
            obj.transform.SetParent(parent);
        }

        if (objType == "Enemy")
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetID(enemyIDCounter++);
            }
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnObjectToPool(GameObject obj, string objType)
    {
        obj.SetActive(false);
        Queue<GameObject> selectedPool = null;

        switch (objType)
        {
            case "Enemy":
                selectedPool = enemyPool;
                break;
            case "Bullet":
                selectedPool = bulletPool;
                break;
            case "ImpactEffect":
                selectedPool = impactEffectPool;
                break;
        }

        selectedPool.Enqueue(obj);
    }
}
