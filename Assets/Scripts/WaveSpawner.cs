using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    private static WaveSpawner instance = null;

    public Transform enemyPrefab;
    public Transform spawnPoint;

    public int timeBetweenWaves = 3;

    public Text waveCountdownText;
    public Text waveNumberText;

    private int waveNumber = 0;
    private int enemyAlive = 0;

    private bool isStart = true;

    public static WaveSpawner Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (isStart)
        {
            isStart = false;
            waveCountdownText.gameObject.SetActive(true);
            StartCoroutine(TimeCountDown());
        }
    }

    IEnumerator SpawnWave()
    {
        waveNumber++;
        waveNumberText.text = waveNumber.ToString();
        enemyAlive = waveNumber;

        for (int i = 0; i < waveNumber; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator TimeCountDown()
    {
        for(int i = timeBetweenWaves; i >= 0; i--)
        {
            waveCountdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        waveCountdownText.text = "Start !";

        yield return new WaitForSeconds(1f);

        waveCountdownText.gameObject.SetActive(false);

        StartCoroutine(SpawnWave());
    }


    void SpawnEnemy()
    {
        var enemyObject = ObjectPoolManager.Instance.GetObjectFromPool(PoolObjectType.Enemy);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        enemy.Init();
        enemy.currentHealth = enemy.maxHealth + waveNumber;
        enemy.speed = enemy.speed + waveNumber;
        enemy.UpdateHealthBar();
        enemyObject.SetActive(true);
    }

    public void EnemyDeathCount()
    {
        enemyAlive--;

        if (enemyAlive == 0)
        {
            isStart = true;
        }
    }
}
