using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapEditorUndoStorage", menuName = "ScriptableObjects/MapEditorUndoStorage", order = 4)]
public class MapEditorUndoStorage : ScriptableObject
{
    public MapData mapdata = null;
}
