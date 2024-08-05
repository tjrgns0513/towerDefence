using System;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{
    None,
    Cube,
    EnemyRoad,
}

[Serializable]
public class MapData
{
    public List<MapLine> verticalLines;
    public List<Vector2Int> waypoints;

    public void SetMapData(MapData newMapData)
    {
        for (var i = 0; i < newMapData.verticalLines.Count; ++i)
        {
            if (verticalLines.Count <= i)
                continue;
            var layer = verticalLines[i];
            var newlayer = newMapData.verticalLines[i];
            for (var j = 0; j < newlayer.horizontalLines.Count; ++j)
            {
                if (layer.horizontalLines.Count <= j)
                    continue;
                layer.horizontalLines[j] = newlayer.horizontalLines[j];
            }
        }
    }

    public void SetMap(MapData mapData)
    {
        verticalLines = mapData.Clone().verticalLines;
    }

    public MapData Clone()
    {
        var newMapData = new MapData();
        newMapData.verticalLines = new List<MapLine>();

        foreach (var layer in verticalLines)
        {
            var newMapLayer = new MapLine();
            newMapLayer.horizontalLines = new List<StructureType>();
            newMapData.verticalLines.Add(newMapLayer);
            foreach (var type in layer.horizontalLines)
            {
                newMapLayer.horizontalLines.Add(type);
            }
        }

        return newMapData;
    }
}


[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
public class MapData_ScriptableObject : ScriptableObject
{
    public MapData mapData;
}

[Serializable]
public class MapLine
{
    public List<StructureType> horizontalLines;
}

public class Route
{
    public List<Vector2Int> waypointIndex = new List<Vector2Int>();
}
