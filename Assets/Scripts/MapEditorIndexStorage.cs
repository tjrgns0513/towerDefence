using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapEditorIndexStorage", menuName = "ScriptableObjects/MapEditorIndexStorage", order = 3)]
public class MapEditorIndexStorage : ScriptableObject
{
    public int groupIndex = 0;
    public int wayPointIndex = 0;
}
