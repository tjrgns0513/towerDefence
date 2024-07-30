using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using static UnityEditor.Experimental.GraphView.GraphView;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

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

    public List<MapData> mapManagerList = new List<MapData>(); // MapData 리스트로 맵 데이터 관리
    //public int currentMapIndex = 0; // 현재 맵 인덱스
    public Transform startPoint;
    public Transform endPoint;
    public Transform waypointParent;
    public Transform cubeParent;
    public Transform enemyRoadParent; // Enemy Load Parent 추가
    public Transform startPointParent;
    public Transform endPointParent;
    public Transform objectParent;

    public GameObject waypointPrefab;
    public GameObject cubePrefab;
    public GameObject startPointPrefab;
    public GameObject endPointPrefab;
    public GameObject enemyRoadPrefab;

    //public Vector2 mapSize = new Vector2(100, 100); // 맵 크기
    public MapSize mapSize = new MapSize(10, 10); // 맵 크기

    public float gridSize = 4.0f; // 그리드 크기
    public Color gridColor = Color.gray; // 그리드 색상

    public Transform[] Waypoints { get; private set; }
    public Transform[] Cubes { get; private set; }
    public Transform[] EnemyLoads { get; private set; } // Enemy Load 추가

    private MapData currentMapData;

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
        DontDestroyOnLoad(this.gameObject);
        //InitializeMap();
    }

    private void Start()
    {
        //LoadMapData(currentMapIndex);
    }

    // 맵 초기화 함수
    public void InitializeMap(MapData mapData)
    {
        if (!mapData)
            return;

        mapData.layers.Clear();

        for (int y = 0; y < mapSize.y; y++)
        {
            MapLayer mapLayer = new MapLayer();
            mapLayer.structureLayers = new List<StructureType>();

            for (int x = 0; x < mapSize.x; x++)
            {
                mapLayer.structureLayers.Add(StructureType.None);
            }

            mapData.layers.Add(mapLayer);
        }

        //MapDataObjectManager.Instance.Init(mapSize.x, mapSize.y);
    }

    // 맵 데이터를 로드하는 함수
    public void LoadMapData(int mapIndex)
    {
        if (mapManagerList.Count <= mapIndex)
        {
            Debug.Log("로드 실패");
            return;
        }

        //ClearMap();
        MapDataObjectManager.Instance.Clear();
        currentMapData = mapManagerList[mapIndex].Clone();


        //로드할때 오브젝트의 x,y좌표의 최대치만큼 맵을 보여주게 변경
        int mapSizeY = currentMapData.layers.Count;
        int mapsizeX = currentMapData.layers[0].structureLayers.Count;

        mapSize = new MapSize(mapsizeX, mapSizeY);

        MapDataObjectManager.Instance.Init(mapSize.x, mapSize.y);

        for (var i = 0; i < currentMapData.layers.Count; ++i)
        {
            var StructureTypes = currentMapData.layers[i].structureLayers;
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
                else if (structureType == StructureType.Waypoint)
                {
                    var position = CalcPosition(x, y);
                    AddObject(position, waypointPrefab, waypointParent, StructureType.Waypoint);
                }
                else if (structureType == StructureType.EnemyRoad)
                {
                    var position = CalcPosition(x, y);
                    AddObject(position, enemyRoadPrefab, enemyRoadParent, StructureType.EnemyRoad);
                }
                else if (structureType == StructureType.StartPoint)
                {
                    var position = CalcPosition(x, y);
                    AddObject(position, startPointPrefab, null, StructureType.StartPoint);
                }
                else if (structureType == StructureType.EndPoint)
                {
                    var position = CalcPosition(x, y);
                    AddObject(position, enemyRoadPrefab, enemyRoadParent, StructureType.EnemyRoad);
                }
                else
                {
                    structureType = StructureType.None;
                }
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
    public void AddObject(Vector3 position, GameObject prefabs, Transform parent, StructureType type, bool isCreated = false)
    {
        if (!currentMapData)
            return;

        if (!prefabs) {
            Debug.LogError("Prefab is not assigned in MapManager.");
            return; 
        }

        MapData mapData = currentMapData;

        int x = Mathf.FloorToInt(position.x / gridSize);
        int y = Mathf.FloorToInt(position.z / gridSize);
        if (y < 0 || mapData.layers.Count <= y)
            return;
        if (x < 0 || mapData.layers[y].structureLayers.Count <= x)
            return;
        if (mapData.layers[y].structureLayers[x] == type && isCreated)
            return;

        if (mapData.layers[y].structureLayers[x] != type)
        {
            if(mapData.layers[y].structureLayers[x] != StructureType.None && MapDataObjectManager.Instance.objects[y][x] != null)
            {
                Undo.DestroyObjectImmediate(MapDataObjectManager.Instance.objects[y][x]);
            }           
        }

        GameObject obj = Instantiate(prefabs, position, Quaternion.identity, objectParent);
        Undo.RegisterCreatedObjectUndo(obj, "Create Object");
        obj.name = ($"{prefabs.name}");
        mapData.layers[y].structureLayers[x] = type;

        MapDataObjectManager.Instance.objects[y][x] = obj;

        //GameObject mapObj = MapDataObjectManager.Instance.objects[y][x];

        //mapObj = Instantiate(prefabs, position, Quaternion.identity, objectParent);
        //Undo.RegisterCreatedObjectUndo(mapObj, "Create Object");
        //mapObj.name = ($"{prefabs.name}");// + objectParent.childCount);
        //mapData.layers[y].structureLayers[x] = type;
    }

    // 기존 오브젝트를 제거하는 함수
    public void ClearMap()
    {
        MapDataObjectManager.Instance.Clear();
        InitializeMap(currentMapData);
    }

    // 맵 데이터를 저장하는 함수
    public void SaveMapData(int mapIndex)
    {
        if (mapManagerList.Count <= mapIndex)
        {
            Debug.Log("저장 실패");
            return;
        }


        mapManagerList[mapIndex].SetMap(currentMapData);

        LoadMapData(mapIndex);
        Debug.Log($"{mapManagerList[mapIndex]} 번맵 저장 완료");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(mapManagerList[mapIndex]);
        AssetDatabase.SaveAssets();
#endif
        //}
    }

    //기존 맵을 저장해두고 x,y사이즈 초기화한후 다시 넣어준다
    internal void MapRefresh()
    {
        var newMap = ScriptableObject.CreateInstance<MapData>();
        newMap.layers = new List<MapLayer>();

        //MapDataObjectManager.Instance.CloneInit();
        
        
        InitializeMap(newMap);
        newMap.SetMapData(currentMapData);


        //GetMapObjectData();
        
        
        currentMapData = newMap;


        //


        //var newMap = ScriptableObject.CreateInstance<MapData>();
        //newMap.layers = new List<MapLayer>();
        //
        //MapDataObjectManager.Instance.Init(mapSize.x,mapSize.y);
        //MapDataObjectManager.Instance.CopyObjects(mapSize.x, mapSize.y);


        //MapDataObjectManager.Instance.CloneInit();
        //InitializeMap(newMap);
        //newMap.SetMapData(currentMapData);
        //GetMapObjectData();
        //currentMapData = newMap;
    }

    public void GetMapObjectData()
    {
        for (var y = 0; y < MapDataObjectManager.Instance.objects.Count; y++)
        {
            for (var x = 0; x < MapDataObjectManager.Instance.objects[y].Count; x++)
            {
                if (currentMapData.layers[y].structureLayers[x] == StructureType.Cube)
                {
                    var pos = MapDataObjectManager.Instance.objectsClone[y][x].transform.position;


                    AddObject(pos, MapDataObjectManager.Instance.objectsClone[y][x].gameObject, objectParent, StructureType.Cube);

                    

                    //GameObject obj = Instantiate(cubePrefab, pos, Quaternion.identity, objectParent);
                    //Undo.RegisterCreatedObjectUndo(obj, "Create Object");
                    //obj.name = ($"{cubePrefab.name}");
                    //currentMapData.layers[y].structureLayers[x] = StructureType.Cube;
                    //MapDataObjectManager.Instance.objects[y][x] = obj;
                }
                else
                {
                    var pos = MapDataObjectManager.Instance.objectsClone[y][x].transform.position;


                    AddObject(pos, MapDataObjectManager.Instance.objectsClone[y][x].gameObject, objectParent, StructureType.None);
                }
            }
        }
    }
}
