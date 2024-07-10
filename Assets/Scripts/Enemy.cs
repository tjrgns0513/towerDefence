using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 10f;
    public Transform target;       //이동할 타켓 위치
    public Vector3 basicPosition;   //기본 시작 위치
    public int wavepointIndex = 0;
    public int ID { get; private set; }

    private void Start()
    {
        target = Waypoints.Instance.Points[0];
        basicPosition = new Vector3(0f, 2f, 0f);
        transform.position = basicPosition;
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
        if (wavepointIndex >= Waypoints.Instance.Points.Length - 1)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, "Enemy");
            return;
        }

        wavepointIndex++;
        target = Waypoints.Instance.Points[wavepointIndex];
    }

    public void SetID(int id)
    {
        ID = id;
    }
}
