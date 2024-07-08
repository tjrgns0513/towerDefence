using UnityEngine;

public class Enemy : MonoBehaviour
{
    private static Enemy instance = null;

    public float speed = 10f;
    private Transform target;
    public Vector3 basicPosition;
    private int wavepointIndex = 0;

    public static Enemy Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new Enemy();
            }
            return instance;
        }
    }

    private void Start()
    {
        target = Waypoints.points[0];
        basicPosition = transform.position;
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
        if (wavepointIndex >= Waypoints.points.Length - 1)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);

            //ObjectPoolManager.Instance.GetObjectFromPool();
            //wavepointIndex = 0;
            //target = Waypoints.points[wavepointIndex];
            //gameObject.transform.position = basicPosition;

            return;
        }

        wavepointIndex++;
        target = Waypoints.points[wavepointIndex];
    }

    public void SpawnEnemy()
    {
        ObjectPoolManager.Instance.GetObjectFromPool();
        wavepointIndex = 0;
        target = Waypoints.points[wavepointIndex];
        gameObject.transform.position = basicPosition;
    }
}
