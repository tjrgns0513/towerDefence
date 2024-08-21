using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private MapManager mapManager;

    public MapEditorIndexStorage mapEditorIndexStorages;
    public MapEditorUndoStorage mapEditorUndoStorages;

    private bool addingWaypoint = false;
    private bool addingCube = false;
    private bool addingEnemyRoad = false;
    private bool deleteObejct = false;

    private float snapValue = 4.0f; // 스냅 간격
    //private float minDistance = 0.1f; // 오브젝트가 이미 있는지 확인하는 최소 거리
    private int selectedMapIndex = 0; // 로드할 맵 인덱스
    private Vector2Int mapSize;

    private List<LabelInfo> waypointLabelInfos = new List<LabelInfo>();

    private Color[] groupColors = new Color[] {
        Color.red, Color.green, Color.blue, Color.white, Color.black, Color.yellow, Color.cyan, Color.magenta, Color.gray, Color.grey, Color.clear
    };
    private Color GetGroupColor(int gidx)
    {
        if (0 <= gidx && gidx < groupColors.Length)
            return groupColors[gidx];
        return Color.white;
    }

    public class LabelInfo
    {
        public int labelGroupIndex;
        public List<Vector2Int> labelPosition = new List<Vector2Int>();
        public string labelText;
    }


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
        SetWaypointLabelInfos(MapManager.Instance.currentMapData.routes);
    }

    private void OnDisable()
    {
        CompilationPipeline.compilationFinished -= MapManager.Instance.OnCompilationFinished;
        SceneView.duringSceneGui -= OnSceneGUI; // Scene GUI 이벤트 해제
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Settings", EditorStyles.boldLabel);

        //맵사이즈 조절
        if (mapManager != null)
        {
            mapSize = EditorGUILayout.Vector2IntField("Map Size", mapSize);

            var mapX = MapManager.Instance.mapSize.x;
            var mapY = MapManager.Instance.mapSize.y;

            Vector2Int currentMapsize = new Vector2Int(mapX, mapY);

            if (mapSize != currentMapsize)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);

                if (GUILayout.Button("Modify", GUILayout.Width(150f)))
                {
                    if (mapSize != currentMapsize)
                    {
                        Undo.RecordObject(mapManager, "Modify Map Size");
                        MapManager.Instance.mapSize.x = mapSize.x;
                        MapManager.Instance.mapSize.y = mapSize.y;
                        MapManager.Instance.MapRefresh();
                    }
                }

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", GUILayout.Width(150f)))
                {
                    mapSize.x = MapManager.Instance.mapSize.x;
                    mapSize.y = MapManager.Instance.mapSize.y;
                    Repaint();

                }


                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }

        }

        GUILayout.Space(20);
        GUILayout.Label("Edit Mode", EditorStyles.boldLabel);

        //snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
        //minDistance = EditorGUILayout.FloatField("Min Distance", minDistance);

        // 각 모드 버튼 설정
        addingCube = GUILayout.Toggle(addingCube, "Edit Cube Mode", "Button");
        if (addingCube)
        {
            addingWaypoint = addingEnemyRoad = false;
        }

        addingEnemyRoad = GUILayout.Toggle(addingEnemyRoad, "Edit Enemy Road Mode", "Button");
        if (addingEnemyRoad)
        {
            addingWaypoint = addingCube = false;
        }

        addingWaypoint = GUILayout.Toggle(addingWaypoint, "Edit Waypoint Mode", "Button");
        if (addingWaypoint)
        {
            addingCube = addingEnemyRoad = false;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("      Group Index: ");

            if (GUILayout.Button("◄", GUILayout.Width(30)))
            {
                mapEditorIndexStorages.groupIndex--;

                if (mapEditorIndexStorages.groupIndex < 0)
                {
                    mapEditorIndexStorages.groupIndex = 0;
                }

                MapManager.Instance.SetRouteCount();
            }

            string groupIndexString = GUILayout.TextField(mapEditorIndexStorages.groupIndex.ToString(), GUILayout.Width(50));
            int.TryParse(groupIndexString, out mapEditorIndexStorages.groupIndex);

            if (GUILayout.Button("►", GUILayout.Width(30)))
            {
                mapEditorIndexStorages.groupIndex++;
                MapManager.Instance.SetRouteCount();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Waypoint Index: ");

            if (GUILayout.Button("◄", GUILayout.Width(30)))
            {
                mapEditorIndexStorages.wayPointIndex--;
            }
            
            string wayPointIndexString = GUILayout.TextField(mapEditorIndexStorages.wayPointIndex.ToString(), GUILayout.Width(50));
            int.TryParse(wayPointIndexString, out mapEditorIndexStorages.wayPointIndex);

            if (GUILayout.Button("►", GUILayout.Width(30)))
            {
                mapEditorIndexStorages.wayPointIndex++;
            }

            if(mapEditorIndexStorages.wayPointIndex < 0)
            {
                mapEditorIndexStorages.wayPointIndex = 0;
            }

            if (mapEditorIndexStorages.groupIndex < 0)
            {
                mapEditorIndexStorages.groupIndex = 0;
            }


            MapManager.Instance.SetWayPointIndex(mapEditorIndexStorages.wayPointIndex);
            MapManager.Instance.GetGroupIndex(mapEditorIndexStorages.groupIndex);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        GUILayout.Space(20);
        // 로드할 맵 인덱스 선택
        GUILayout.Label("Load Map", EditorStyles.boldLabel);
        selectedMapIndex = EditorGUILayout.IntField("Map Index", selectedMapIndex);


        // 맵 초기화, 저장, 로드 버튼
        if (GUILayout.Button("Clear Map"))
        {
            MapManager.Instance.ClearMap();
            waypointLabelInfos.Clear();

            string clearInfo = "Clear Map";
            ModifyData(clearInfo);
        }

        if (GUILayout.Button("Save Map"))
        {
            MapManager.Instance.SaveMapData(selectedMapIndex);
            SetWaypointLabelInfos(MapManager.Instance.currentMapData.routes);
        }

        if (GUILayout.Button("Load Map"))
        {
            MapManager.Instance.ClearMap();
            MapManager.Instance.LoadMapData(selectedMapIndex);
            SetWaypointLabelInfos(MapManager.Instance.currentMapData.routes);

            string loadInfo = "Load Map";
            ModifyData(loadInfo);
        }

        mapEditorIndexStorages = (MapEditorIndexStorage)EditorGUILayout.ObjectField("Map Editor Index Storage",mapEditorIndexStorages,typeof(MapEditorIndexStorage),false);

        if (mapEditorIndexStorages != null)
        {
            EditorGUILayout.LabelField("Group Index:", mapEditorIndexStorages.groupIndex.ToString());
            EditorGUILayout.LabelField("WayPoint Index:", mapEditorIndexStorages.wayPointIndex.ToString());
        }

        mapEditorUndoStorages = (MapEditorUndoStorage)EditorGUILayout.ObjectField("Map Editor Undo Storage", mapEditorUndoStorages, typeof(MapEditorUndoStorage), false);

        SceneView.RepaintAll();
        
    }

    private void SetWaypointLabelInfos(List<Route> routes)
    {
        waypointLabelInfos.Clear();
        routes.ForEach(r => {
            var labelInfo = new LabelInfo();
            waypointLabelInfos.Add(labelInfo);
            r.waypointCoordinates.ForEach(w => labelInfo.labelPosition.Add(w));
        });
    }

    public void ModifyData(string info)
    {
        Undo.RegisterCompleteObjectUndo(mapEditorUndoStorages, $"{info}");
        mapEditorUndoStorages.mapdata = MapManager.Instance.currentMapData.Clone();
        EditorUtility.SetDirty(mapEditorUndoStorages);
    }

    public void UndoData()
    {
        //MapManager.Instance.InitializeMap(MapManager.Instance.currentMapData);

        MapManager.Instance.currentMapData = mapEditorUndoStorages.mapdata.Clone();

        if (MapManager.Instance.objectParent.childCount > 0)
        {
            for (var i = MapManager.Instance.objectParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(MapManager.Instance.objectParent.GetChild(i).gameObject);
            }
        }

        MapManager.Instance.DrawMap();
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

        Tools.current = Tool.None;

        DrawGrid(); // 그리드 그리기
        DrawMapBoundary(); // 맵 경계 그리기
        DrawGridCoordinates(); // 그리드 좌표 표시

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        UpdateInput();

        if (MapManager.Instance.routes.Count < mapEditorIndexStorages.groupIndex + 1)
        {
            for (var i = 0; i < mapEditorIndexStorages.groupIndex - MapManager.Instance.routes.Count + 1; i++)
            {
                Route route = new Route();

                MapManager.Instance.routes.Add(route);
            }
        }

        DrawWaypointLines();
        DrawWayPointLabels();

        Repaint();
    }

    private void UpdateInput()
    {
        Event e = Event.current;
        GUIStyle style = new GUIStyle()
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        // Ctrl + Z를 누르면 Undo 실행
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Z)
        {
            Undo.PerformUndo();
            UndoData();
            e.Use();
        }
        // Ctrl + Y를 누르면 Redo 실행
        else if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Y)
        {
            Undo.PerformRedo();
            UndoData();
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
                int x = Mathf.FloorToInt(point.x / MapManager.Instance.gridSize);
                int y = Mathf.FloorToInt(point.z / MapManager.Instance.gridSize);
                if (addingWaypoint)
                {

                    //그리드 밖 설치 리턴
                    if (y < 0 || MapManager.Instance.currentMapData.verticalLines.Count <= y ||
                        x < 0 || MapManager.Instance.currentMapData.verticalLines[y].horizontalLines.Count <= x)
                        return;

                    Vector2Int newWaypoint = new Vector2Int(x, y);

                    //중복설치 리턴
                    if (MapManager.Instance.routes[mapEditorIndexStorages.groupIndex].waypointCoordinates.Contains(newWaypoint))
                        return;

                    //오브젝트 추가
                    MapManager.Instance.AddWayPoint(newWaypoint, point, MapManager.Instance.waypointPrefab, MapManager.Instance.waypointParent, true);

                    if (waypointLabelInfos.Count <= mapEditorIndexStorages.groupIndex)
                    {
                        LabelInfo labelInfo = new LabelInfo();

                        waypointLabelInfos.Add(labelInfo);
                    }

                    if (waypointLabelInfos[mapEditorIndexStorages.groupIndex].labelPosition.Count <= mapEditorIndexStorages.wayPointIndex)
                    {
                        waypointLabelInfos[mapEditorIndexStorages.groupIndex].labelPosition.Add(newWaypoint);
                    }


                    e.Use();
                }
                else if (addingCube)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.cubePrefab, MapManager.Instance.objectParent, StructureType.Cube, true);

                    string addcubeInfo = $"Add Cube({x},{y})"; 

                    ModifyData(addcubeInfo);

                    e.Use();
                }
                else if (addingEnemyRoad)
                {
                    MapManager.Instance.AddObject(point, MapManager.Instance.enemyRoadPrefab, MapManager.Instance.objectParent, StructureType.EnemyRoad, true);
                    e.Use();
                }
                else if (deleteObejct)
                {
                    MapManager.Instance.DeleteObject(point, false);
                    e.Use();
                }
            }
        }
        else if (e.type == EventType.MouseUp && e.button == 1)
        {
            deleteObejct = false;
        }
        else if (e.type == EventType.MouseDown && e.button == 1)
        { 
            if (deleteObejct)
                return;

            deleteObejct = true;

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                point = SnapToGrid(point, snapValue);

                MapManager.Instance.DeleteObject(point, false);


                /*
                 * 임시
                 */
                int x = Mathf.FloorToInt(point.x / MapManager.Instance.gridSize);
                int y = Mathf.FloorToInt(point.z / MapManager.Instance.gridSize);

                string deleteObjectInfo = $"delete {MapManager.Instance.currentMapData.verticalLines[y].horizontalLines[x]} {x},{y}";

                ModifyData(deleteObjectInfo);
                /*
                * 임시
                */

                if (addingWaypoint)
                {
                    waypointLabelInfos.RemoveAt(mapEditorIndexStorages.groupIndex);

                    LabelInfo labelInfo = new LabelInfo();

                    waypointLabelInfos.Add(labelInfo);

                    for (int i = 0; i < MapManager.Instance.routes[mapEditorIndexStorages.groupIndex].waypointCoordinates.Count; i++)
                    {
                        waypointLabelInfos[mapEditorIndexStorages.groupIndex].labelPosition.Add(MapManager.Instance.routes[mapEditorIndexStorages.groupIndex].waypointCoordinates[i]);
                    }
                }
            }
        }
    }

    IEnumerator AddCube(Vector3 point)
    {
        yield return new WaitForEndOfFrame();
        MapManager.Instance.AddObject(point, MapManager.Instance.cubePrefab, MapManager.Instance.objectParent, StructureType.Cube, true);

    }
    private void DrawWaypointLines()
    {
        for (var y = 0; y < MapManager.Instance.routes.Count; y++)
        {
            for (var x = 1; x < MapManager.Instance.routes[y].waypointCoordinates.Count; x++)
            {
                float gridSize = mapManager.gridSize;

                Vector2Int startInt = new Vector2Int(MapManager.Instance.routes[y].waypointCoordinates[x - 1].x, MapManager.Instance.routes[y].waypointCoordinates[x - 1].y);
                Vector2Int EndInt = new Vector2Int(MapManager.Instance.routes[y].waypointCoordinates[x].x, MapManager.Instance.routes[y].waypointCoordinates[x].y);
                Vector3 start = new Vector3(startInt.x * gridSize + 2, 0, startInt.y * gridSize + 2);
                Vector3 end = new Vector3(EndInt.x * gridSize + 2, 0, EndInt.y * gridSize + 2);
                Handles.color = GetGroupColor(y);
                Handles.DrawLine(start, end);
            }
        }
    }

    private void DrawWayPointLabels()
    {
        GUIStyle style = new GUIStyle()
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };

        style.normal.textColor = GetGroupColor(mapEditorIndexStorages.groupIndex);

        for (var i = 0; i < MapManager.Instance.routes[mapEditorIndexStorages.groupIndex].waypointCoordinates.Count; i++)
        {
            Vector3 pos = new Vector3(MapManager.Instance.routes[mapEditorIndexStorages.groupIndex].waypointCoordinates[i].x * MapManager.Instance.gridSize + MapManager.Instance.gridSize / 2, 
                0, MapManager.Instance.routes[mapEditorIndexStorages.groupIndex].waypointCoordinates[i].y * MapManager.Instance.gridSize + MapManager.Instance.gridSize / 2);

            Handles.Label(pos, i.ToString(), style);
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
