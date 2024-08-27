using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

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

    public List<MapData_ScriptableObject> mapManagerList; // MapData 리스트로 맵 데이터 관리

    public MapEditorIndexStorage editorIndex;

    public List<Route> routes => currentMapData.routes;

    public Transform waypointParent;
    public Transform objectParent;

    public GameObject waypointPrefab;
    public GameObject cubePrefab;
    public GameObject enemyRoadPrefab;

    public MapSize mapSize = new MapSize(10, 10); // 맵 크기

    public float gridSize = 4.0f; // 그리드 크기
    public Color gridColor = Color.gray; // 그리드 색상

    public Transform[] Waypoints { get; private set; }
    public Transform[] Cubes { get; private set; }
    public Transform[] EnemyRoads { get; private set; }

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

        mapData.routes?.Clear();

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

        if (objectParent.childCount > 0)
        {
            for(var i = objectParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(objectParent.GetChild(i).gameObject);
            }
        }

        if (waypointParent.childCount > 0)
        {
            for (var i = waypointParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(waypointParent.GetChild(i).gameObject);
            }
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

        currentMapData = mapManagerList[mapIndex].mapData.Clone();

        //로드할때 오브젝트의 x,y좌표의 최대치만큼 맵을 보여주게 변경
        int mapsizeY = currentMapData.verticalLines.Count;
        int mapsizeX = currentMapData.verticalLines[0].horizontalLines.Count;

        mapSize = new MapSize(mapsizeX, mapsizeY);

        DrawMap();
        OnMapLoaded?.Invoke();
        Debug.Log($"{mapIndex} 번맵 로드 완료");
    }

    public void DrawMap()
    {
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
                    AddObject(position, cubePrefab, objectParent, StructureType.Cube);
                }
                else if (structureType == StructureType.EnemyRoad)
                {
                    var position = CalcPosition(x, y);
                    AddObject(position, enemyRoadPrefab, objectParent, StructureType.EnemyRoad);
                }
            }
        }

        if (currentMapData.routes.Count <= 0)
        {
            editorIndex.wayPointIndex = 0;
        }

        for (var i = 0; i < currentMapData.routes.Count; ++i)
        {
            var route = currentMapData.routes[i];

            int count = route.waypointCoordinates.Count;

            editorIndex.groupIndex = i;
            editorIndex.wayPointIndex = 0;

            for (var j = 0; j < count; ++j)
            {
                var waypoint = route.waypointCoordinates[j];
                var position = CalcPosition(waypoint.x, waypoint.y);
                AddWayPoint(waypoint, position, waypointPrefab, waypointParent);
            }
        }
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
        if (y < 0 || mapData.verticalLines.Count <= y ||
            x < 0 || mapData.verticalLines[y].horizontalLines.Count <= x)
            return;

        if (check && mapData.verticalLines[y].horizontalLines[x] == type)
        {
            return;
        }

        //오브젝트가 같은 타입이 아닐경우 덮어쓰기
        if (mapData.verticalLines[y].horizontalLines[x] != type && mapData.verticalLines[y].horizontalLines[x] != StructureType.None)
        {
            mapData.verticalLines[y].horizontalLines[x] = type;
            OverWrite();
            return;
        }

        //오브젝트 생성
        GameObject obj = Instantiate(prefabs, position, Quaternion.identity, parent);
        obj.name = ($"{prefabs.name}");
        mapData.verticalLines[y].horizontalLines[x] = type;

    }

    public void AddWayPoint(Vector2Int waypoint, Vector3 position, GameObject prefabs, Transform parent, bool check = false)
    {
        MapData mapData = currentMapData;

        int x = Mathf.FloorToInt(position.x / gridSize);
        int y = Mathf.FloorToInt(position.z / gridSize);

        //설치하려는 위치에 큐브가 있다면 return
        if (mapData.verticalLines[y].horizontalLines[x] == StructureType.Cube)
            return;

        EnsureRoutesSize();

        if (GetWayPointCount(editorIndex.groupIndex) > editorIndex.wayPointIndex)
        {
            if (routes[editorIndex.groupIndex].waypointCoordinates[editorIndex.wayPointIndex] != null)
            {
                editorIndex.wayPointIndex++;
                position.y = 1f;
                GameObject obj1 = Instantiate(waypointPrefab, position, Quaternion.identity, waypointParent);
                obj1.name = ($"{waypointPrefab.name}");
                return;
            }
        }

        //대각선 설치 안되게 확인
        if (DiagonalBlock(position) == false)
        {
            routes[editorIndex.groupIndex].waypointCoordinates.Add(waypoint);
            editorIndex.wayPointIndex++;
            position.y = 1f;
            GameObject obj = Instantiate(prefabs, position, Quaternion.identity, parent);
            obj.name = ($"{prefabs.name}");
        }
    }

    private bool DiagonalBlock(Vector3 position)
    {
        bool isBlock = false;

        var waypointPos = routes[editorIndex.groupIndex].waypointCoordinates;

        if (waypointPos.Count > 0)
        {
            var x = Mathf.FloorToInt(position.x / gridSize);
            var y = Mathf.FloorToInt(position.z / gridSize);

            Vector2Int pos = waypointPos[editorIndex.wayPointIndex - 1];
            Vector2Int pos2 = new Vector2Int(x, y);

            if (pos.x == pos2.x || pos.y == pos2.y)
                isBlock = false;
            else
            {
                isBlock = true;
                Debug.LogError("Waypoints cannot be installed diagonally");
            }
                
        }

        return isBlock;
    }

    public int GetWayPointCount(int groupIdx)
    {
        return routes[groupIdx].GetWayPointCount();
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

        if (waypointParent.childCount > 0)
        {
            for (var i = waypointParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(waypointParent.GetChild(i).gameObject);
            }
        }

        var wayPointCopyList = new Vector2Int(x, y);

        for(var i = 0; i < routes[editorIndex.groupIndex].waypointCoordinates.Count; i++)
        {
            if(routes[editorIndex.groupIndex].waypointCoordinates[i] == wayPointCopyList)
            {
                routes[editorIndex.groupIndex].waypointCoordinates.Remove
            (routes[editorIndex.groupIndex].waypointCoordinates[i]);

                editorIndex.wayPointIndex--;
            }
        }

        for (var groupIdx = 0; groupIdx < routes.Count; groupIdx++)
        {
            for (var i = 0; i < routes[groupIdx].waypointCoordinates.Count; i++)
            {
                var wayX = Mathf.FloorToInt(routes[groupIdx].waypointCoordinates[i].x);
                var wayY = Mathf.FloorToInt(routes[groupIdx].waypointCoordinates[i].y);

                var ResetPosition = CalcPosition(wayX, wayY);

                GameObject obj = Instantiate(waypointPrefab, ResetPosition, Quaternion.identity, waypointParent);

                obj.name = ($"{waypointPrefab.name}");
            }
        }

        if (objectParent.childCount > 0)
        {
            for (var i = objectParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(objectParent.GetChild(i).gameObject);
            }
        }

        mapData.verticalLines[y].horizontalLines[x] = StructureType.None;

        for (var i = 0; i < mapData.verticalLines.Count; i++)
        {
            for (var j = 0; j < mapData.verticalLines[i].horizontalLines.Count; j++)
            {
                if (mapData.verticalLines[i].horizontalLines[j] == StructureType.Cube)
                {
                    var ResetPosition = CalcPosition(j, i);
                    GameObject obj = Instantiate(cubePrefab, ResetPosition, Quaternion.identity, objectParent);
                    obj.name = ($"{cubePrefab.name}");
                }
                else if (mapData.verticalLines[i].horizontalLines[j] == StructureType.EnemyRoad)
                {
                    var ResetPosition = CalcPosition(j, i);
                    GameObject obj = Instantiate(enemyRoadPrefab, ResetPosition, Quaternion.identity, objectParent);
                    obj.name = ($"{enemyRoadPrefab.name}");
                }
            }
        }
    }

    //오브젝트 덮어쓰기
    public void OverWrite()
    {
        for (var i = objectParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(objectParent.GetChild(i).gameObject);
        }

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
                    var position1 = CalcPosition(x, y);
                    AddObject(position1, cubePrefab, objectParent, StructureType.Cube);
                }
                else if (structureType == StructureType.EnemyRoad)
                {
                    var position1 = CalcPosition(x, y);
                    AddObject(position1, enemyRoadPrefab, objectParent, StructureType.EnemyRoad);
                }
            }
        }
    }

    // 기존 오브젝트를 제거하는 함수
    public void ClearMap()
    {
        InitializeMap(currentMapData);
        editorIndex.wayPointIndex = 0;
        editorIndex.groupIndex = 0;
        //routes.Clear();
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

        //LoadMapData(mapIndex);
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
        newMap.SetMapData(currentMapData, routes);
        currentMapData = newMap;
        DrawMap();
    }

    public void SetWayPointIndex(int index)
    {
        editorIndex.wayPointIndex = index;
    }

    public void GetGroupIndex(int index)
    {
        editorIndex.groupIndex = index;
    }

    public void SetRouteCount()
    {
        EnsureRoutesSize();

        editorIndex.wayPointIndex = routes[editorIndex.groupIndex].waypointCoordinates.Count;
    }

    public void EnsureRoutesSize()
    {
        if (routes.Count <= editorIndex.groupIndex)
        {
            Route route = new Route();
            routes.Add(route);
        }
    }

    //리로드 할때 초기화
    public void OnCompilationFinished(object obj)
    { 
        ClearMap();
    }
}
