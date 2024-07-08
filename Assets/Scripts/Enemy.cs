using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 10f;

    private Transform target;
    private int wavepointIndex = 0;
    private Vector3 basicPosition;

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
        if(wavepointIndex >= Waypoints.points.Length - 1)
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
}
