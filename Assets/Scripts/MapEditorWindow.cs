using UnityEditor;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private MapManager mapManager;
    private bool addingWaypoint = false;
    private bool addingStartPoint = false;
    private bool addingEndPoint = false;
    private bool addingCube = false;
    private bool addingEnemyLoad = false;

    private float snapValue = 4.0f; // 스냅 간격
    private float minDistance = 0.1f; // 오브젝트가 이미 있는지 확인하는 최소 거리
    private int selectedMapIndex = 0; // 로드할 맵 인덱스

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }


    private void OnEnable()
    {
        //MapManager.Instance.InitializeMap();
        SceneView.duringSceneGui += OnSceneGUI; // Scene GUI 이벤트 등록
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI; // Scene GUI 이벤트 해제
    }

    private void OnGUI()
    {
        GUILayout.Label("Snap Settings", EditorStyles.boldLabel);
        snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
        minDistance = EditorGUILayout.FloatField("Min Distance", minDistance);

        GUILayout.Label("Map Settings", EditorStyles.boldLabel);

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
            addingStartPoint = addingEndPoint = addingCube = addingEnemyLoad = false;
        }

        addingStartPoint = GUILayout.Toggle(addingStartPoint, "Add Start Point Mode", "Button");
        if (addingStartPoint)
        {
            addingWaypoint = addingEndPoint = addingCube = addingEnemyLoad = false;
        }

        addingEndPoint = GUILayout.Toggle(addingEndPoint, "Add End Point Mode", "Button");
        if (addingEndPoint)
        {
            addingWaypoint = addingStartPoint = addingCube = addingEnemyLoad = false;
        }

        addingCube = GUILayout.Toggle(addingCube, "Add Cube Mode", "Button");
        if (addingCube)
        {
            addingWaypoint = addingStartPoint = addingEndPoint = addingEnemyLoad = false;
        }

        addingEnemyLoad = GUILayout.Toggle(addingEnemyLoad, "Add Enemy Load Mode", "Button");
        if (addingEnemyLoad)
        {
            addingWaypoint = addingStartPoint = addingEndPoint = addingCube = false;
        }

        // 로드할 맵 인덱스 선택
        GUILayout.Label("Load Map", EditorStyles.boldLabel);
        selectedMapIndex = EditorGUILayout.IntField("Map Index", selectedMapIndex);
        //MapManager.Instance.currentMapIndex = selectedMapIndex;

        // 맵 초기화, 저장, 로드 버튼
        if (GUILayout.Button("Clear Map"))
        {
            MapManager.Instance.ClearMap();
            //ClearMap();
        }

        if (GUILayout.Button("Save Map"))
        {
            MapManager.Instance.SaveMapData(selectedMapIndex);
        }

        if (GUILayout.Button("Load Map"))
        {
            //ClearMap();
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
                    MapManager.Instance.AddObject(point, MapManager.Instance.waypointPrefab, MapManager.Instance.waypointParent, StructureType.Waypoint, true);
                    e.Use();
                }
                else if (addingStartPoint)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.startPointPrefab, MapManager.Instance.startPointParent, StructureType.StartPoint, true);
                    e.Use();
                }
                else if (addingEndPoint)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.endPointPrefab, MapManager.Instance.endPointParent, StructureType.EndPoint, true);
                    e.Use();
                }
                else if (addingCube)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.cubePrefab, MapManager.Instance.cubeParent, StructureType.Cube, true);
                    e.Use();
                }
                else if (addingEnemyLoad)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.enemyRoadPrefab, MapManager.Instance.enemyRoadParent, StructureType.EnemyRoad, true);
                    e.Use();
                }
            }
        }

        HandlePositionHandles(); // 오브젝트 이동 핸들 처리
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

        if (mapManager.startPoint != null)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newStartPos = Handles.PositionHandle(mapManager.startPoint.position, Quaternion.identity);
            newStartPos = SnapToGrid(newStartPos, snapValue); // 스냅 적용
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mapManager.startPoint, "Move Start Point");
                mapManager.startPoint.position = newStartPos;
            }
        }

        if (mapManager.endPoint != null)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newEndPos = Handles.PositionHandle(mapManager.endPoint.position, Quaternion.identity);
            newEndPos = SnapToGrid(newEndPos, snapValue); // 스냅 적용
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mapManager.endPoint, "Move End Point");
                mapManager.endPoint.position = newEndPos;
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

        for (int i = 0; i < mapManager.EnemyLoads.Length; i++)
        {
            if (mapManager.EnemyLoads[i] != null)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newEnemyLoadPos = Handles.PositionHandle(mapManager.EnemyLoads[i].position, Quaternion.identity);
                newEnemyLoadPos = SnapToGrid(newEnemyLoadPos, snapValue); // 스냅 적용
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(mapManager.EnemyLoads[i], "Move Enemy Load");
                    mapManager.EnemyLoads[i].position = newEnemyLoadPos;
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

    // 위치가 이미 점유되었는지 확인하는 함수
    //private bool IsPositionOccupied(Vector3 position, string objectType)
    //{
    //    if (objectType == "Waypoint")
    //    {
    //        foreach (Transform waypoint in mapManager.Waypoints)
    //        {
    //            if (waypoint != null && Vector3.Distance(waypoint.position, position) < minDistance)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    else if (objectType == "StartPoint")
    //    {
    //        if (mapManager.startPoint != null && Vector3.Distance(mapManager.startPoint.position, position) < minDistance)
    //        {
    //            return true;
    //        }
    //    }
    //    else if (objectType == "EndPoint")
    //    {
    //        if (mapManager.endPoint != null && Vector3.Distance(mapManager.endPoint.position, position) < minDistance)
    //        {
    //            return true;
    //        }
    //    }
    //    else if (objectType == "Cube")
    //    {
    //        foreach (Transform cube in mapManager.Cubes)
    //        {
    //            if (cube != null && Vector3.Distance(cube.position, position) < minDistance)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    else if (objectType == "EnemyLoad")
    //    {
    //        foreach (Transform enemyLoad in mapManager.EnemyLoads)
    //        {
    //            if (enemyLoad != null && Vector3.Distance(enemyLoad.position, position) < minDistance)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

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
