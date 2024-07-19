using UnityEngine;
using System;

public class MapManager : MonoBehaviour
{
    private static MapManager instance = null;

    public MapData[] mapDataArray; // MapData 배열로 맵 데이터 관리
    public int currentMapIndex = 0; // 현재 맵 인덱스
    public Transform startPoint;
    public Transform endPoint;
    public Transform waypointParent;
    public Transform cubeParent;
    public Transform enemyLoadParent; // Enemy Load Parent 추가
    public GameObject waypointPrefab;
    public GameObject cubePrefab;
    public GameObject startPointPrefab;
    public GameObject endPointPrefab;
    public GameObject enemyLoadPrefab;

    public Vector2 mapSize = new Vector2(100, 100); // 맵 크기
    public float gridSize = 4f; // 그리드 크기
    public Color gridColor = Color.gray; // 그리드 색상

    public Transform[] Waypoints { get; private set; }
    public Transform[] Cubes { get; private set; }
    public Transform[] EnemyLoads { get; private set; } // Enemy Load 추가

    public static MapManager Instance
    {
        get
        {
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

        InitializeMap();
    }

    private void Start()
    {
        LoadMapData(currentMapIndex);
    }

    // 맵 초기화 함수
    public void InitializeMap()
    {
        Waypoints = new Transform[waypointParent.childCount];
        for (int i = 0; i < Waypoints.Length; i++)
        {
            Waypoints[i] = waypointParent.GetChild(i);
        }

        Cubes = new Transform[cubeParent.childCount];
        for (int i = 0; i < Cubes.Length; i++)
        {
            Cubes[i] = cubeParent.GetChild(i);
        }

        EnemyLoads = new Transform[enemyLoadParent.childCount]; // Enemy Load 초기화
        for (int i = 0; i < EnemyLoads.Length; i++)
        {
            EnemyLoads[i] = enemyLoadParent.GetChild(i);
        }
    }

    // 맵 데이터를 로드하는 함수
    public void LoadMapData(int mapIndex)
    {
        if (mapDataArray != null && mapDataArray.Length > mapIndex)
        {
            MapData mapData = mapDataArray[mapIndex];
            ClearExistingObjects(); // 기존 오브젝트 제거
            InitializeMap(); // 초기 상태로 맵 초기화

            // 웨이포인트 로드
            foreach (Vector3 point in mapData.waypoints)
            {
                GameObject waypointObject = Instantiate(waypointPrefab, point, Quaternion.identity, waypointParent);
            }

            // 시작 위치 로드
            if (mapData.startPoint != Vector3.zero)
            {
                if (startPoint == null)
                {
                    GameObject startObject = Instantiate(startPointPrefab, mapData.startPoint, Quaternion.identity);
                    startObject.name = "Start Point";
                    startPoint = startObject.transform;
                }
                else
                {
                    startPoint.position = mapData.startPoint;
                }
            }

            // 끝 위치 로드
            if (mapData.endPoint != Vector3.zero)
            {
                if (endPoint == null)
                {
                    GameObject endObject = Instantiate(endPointPrefab, mapData.endPoint, Quaternion.identity);
                    endObject.name = "End Point";
                    endPoint = endObject.transform;
                }
                else
                {
                    endPoint.position = mapData.endPoint;
                }
            }

            // 큐브 로드
            foreach (Vector3 cubePos in mapData.cubes)
            {
                GameObject cubeObject = Instantiate(cubePrefab, cubePos, Quaternion.identity, cubeParent);
            }

            // Enemy Load 로드
            foreach (Vector3 enemyLoadPos in mapData.enemyLoads)
            {
                GameObject enemyLoadObject = Instantiate(enemyLoadPrefab, enemyLoadPos, Quaternion.identity, enemyLoadParent);
            }

            InitializeMap();

            OnMapLoaded?.Invoke();
        }
    }

    // 기존 오브젝트를 제거하는 함수
    private void ClearExistingObjects()
    {
        for (int i = waypointParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(waypointParent.GetChild(i).gameObject);
        }

        for (int i = cubeParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(cubeParent.GetChild(i).gameObject);
        }

        for (int i = enemyLoadParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(enemyLoadParent.GetChild(i).gameObject);
        }

        if (startPoint != null)
        {
            DestroyImmediate(startPoint.gameObject);
            startPoint = null;
        }

        if (endPoint != null)
        {
            DestroyImmediate(endPoint.gameObject);
            endPoint = null;
        }
    }

    // 맵 데이터를 저장하는 함수
    public void SaveMapData()
    {
        if (mapDataArray != null && currentMapIndex < mapDataArray.Length)
        {
            MapData mapData = mapDataArray[currentMapIndex];

            mapData.waypoints = new Vector3[waypointParent.childCount];
            for (int i = 0; i < waypointParent.childCount; i++)
            {
                mapData.waypoints[i] = waypointParent.GetChild(i).position;
            }

            mapData.startPoint = startPoint != null ? startPoint.position : Vector3.zero;
            mapData.endPoint = endPoint != null ? endPoint.position : Vector3.zero;

            mapData.cubes = new Vector3[cubeParent.childCount];
            for (int i = 0; i < cubeParent.childCount; i++)
            {
                mapData.cubes[i] = cubeParent.GetChild(i).position;
            }

            mapData.enemyLoads = new Vector3[enemyLoadParent.childCount];
            for (int i = 0; i < enemyLoadParent.childCount; i++)
            {
                mapData.enemyLoads[i] = enemyLoadParent.GetChild(i).position;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(mapData);
#endif
        }
    }
}
