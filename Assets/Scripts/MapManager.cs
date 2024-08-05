using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.VisualScripting;

[Serializable]
public class MapSize
{
    public int x, y;

    public MapSize(int x, int y)
    {
        this.x = x;
        this.y = y;
    }   
}


public class MapManager : MonoBehaviour
{
    private static MapManager instance = null;

    public List<MapData_ScriptableObject> mapManagerList = new List<MapData_ScriptableObject>(); // MapData 리스트로 맵 데이터 관리

    public Transform waypointParent;
    public Transform cubeParent;
    public Transform enemyRoadParent; // Enemy Load Parent 추가
    public Transform objectParent;

    public GameObject waypointPrefab;
    public GameObject cubePrefab;
    public GameObject enemyRoadPrefab;

    public List<Route> routes = new List<Route>();

    public MapSize mapSize = new MapSize(10, 10); // 맵 크기

    public float gridSize = 4.0f; // 그리드 크기
    public Color gridColor = Color.gray; // 그리드 색상

    private int wayPointIndex;
    private int groupIndex;

    public Vector2Int waypointIndex;

    public Transform[] Waypoints { get; private set; }
    public Transform[] Cubes { get; private set; }
    public Transform[] EnemyRoads { get; private set; } // Enemy Load 추가

    public MapData currentMapData;

