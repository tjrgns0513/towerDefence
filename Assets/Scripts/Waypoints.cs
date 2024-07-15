using UnityEngine;

public class Waypoints : MonoBehaviour
{
    private static Waypoints instance = null;
    public Transform[] Points { get; private set; }

    public static Waypoints Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;


        Points = new Transform[transform.childCount];

        for (int i = 0; i < Points.Length; i++)
        {
            Points[i] = transform.GetChild(i);
        }
    }
}
