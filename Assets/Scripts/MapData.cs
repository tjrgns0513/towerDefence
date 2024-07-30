using System;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{
    None,
    Cube,
    StartPoint,
    EndPoint,
    Waypoint,
    EnemyRoad,
}

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
public class MapData : ScriptableObject
{
    //public List<List<StructureType>> mapDataList;

    public List<MapLayer> layers;

    public void SetMapData(MapData newMapData)
    {
        for (var i = 0; i < newMapData.layers.Count; ++i)
        {
            if (layers.Count <= i)
                continue;
            var layer  = layers[i];
            var newlayer = newMapData.layers[i];
            for (var j = 0; j < newlayer.structureLayers.Count; ++j)
            {
                if (layer.structureLayers.Count <= j)
                    continue; 
                layer.structureLayers[j] = newlayer.structureLayers[j];
            }
        }
    }

    public void SetMap(MapData mapData)
    {
        layers = mapData.Clone().layers;
    }

    public MapData Clone()
    {
        var newMapData = CreateInstance<MapData>();
        newMapData.layers = new List<MapLayer>();

        foreach (var layer in layers)
        {
            var newMapLayer = new MapLayer();
            newMapLayer.structureLayers = new List<StructureType>();
            newMapData.layers.Add(newMapLayer);
            foreach (var type in layer.structureLayers)
            {
                newMapLayer.structureLayers.Add(type);
            }
        }

        return newMapData;
    }


}

[Serializable]
public class MapLayer
{
    [SerializeField]
    public List<StructureType> structureLayers;
}
