using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(MapData))]
//public class MapDataEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        MapData mapData = (MapData)target;

//        // mapDataList가 null인 경우 초기화
//        if (mapData.mapDataList == null)
//        {
//            mapData.mapDataList = new List<List<StructureType>>();
//        }

//        // 행의 수 설정
//        int newRowCount = Mathf.Max(0, EditorGUILayout.IntField("Row Count", mapData.mapDataList.Count));
//        while (newRowCount > mapData.mapDataList.Count)
//        {
//            mapData.mapDataList.Add(new List<StructureType>());
//        }
//        while (newRowCount < mapData.mapDataList.Count)
//        {
//            mapData.mapDataList.RemoveAt(mapData.mapDataList.Count - 1);
//        }

//        // 각 행에 대한 열의 수 설정
//        for (int i = 0; i < mapData.mapDataList.Count; i++)
//        {
//            EditorGUILayout.LabelField("Row " + i);
//            if (mapData.mapDataList[i] == null)
//            {
//                mapData.mapDataList[i] = new List<StructureType>();
//            }

//            int newColumnCount = Mathf.Max(0, EditorGUILayout.IntField("Column Count", mapData.mapDataList[i].Count));
//            while (newColumnCount > mapData.mapDataList[i].Count)
//            {
//                mapData.mapDataList[i].Add(StructureType.None);
//            }
//            while (newColumnCount < mapData.mapDataList[i].Count)
//            {
//                mapData.mapDataList[i].RemoveAt(mapData.mapDataList[i].Count - 1);
//            }

//            // 각 열의 값 설정
//            for (int j = 0; j < mapData.mapDataList[i].Count; j++)
//            {
//                mapData.mapDataList[i][j] = (StructureType)EditorGUILayout.EnumPopup("Element " + j, mapData.mapDataList[i][j]);
//            }
//        }
//        if (GUILayout.Button("Save"))
//        {
//            EditorUtility.SetDirty(mapData);
//            AssetDatabase.SaveAssets();
//        }
//        // 변경 사항 저장
//        //if (GUI.changed)
//        //{
//        //    EditorUtility.SetDirty(target);
//        //}
//    }
//}
