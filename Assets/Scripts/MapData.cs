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
    //public List<Vector2Int> waypoints;
    public List<Route> routes;


    public void SetMapData(MapData newMapData, List<Route> routes)
    {
        //for (var i = 0; i < newMapData.verticalLines.Count; ++i)
        //{
        //    if (verticalLines.Count <= i)
        //        continue;
        //    var layer = verticalLines[i];
        //    var newlayer = newMapData.verticalLines[i];
        //    for (var j = 0; j < newlayer.horizontalLines.Count; ++j)
        //    {
        //        if (layer.horizontalLines.Count <= j)
        //            continue;
        //        layer.horizontalLines[j] = newlayer.horizontalLines[j];
        //    }
        //}

        for (var i = 0; i < MapManager.Instance.mapSize.y; ++i)
        {
            if (verticalLines.Count <= i || newMapData.verticalLines.Count <= i)
                continue;
            var layer = verticalLines[i];
            var newlayer = newMapData.verticalLines[i];
            for (var j = 0; j < MapManager.Instance.mapSize.x; ++j)
            {
                if (layer.horizontalLines.Count <= j || newlayer.horizontalLines.Count <= j)
                    continue;
                layer.horizontalLines[j] = newlayer.horizontalLines[j];
            }
        }

        var newRoutes = new List<Route>();
        foreach (var route in routes)
        {
            var newRoute = new Route();
            route.waypointCoordinates.ForEach(w =>
            {
                newRoute.waypointCoordinates.Add(w);
            });
            newRoutes.Add(newRoute);
        }
        this.routes = newRoutes;
    }

    public void SetMap(MapData mapData)
    {
        var clone = mapData.Clone();
        verticalLines = clone.verticalLines;
        routes = clone.routes;
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

        var newRoutes = new List<Route>();
        foreach (var route in this.routes)
        {
            var newRoute = new Route();
            route.waypointCoordinates.ForEach(w =>
            {
                newRoute.waypointCoordinates.Add(w);
            });
            newRoutes.Add(newRoute);
        }
        newMapData.routes = newRoutes;

        return newMapData;
    }


}

[Serializable]
public class MapLine
{
    public List<StructureType> horizontalLines;
}

[Serializable]
public class Route
{
    public List<Vector2Int> waypointCoordinates = new List<Vector2Int>();

    public int GetWayPointCount()
    { 
        return waypointCoordinates.Count; 
    }
}