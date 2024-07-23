using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{
    None,
    Cube,
    StartPoint,
    EndPoint,
    Waypoint,
    EnemyLoad
}

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
public class MapData : ScriptableObject
{

    public List<List<StructureType>> MapDataList;

    public Vector3[] waypoints;
    public Vector3 startPoint;
    public Vector3 endPoint;
    public Vector3[] cubes;
    public Vector3[] enemyLoads;
}
