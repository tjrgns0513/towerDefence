using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MapEditorWindow : EditorWindow
{
    private MapManager mapManager;
    private bool addingWaypoint = false;
    private bool addingCube = false;
    private bool addingEnemyRoad = false;
    private bool deleteObejct = false;

    private float snapValue = 4.0f; // 스냅 간격
    private float minDistance = 0.1f; // 오브젝트가 이미 있는지 확인하는 최소 거리
    private int selectedMapIndex = 0; // 로드할 맵 인덱스
    private int groupIndex = 0; // 그룹 인덱스 ( startpoint , endpoint , waypoint 하나의 그룹의 대한 인덱스 )
    private int wayPointIndex = 0; // 웨이 포인트 인덱스

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }


    private void OnEnable()
    { 
        SceneView.duringSceneGui += OnSceneGUI; // Scene GUI 이벤트 등록

        CompilationPipeline.compilationFinished += MapManager.Instance.OnCompilationFinished;

        MapManager.Instance.LoadMapData(0);
    }

    private void OnDisable()
    {
        CompilationPipeline.compilationFinished -= MapManager.Instance.OnCompilationFinished;

        SceneView.duringSceneGui -= OnSceneGUI; // Scene GUI 이벤트 해제
    }

    private void OnGUI()
    {
        GUILayout.Label("Data Settings", EditorStyles.boldLabel);
        groupIndex = EditorGUILayout.IntField("Group Index", groupIndex);
        snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
        minDistance = EditorGUILayout.FloatField("Min Distance", minDistance);

        GUILayout.Label("Map Settings", EditorStyles.boldLabel);

        MapManager.Instance.GetGroupIndex(groupIndex);

        //맵사이즈 조절
        if (mapManager != null)
        {
            Vector2Int mapSizeVector = new Vector2Int(MapManager.Instance.mapSize.x, MapManager.Instance.mapSize.y);

            var mapSize = EditorGUILayout.Vector2IntField("Map Size", mapSizeVector);

            var mapX = MapManager.Instance.mapSize.x;
            var mapY = MapManager.Instance.mapSize.y;

            Vector2Int currentMapsize = new Vector2Int(mapX, mapY);

            if (mapSize != currentMapsize)
            {
                mapManager.mapSize.x = mapSize.x;
                mapManager.mapSize.y = mapSize.y;
                mapManager.MapRefresh();
            }
        }

        // 각 모드 버튼 설정
        addingWaypoint = GUILayout.Toggle(addingWaypoint, "Add Waypoint Mode", "Button");
        if (addingWaypoint)
        {
            addingCube = addingEnemyRoad = deleteObejct = false;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Waypoint Index: ");
            string indexString = GUILayout.TextField(wayPointIndex.ToString(), GUILayout.Width(50));
            int.TryParse(indexString, out wayPointIndex); // 입력된 텍스트를 인덱스로 변환
            MapManager.Instance.SetWayPointIndex(wayPointIndex);
            GUILayout.EndHorizontal();

            
        }

        addingCube = GUILayout.Toggle(addingCube, "Add Cube Mode", "Button");
        if (addingCube)
        {
            addingWaypoint = addingEnemyRoad = deleteObejct = false;
        }

        addingEnemyRoad = GUILayout.Toggle(addingEnemyRoad, "Add Enemy Road Mode", "Button");
        if (addingEnemyRoad)
        {
            addingWaypoint = addingCube = deleteObejct = false;
        }

        deleteObejct = GUILayout.Toggle(deleteObejct, "Delete Object Mode", "Button");
        if (deleteObejct)
        {
            addingWaypoint = addingCube = addingEnemyRoad = false;
        }


        // 로드할 맵 인덱스 선택
        GUILayout.Label("Load Map", EditorStyles.boldLabel);
        selectedMapIndex = EditorGUILayout.IntField("Map Index", selectedMapIndex);


        // 맵 초기화, 저장, 로드 버튼
        if (GUILayout.Button("Clear Map"))
        {
            MapManager.Instance.ClearMap(); 
            MapManager.Instance.ClearData();
        }

        if (GUILayout.Button("Save Map"))
        {
            MapManager.Instance.SaveMapData(selectedMapIndex);
        }

        if (GUILayout.Button("Load Map"))
        {
            MapManager.Instance.ClearMap();
            MapManager.Instance.LoadMapData(selectedMapIndex);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
            if (mapManager == null)
            {
                Debug.LogError("MapManager not found in the scene.");
                return;
            }
        }

        DrawGrid(); // 그리드 그리기
        DrawMapBoundary(); // 맵 경계 그리기
        DrawGridCoordinates(); // 그리드 좌표 표시

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;

        // Ctrl + Z를 누르면 Undo 실행
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Z)
        {
            Undo.PerformUndo();
            e.Use();
        }
        // Ctrl + Y를 누르면 Redo 실행
        else if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Y)
        {
            Undo.PerformRedo();
            e.Use();
        }
        // 마우스 클릭 또는 드래그 이벤트 처리
        else if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);
                point = SnapToGrid(point, snapValue); // 스냅 적용
                if (addingWaypoint)
                {
                    int x = Mathf.FloorToInt(point.x / MapManager.Instance.gridSize);
                    int y = Mathf.FloorToInt(point.z / MapManager.Instance.gridSize);

                    Vector2Int newWaypoint = new Vector2Int(x, y);

                    MapManager.Instance.AddWayPoint(newWaypoint, point,  wayPointIndex, groupIndex, MapManager.Instance.waypointPrefab , MapManager.Instance.waypointParent, true);


                    //MapManager.Instance.AddObject(point, MapManager.Instance.waypointPrefab, MapManager.Instance.waypointParent, StructureType.None, true);
                    e.Use();
                }
                else if (addingCube)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.cubePrefab, MapManager.Instance.cubeParent, StructureType.Cube, true);
                    e.Use();
                }
                else if (addingEnemyRoad)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.enemyRoadPrefab, MapManager.Instance.enemyRoadParent, StructureType.EnemyRoad, true);
                    e.Use();
                }
                else if (deleteObejct)
                {
                    MapManager.Instance.DeleteObject(point, false);
                    e.Use();
                }
            }
        }

        //HandlePositionHandles(); // 오브젝트 이동 핸들 처리
    }



    // 오브젝트 이동 핸들 처리 함수
    private void HandlePositionHandles()
    {
        if(mapManager.Waypoints == null) return;

        var waypointCount = mapManager.Waypoints.Length;

        for (int i = 0; i < waypointCount; i++)
        {
            if (mapManager.Waypoints[i] != null)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(mapManager.Waypoints[i].position, Quaternion.identity);
                newPos = SnapToGrid(newPos, snapValue); // 스냅 적용
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(mapManager.Waypoints[i], "Move Waypoint");
                    mapManager.Waypoints[i].position = newPos;
                }
            }
        }

        for (int i = 0; i < mapManager.Cubes.Length; i++)
        {
            if (mapManager.Cubes[i] != null)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newCubePos = Handles.PositionHandle(mapManager.Cubes[i].position, Quaternion.identity);
                newCubePos = SnapToGrid(newCubePos, snapValue); // 스냅 적용
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(mapManager.Cubes[i], "Move Cube");
                    mapManager.Cubes[i].position = newCubePos;
                }
            }
        }

        for (int i = 0; i < mapManager.EnemyRoads.Length; i++)
        {
            if (mapManager.EnemyRoads[i] != null)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newEnemyRoadPos = Handles.PositionHandle(mapManager.EnemyRoads[i].position, Quaternion.identity);
                newEnemyRoadPos = SnapToGrid(newEnemyRoadPos, snapValue); // 스냅 적용
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(mapManager.EnemyRoads[i], "Move Enemy Load");
                    mapManager.EnemyRoads[i].position = newEnemyRoadPos;
                }
            }
        }
    }

    // 그리드 스냅 함수
    private Vector3 SnapToGrid(Vector3 position, float snapValue)
    {
        position.x = Mathf.Floor(position.x / snapValue) * snapValue + snapValue / 2;
        position.z = Mathf.Floor(position.z / snapValue) * snapValue + snapValue / 2;
        return position;
    }

    // 그리드를 그리는 함수
    private void DrawGrid()
    {
        Handles.color = mapManager.gridColor;

        float gridSize = mapManager.gridSize;
        for (float x = -100; x < 100; x += gridSize)
        {
            Handles.DrawLine(new Vector3(x, 0, -100), new Vector3(x, 0, 100));
        }
        for (float z = -100; z < 100; z += gridSize)
        {
            Handles.DrawLine(new Vector3(-100, 0, z), new Vector3(100, 0, z));
        }
    }

    // 맵 경계를 그리는 함수
    private void DrawMapBoundary()
    {
        Handles.color = Color.red;
        Vector3 size = new Vector3(mapManager.mapSize.x * mapManager.gridSize, 0, mapManager.mapSize.y * mapManager.gridSize);
        Vector3[] corners = new Vector3[4]
        {
            Vector3.zero,
            new Vector3(size.x, 0, 0),
            new Vector3(size.x, 0, size.z),
            new Vector3(0, 0, size.z)
        };

        for (int i = 0; i < 4; i++)
        {
            Handles.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }

    // 그리드 좌표를 그리는 함수
    private void DrawGridCoordinates()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        style.alignment = TextAnchor.MiddleCenter;

        float gridSize = mapManager.gridSize;
        for (float x = 0; x < mapManager.mapSize.x * mapManager.gridSize; x += mapManager.gridSize)
        {
            for (float z = 0; z < mapManager.mapSize.y * mapManager.gridSize; z+= mapManager.gridSize)
            {
                Vector3 position = new Vector3(x + gridSize / 2, 0, z + gridSize / 2);


                Handles.Label(position, $"({(int)(x / gridSize)}, {(int)(z / gridSize)})", style);
            }
        }

    }
}
