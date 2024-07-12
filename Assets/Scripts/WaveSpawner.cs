using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static ObjectPoolManager;

public class WaveSpawner : MonoBehaviour
{
    private static WaveSpawner instance = null;

    public Transform enemyPrefab;
    public Transform spawnPoint;

    public float timeBetweenWaves = 5f;

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
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator TimeCountDown()
    {
        for(int i = 5; i >= 0; i--)
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
        var enemyObject = ObjectPoolManager.Instance.GetObjectFromPool(ObjectPoolManager.PoolObjectType.Enemy);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        enemy.wavepointIndex = 0;
        enemy.target = Waypoints.Instance.Points[enemy.wavepointIndex];
        enemy.transform.position = enemy.basicPosition;
        enemy.speed = waveNumber + 20;

        enemyObject.SetActive(true);
    }

    public void EnemyDeathCount()
    {
        enemyAlive--;

        if (enemyAlive == 0)
        {
            isStart = true;
        }

        Debug.Log("Enemy died. Enemies left: " + enemyAlive);
    }
}