    public static MapManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<MapManager>();
            return instance;
        }
    }

    public static event Action OnMapLoaded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;

    }

    // 맵 초기화 함수
    public void InitializeMap(MapData mapData)
    {
        if (mapData == null)
            return;

        mapData.verticalLines.Clear();

        for (int y = 0; y < mapSize.y; y++)
        {
            MapLine mapLayer = new MapLine();
            mapLayer.horizontalLines = new List<StructureType>();
            for (int x = 0; x < mapSize.x; x++)
            {
                mapLayer.horizontalLines.Add(StructureType.None);
            }

            mapData.verticalLines.Add(mapLayer);
        }
    }

    // 맵 데이터를 로드하는 함수
    public void LoadMapData(int mapIndex)
    {
        if (mapManagerList.Count <= mapIndex)
        {
            Debug.Log("로드 실패");
            return;
        }

        //처음 로드하는 맵이면 초기화하는 조건
        //if (currentMapData.verticalLines.Count == 0)
        //{
        //    ClearMap();
        //}

        MapDataObjectManager.Instance.Clear();

        //mapManagerList[mapIndex].mapData.SetMap(currentMapData);
        currentMapData = mapManagerList[mapIndex].mapData.Clone();

        //로드할때 오브젝트의 x,y좌표의 최대치만큼 맵을 보여주게 변경
        int mapsizeY = currentMapData.verticalLines.Count;
        int mapsizeX = currentMapData.verticalLines[0].horizontalLines.Count;

        mapSize = new MapSize(mapsizeX, mapsizeY);

        MapDataObjectManager.Instance.Init(mapSize.x, mapSize.y);

        for (var i = 0; i < currentMapData.verticalLines.Count; ++i)
        {
            var StructureTypes = currentMapData.verticalLines[i].horizontalLines;
            for (var j = 0; j < StructureTypes.Count; ++j)
            {
                var x = j;
                var y = i;
                var structureType = StructureTypes[j];

                if (structureType == StructureType.Cube)
                {
                    var position = CalcPosition(x, y);
                    AddObject(position, cubePrefab, cubeParent, StructureType.Cube);
                }
                else if (structureType == StructureType.EnemyRoad)
                {
                    var position = CalcPosition(x, y);
                    AddObject(position, enemyRoadPrefab, enemyRoadParent, StructureType.EnemyRoad);
                }
                //else if (structureType == StructureType.Waypoint)
                //{
                //    var position = CalcPosition(x, y);
                //    AddObject(position, waypointPrefab, waypointParent, StructureType.Waypoint);
                //}
            }
        }

        OnMapLoaded?.Invoke();
        Debug.Log($"{mapIndex} 번맵 로드 완료");
    }

    private Vector3 CalcPosition(int x, int y)
    {
        var indexX = x * gridSize + gridSize / 2;
        var indexY = y * gridSize + gridSize / 2;
        return new Vector3(indexX, 0, indexY);
    }

    //맵에 오브젝트 추가하는 함수
    public void AddObject(Vector3 position, GameObject prefabs, Transform parent, StructureType type, bool check = false)
    {
        if (currentMapData == null)
            return;

        if (!prefabs) {
            Debug.LogError("Prefab is not assigned in MapManager.");
            return; 
        }

        MapData mapData = currentMapData;

        int x = Mathf.FloorToInt(position.x / gridSize);
        int y = Mathf.FloorToInt(position.z / gridSize);

        if (y < 0 || mapData.verticalLines.Count <= y)
            return;
        if (x < 0 || mapData.verticalLines[y].horizontalLines.Count <= x)
            return;
        
        if(check && mapData.verticalLines[y].horizontalLines[x] == type)
        {
            return;
        }
           
        //덮어쓰기
        if (mapData.verticalLines[y].horizontalLines[x] != type)
        {
            if(mapData.verticalLines[y].horizontalLines[x] != StructureType.None && MapDataObjectManager.Instance.objects[y][x] != null)
            {
                Undo.DestroyObjectImmediate(MapDataObjectManager.Instance.objects[y][x]);
            }           
        }

        //오브젝트 생성
        GameObject obj = Instantiate(prefabs, position, Quaternion.identity, parent);
        Undo.RegisterCreatedObjectUndo(obj, "Create Object");
        obj.name = ($"{prefabs.name}");
        mapData.verticalLines[y].horizontalLines[x] = type;
        MapDataObjectManager.Instance.objects[y][x] = obj;

    }

    public void AddWayPoint(Vector2Int waypoint, Vector3 position, int waypointIndex, int groupIndex, GameObject prefabs, Transform parent, bool check = false)
    {

        MapData mapData = currentMapData;

        int x = Mathf.FloorToInt(position.x / gridSize);
        int y = Mathf.FloorToInt(position.z / gridSize);

        if (routes.Count <= groupIndex)
        {
            Route route = new Route();
            routes.Add(route);
        }

        //if (routes[groupIndex].waypointIndex != null)
        //{
        //    if (routes[groupIndex].waypointIndex[wayPointIndex] != null)
        //    {
        //        Undo.DestroyObjectImmediate(MapDataObjectManager.Instance.objects[y][x]);
        //        MapDataObjectManager.Instance.objects[y][x] = null;

        //        return;
        //    }
        //}

        Debug.Log("wayPointIndex : " + wayPointIndex);

        if (routes[groupIndex].waypointIndex.Count > wayPointIndex)
        {
            var xy = routes[groupIndex].waypointIndex[wayPointIndex];
            Undo.DestroyObjectImmediate(MapDataObjectManager.Instance.objects[xy.y][xy.x]);
            // MapDataObjectManager.Instance.objects[y][x] = null;
        }


        if (routes[groupIndex].waypointIndex.Contains(waypoint))
        {
            return;
        }
        else
        {
            routes[groupIndex].waypointIndex.Add(waypoint);
        }

        //if (routes[groupIndex].waypointIndex[waypointIndex] == routes[groupIndex].waypointIndex)
        //{
        //    Undo.DestroyObjectImmediate(MapDataObjectManager.Instance.objects[y][x]);
        //    MapDataObjectManager.Instance.objects[y][x] = null;

        //    return;
        //}

        for (int i = 0; i < routes[groupIndex].waypointIndex.Count; i++)
        {
            Debug.Log("routes : " + routes[groupIndex].waypointIndex[i]);
        }

        //오브젝트 생성
        GameObject obj = Instantiate(prefabs, position, Quaternion.identity, parent);
        Undo.RegisterCreatedObjectUndo(obj, "Create Object");
        obj.name = ($"{prefabs.name}");
        MapDataObjectManager.Instance.objects[y][x] = obj;
    }

    public void DeleteObject(Vector3 position, bool isCreated = false)
    {
        if (currentMapData == null)
            return;

        MapData mapData = currentMapData;

        int x = Mathf.FloorToInt(position.x / gridSize);
        int y = Mathf.FloorToInt(position.z / gridSize);
        if (y < 0 || mapData.verticalLines.Count <= y)
            return;
        if (x < 0 || mapData.verticalLines[y].horizontalLines.Count <= x)
            return;
        if (mapData.verticalLines[y].horizontalLines[x] == StructureType.None && !isCreated)
            return;

        if (mapData.verticalLines[y].horizontalLines[x] != StructureType.None && MapDataObjectManager.Instance.objects[y][x] != null)
        {
            Undo.DestroyObjectImmediate(MapDataObjectManager.Instance.objects[y][x]);
        }

        mapData.verticalLines[y].horizontalLines[x] = StructureType.None;
        MapDataObjectManager.Instance.objects[y][x] = null;
    }

    // 기존 오브젝트를 제거하는 함수
    public void ClearMap()
    {
        InitializeMap(currentMapData);
        ClearData();

        routes.Clear();
    }

    public void ClearData()
    {
        MapDataObjectManager.Instance.Clear();
        MapDataObjectManager.Instance.Init(mapSize.x, mapSize.y);
    }

    // 맵 데이터를 저장하는 함수
    public void SaveMapData(int mapIndex)
    {
        if (mapManagerList.Count <= mapIndex)
        {
            Debug.Log("저장 실패");
            return;
        }

        mapManagerList[mapIndex].mapData.SetMap(currentMapData);

        LoadMapData(mapIndex);
        Debug.Log($"{mapManagerList[mapIndex]} 번맵 저장 완료");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(mapManagerList[mapIndex]);
        AssetDatabase.SaveAssets();
#endif
    }

    //기존 맵을 저장해두고 x,y사이즈 초기화한후 다시 넣어준다
    internal void MapRefresh()
    {
        var newMap = new MapData();
        newMap.verticalLines = new List<MapLine>();
        InitializeMap(newMap);
        ClearData();
        newMap.SetMapData(currentMapData);
        currentMapData = newMap;
    }

    public void SetWayPointIndex(int index)
    {
        wayPointIndex = index;
    }

    public int GetWayPointIndex()
    {
        return wayPointIndex;
    }

    public void GetGroupIndex(int index)
    {
        groupIndex = index;
    }

    //중복되는 웨이포인트 삭제
    //public void FindDuplicateWayPoint(int index)
    //{
    //    for (int y = 0; y < currentMapData.layers.Count; y++)
    //    {
    //        for (int x = 0; x < currentMapData.layers[y].waypointIndexs.Count; x++)
    //        {
    //            if (currentMapData.layers[y].waypointIndexs[x] == index)
    //            {
    //                currentMapData.layers[y].waypointIndexs[x] = -1;
    //                currentMapData.layers[y].structureLayers[x] = StructureType.None;
    //                Undo.DestroyObjectImmediate(MapDataObjectManager.Instance.objects[y][x]);
    //                MapDataObjectManager.Instance.objects[y][x] = null;
    //            }
    //        }
    //    }
    //}

    //리로드 할때 초기화
    public void OnCompilationFinished(object obj)
    { 
        ClearData();
        ClearMap();
    }
}
