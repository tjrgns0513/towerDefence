using UnityEngine;

public class Enemy : MonoBehaviour
{


    public float speed = 10f;
    private Transform target;       //이동할 타켓 위치
    public Vector3 basicPosition;   //기본 시작 위치
    private int wavepointIndex = 0;



    private void Start()
    {
        target = Waypoints.Instance.points[0];
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
        if (wavepointIndex >= Waypoints.Instance.points.Length - 1)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);

            //ObjectPoolManager.Instance.GetObjectFromPool();
            //wavepointIndex = 0;
            //target = Waypoints.points[wavepointIndex];
            //gameObject.transform.position = basicPosition;

            return;
        }

        wavepointIndex++;
        target = Waypoints.Instance.points[wavepointIndex];
    }

    public static void SpawnEnemy()
    {
        ObjectPoolManager.Instance.GetObjectFromPool();
        
        //wavepointIndex = 0;
        //target = Waypoints.Instance.points[wavepointIndex];
        //gameObject.transform.position = basicPosition;
    }
}
