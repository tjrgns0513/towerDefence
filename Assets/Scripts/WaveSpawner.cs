using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    private static WaveSpawner instance = null;

    public EnemyType[] enemyTypes; // 스크립터블 오브젝트 배열
    public Transform spawnPoint;

    public int timeBetweenWaves = 3;

    public UnityEngine.UI.Text waveCountdownText;
    public UnityEngine.UI.Text waveNumberText;

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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        MapManager.OnMapLoaded += SetSpawnPoint; // 맵 로드 완료 이벤트 구독
    }

    private void OnDisable()
    {
        MapManager.OnMapLoaded -= SetSpawnPoint; // 맵 로드 완료 이벤트 구독 해제
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
        for (int i = timeBetweenWaves; i >= 0; i--)
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
        // 랜덤으로 적 타입 선택 (또는 다른 로직 사용 가능)
        int randomIndex = Random.Range(0, enemyTypes.Length);
        EnemyType selectedEnemyType = enemyTypes[randomIndex];

        var enemyObject = ObjectPoolManager.Instance.GetObjectFromPool(selectedEnemyType.enemyName);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        enemy.Init(selectedEnemyType);
        enemy.transform.position = spawnPoint.position;
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

    private void SetSpawnPoint()
    {
        //spawnPoint = MapManager.Instance.startPoint;
    }
}
