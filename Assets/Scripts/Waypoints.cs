using System.Linq.Expressions;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Waypoints : MonoBehaviour
{
    private static Waypoints instance = null;
    public static Transform[] points { get; private set; }

    public static Waypoints Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Waypoints();
            }
            return instance;
        }
    }

    private void Awake()
    {
        points = new Transform[transform.childCount];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }
}
