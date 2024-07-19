using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
public class MapData : ScriptableObject
{
    public Vector3[] waypoints;
    public Vector3 startPoint;
    public Vector3 endPoint;
    public Vector3[] cubes;
    public Vector3[] enemyLoads;
}
