using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    public Transform enemyPrefab;
    public Transform spawnPoint;
    public float timeBetweenWaves = 5f;
    private float countdown = 2f;
    public Text waveCountdownText;
    private int waveNumber = 0;

    private void Update()
    {
        if(countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }

        countdown -= Time.deltaTime;
        
        waveCountdownText.text = Mathf.Round(countdown).ToString();
    }

    IEnumerator SpawnWave()
    {
        waveNumber++;

        for (int i = 0; i < waveNumber; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }

    }

    void SpawnEnemy()
    {
        var enemyObject = ObjectPoolManager.Instance.GetObjectFromPool("Enemy");

        Enemy enemy = enemyObject.GetComponent<Enemy>();

        enemy.wavepointIndex = 0;
        enemy.target = Waypoints.Instance.Points[enemy.wavepointIndex];
        enemy.transform.position = enemy.basicPosition;

        enemyObject.SetActive(true);
    }
}
