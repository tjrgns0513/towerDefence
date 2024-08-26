using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private MapManager mapManager;

    public MapEditorIndexStorage editorIndex;
    public MapEditorUndoStorage editorUndo;

    private bool addingWaypoint = false;
    private bool addingCube = false;
    private bool addingEnemyRoad = false;
    private bool deleteObejct = false;

    private float snapValue = 4.0f; // 스냅 간격
    private int selectedMapIndex = 0; // 로드할 맵 인덱스
    private Vector2Int mapSize;

    private List<LabelInfo> waypointLabelInfos = new List<LabelInfo>();
    Dictionary<Vector2Int, List<int>> dicLabel = new Dictionary<Vector2Int, List<int>>();

    private Color[] groupColors = new Color[]
    {
        Color.red, Color.green, Color.blue, Color.white, Color.black, Color.yellow,
        Color.cyan, Color.magenta, Color.gray, Color.grey, Color.clear
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

        CompilationPipeline.compilationFinished += mapManager.OnCompilationFinished;
        mapManager.LoadMapData(0);
        SetWaypointLabelInfos(mapManager.currentMapData.routes);
    }

    private void OnDisable()
    {
        CompilationPipeline.compilationFinished -= mapManager.OnCompilationFinished;
        SceneView.duringSceneGui -= OnSceneGUI; // Scene GUI 이벤트 해제
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Settings", EditorStyles.boldLabel);

        //맵사이즈 조절
        if (mapManager != null)
        {
            DrawMapSize();
        }

        GUILayout.Space(20);
        GUILayout.Label("Edit Mode", EditorStyles.boldLabel);

        // 각 모드 버튼 설정
        DrawEditMode();

        GUILayout.Space(20);
        // 로드할 맵 인덱스 선택
        GUILayout.Label("Load Map", EditorStyles.boldLabel);
        selectedMapIndex = EditorGUILayout.IntField("Map Index", selectedMapIndex);

        // 맵 초기화, 저장, 로드 버튼
        DrawButtons();

        DrawObjectField();

        SceneView.RepaintAll();
    }

    private void DrawObjectField()
    {
        editorIndex = (MapEditorIndexStorage)EditorGUILayout.ObjectField("Map Editor Index Storage", editorIndex, typeof(MapEditorIndexStorage), false);

        if (editorIndex != null)
        {
            EditorGUILayout.LabelField("Group Index:", editorIndex.groupIndex.ToString());
            EditorGUILayout.LabelField("WayPoint Index:", editorIndex.wayPointIndex.ToString());
        }

        editorUndo = (MapEditorUndoStorage)EditorGUILayout.ObjectField("Map Editor Undo Storage", editorUndo, typeof(MapEditorUndoStorage), false);
    }

    private void DrawButtons()
    {
        if (GUILayout.Button("Clear Map"))
        {
            mapManager.ClearMap();
            waypointLabelInfos.Clear();

            string clearInfo = "Clear Map";
            ModifyData(clearInfo);
        }

        if (GUILayout.Button("Save Map"))
        {
            mapManager.SaveMapData(selectedMapIndex);
            SetWaypointLabelInfos(mapManager.currentMapData.routes);
        }

        if (GUILayout.Button("Load Map"))
        {
            mapManager.ClearMap();
            mapManager.LoadMapData(selectedMapIndex);
            SetWaypointLabelInfos(mapManager.currentMapData.routes);

            string loadInfo = "Load Map";
            ModifyData(loadInfo);
        }
    }

    private void DrawEditMode()
    {
        DrawToggle("Edit Cube Mode", ref addingCube, () => ResetOtherModes(ref addingWaypoint, ref addingEnemyRoad));
        DrawToggle("Edit Enemy Road Mode", ref addingEnemyRoad, () => ResetOtherModes(ref addingWaypoint, ref addingCube));
        DrawToggle("Edit Waypoint Mode", ref addingWaypoint, () => ResetOtherModes(ref addingEnemyRoad, ref addingCube));
        if(addingWaypoint)
            DrawIndex();
    }

    private void DrawToggle(string label, ref bool toggle, Action onToggleOn)
    {
        if(GUILayout.Toggle(toggle, label, "Button") != toggle)
        {
            toggle = !toggle;
            if (toggle) onToggleOn.Invoke();
        }
    }

    private void ResetOtherModes(ref bool mode1, ref bool mode2)
    {
        mode1 = false;
        mode2 = false;
    }

    private void DrawIndex()
    {
        DrawIndexControl("Group Index: ", ref editorIndex.groupIndex, () => mapManager.SetRouteCount());
        DrawIndexControl("Waypoint Index: ", ref editorIndex.wayPointIndex, () => mapManager.SetWayPointIndex(editorIndex.wayPointIndex));
        mapManager.GetGroupIndex(editorIndex.groupIndex);
    }

    private void DrawIndexControl(string label, ref int index, Action onIndexChange)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if(label == "Group Index: ")
        {
            GUILayout.Space(20);
        }
        
        GUILayout.Label(label);

        if (GUILayout.Button("◄", GUILayout.Width(30)))
        {
            index = Mathf.Max(0, --index);
            onIndexChange.Invoke();
        }

        string indexString = GUILayout.TextField(index.ToString(), GUILayout.Width(50));
        if (int.TryParse(indexString, out int parsedIndex))
        {
            index = Mathf.Max(0, parsedIndex);
        }

        if (GUILayout.Button("►", GUILayout.Width(30)))
        {
            index++;
            onIndexChange.Invoke();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawMapSize()
    {
        mapSize = EditorGUILayout.Vector2IntField("Map Size", mapSize);

        var mapX = mapManager.mapSize.x;
        var mapY = mapManager.mapSize.y;

        Vector2Int currentMapsize = new Vector2Int(mapX, mapY);

        if (mapSize != currentMapsize)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);

            if (GUILayout.Button("Modify", GUILayout.Width(150f)))
            {
                Undo.RecordObject(mapManager, "Modify Map Size");
                mapManager.mapSize.x = mapSize.x;
                mapManager.mapSize.y = mapSize.y;
                mapManager.MapRefresh();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", GUILayout.Width(150f)))
            {
                Undo.RecordObject(mapManager, "Modify Map Size");
                mapSize.x = mapManager.mapSize.x;
                mapSize.y = mapManager.mapSize.y;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            Repaint();
        }
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
        Undo.RegisterCompleteObjectUndo(editorUndo, $"{info}");
        editorUndo.mapdata = mapManager.currentMapData.Clone();
        EditorUtility.SetDirty(editorUndo);
    }

    public void UndoData()
    {
        mapManager.InitializeMap(mapManager.currentMapData);

        mapManager.currentMapData = editorUndo.mapdata.Clone();

        mapManager.DrawMap();
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

        EnsureRoutesList(); // Routes List 없을경우 생성 
        DrawWaypointLines(); // 웨이포인트 라인 그리기
        DrawWayPointLabels(); // 웨이포인트 라벨 그리기

        Repaint();
    }

    private void EnsureRoutesList()
    {
        if (mapManager.routes.Count < editorIndex.groupIndex + 1)
        {
            for (var i = 0; i < editorIndex.groupIndex - mapManager.routes.Count + 1; i++)
            {
                Route route = new Route();

                mapManager.routes.Add(route);
            }
        }
    }

    private void UpdateInput()
    {
        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

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
            string undoInfo = "";

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                point = SnapToGrid(point, snapValue); // 스냅 적용

                int x = Mathf.FloorToInt(point.x / mapManager.gridSize);
                int y = Mathf.FloorToInt(point.z / mapManager.gridSize);

                if (addingWaypoint)
                {
                    AddWaypoint(point,x ,y);

                    undoInfo = $"Add Waypoint({x},{y})";

                    e.Use();
                }
                else if (addingCube)
                {
                    mapManager.AddObject(point, mapManager.cubePrefab, mapManager.objectParent, StructureType.Cube, true);

                    undoInfo = $"Add Cube({x},{y})";

                    e.Use();
                }
                else if (addingEnemyRoad)
                {
                    mapManager.AddObject(point, mapManager.enemyRoadPrefab, mapManager.objectParent, StructureType.EnemyRoad, true);

                    undoInfo = $"Add EnemyRoad({x},{y})";

                    e.Use();
                }

                ModifyData(undoInfo);
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

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                point = SnapToGrid(point, snapValue);

                mapManager.DeleteObject(point, false);

                int x = Mathf.FloorToInt(point.x / mapManager.gridSize);
                int y = Mathf.FloorToInt(point.z / mapManager.gridSize);

                string deleteObjectInfo = $"delete {mapManager.currentMapData.verticalLines[y].horizontalLines[x]} {x},{y}";

                ModifyData(deleteObjectInfo);

                if (addingWaypoint)
                {
                    waypointLabelInfos.RemoveAt(editorIndex.groupIndex);

                    LabelInfo labelInfo = new LabelInfo();

                    waypointLabelInfos.Add(labelInfo);

                    var waypointPos = mapManager.routes[editorIndex.groupIndex]?.waypointCoordinates;

                    for (int i = 0; i < waypointPos.Count; i++)
                    {
                        waypointLabelInfos[editorIndex.groupIndex].labelPosition.Add(waypointPos[i]);
                    }
                }
            }
        }
    }

    private void AddWaypoint(Vector3 point, int x, int y)
    {
        //그리드 밖 설치 리턴
        if (y < 0 || mapManager.currentMapData.verticalLines.Count <= y ||
            x < 0 || mapManager.currentMapData.verticalLines[y].horizontalLines.Count <= x)
            return;

        Vector2Int newWaypoint = new Vector2Int(x, y);

        var waypointPos = mapManager.routes[editorIndex.groupIndex]?.waypointCoordinates;

        //연속으로 같은위치는 설치 x
        if (editorIndex.wayPointIndex > 0)
        {
            if (waypointPos[editorIndex.wayPointIndex - 1] == newWaypoint)
                return;
        }

        //오브젝트 추가
        mapManager.AddWayPoint(newWaypoint, point, mapManager.waypointPrefab, mapManager.waypointParent, true);

        if (waypointLabelInfos.Count <= editorIndex.groupIndex)
        {
            LabelInfo labelInfo = new LabelInfo();

            waypointLabelInfos.Add(labelInfo);
        }

        if (waypointLabelInfos[editorIndex.groupIndex].labelPosition.Count <= editorIndex.wayPointIndex)
        {
            waypointLabelInfos[editorIndex.groupIndex].labelPosition.Add(newWaypoint);
        }
    }

    //웨이포인트 라인 그리기
    private void DrawWaypointLines()
    {
        for (var y = 0; y < mapManager.routes.Count; y++)
        {
            for (var x = 1; x < mapManager.routes[y].waypointCoordinates.Count; x++)
            {
                float gridSize = mapManager.gridSize;
                var waypointPos = mapManager.routes[y].waypointCoordinates;

                Vector2Int startInt = new (waypointPos[x - 1].x, waypointPos[x - 1].y);
                Vector2Int EndInt = new (waypointPos[x].x, waypointPos[x].y);
                Vector3 start = new (startInt.x * gridSize + 2, 0, startInt.y * gridSize + 2);
                Vector3 end = new (EndInt.x * gridSize + 2, 0, EndInt.y * gridSize + 2);
                Handles.color = GetGroupColor(y);
                Handles.DrawLine(start, end);
            }
        }
    }
    //웨이포인트 라벨 그리기
    private void DrawWayPointLabels()
    {
        GUIStyle style = new GUIStyle()
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        style.normal.textColor = GetGroupColor(editorIndex.groupIndex);

        AddWaypointPosToLabelDictionary();

        foreach (KeyValuePair<Vector2Int, List<int>> data in dicLabel)
        {
            var pos = data.Key;
            var label = data.Value;

            var labelPos = new Vector3(pos.x * mapManager.gridSize + mapManager.gridSize / 2, 0, 
                                        pos.y * mapManager.gridSize + mapManager.gridSize / 3);

            string labelTxt = label[0].ToString();

            for (int i = 1; i < label.Count; i++)
            {
                labelTxt = $"{labelTxt}, {label[i]}";
            }

            Handles.Label(labelPos, labelTxt, style);
        }
    }
    //웨이포인트 Dictionary에 Label Add
    private void AddWaypointPosToLabelDictionary()
    {
        dicLabel.Clear();

        var waypointPos = mapManager.routes[editorIndex.groupIndex].waypointCoordinates;

        for (var i = 0; i < waypointPos.Count; i++)
        {
            var pos = new Vector2Int(waypointPos[i].x, waypointPos[i].y);

            AddToDictionary(dicLabel, pos, i);
        }
    }
    //웨이포인트 Dicstionary Add
    private void AddToDictionary(Dictionary<Vector2Int, List<int>> dict, Vector2Int key, int value)
    {
        if (!dict.ContainsKey(key))
        {
            dict[key] = new List<int>();
        }
        dict[key].Add(value);
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
            for (float y = 0; y < mapManager.mapSize.y * mapManager.gridSize; y+= mapManager.gridSize)
            {
                Vector3 position = new Vector3(x + gridSize / 2, 0, y + gridSize / 2);
                Handles.Label(position, $"({(int)(x / gridSize)}, {(int)(y / gridSize)})", style);
            }
        }
        
    }

}
