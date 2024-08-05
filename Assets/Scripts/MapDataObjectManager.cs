using System.Collections.Generic;
using UnityEngine;

public class MapDataObjectManager : MonoBehaviour
{
    private static MapDataObjectManager instance = null;

    public static MapDataObjectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapDataObjectManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("MapDataObjectManager");
                    instance = singletonObject.AddComponent<MapDataObjectManager>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    public List<List<GameObject>> objects = new List<List<GameObject>>();

    //public Dictionary<int, Dictionary<Vector2Int, GameObject>> objects;
    public List<List<GameObject>> objectsClone;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Init(int x, int y)
    {
        objects.Clear();
        for(var i = 0; i < y; ++i)
        {
            var ylist = new List<GameObject>();
            
            for (var j = 0; j < x; ++j)
            {
                GameObject newGameObject = null;
                ylist.Add(newGameObject);
            }
            objects.Add(ylist);
        }

    }

    public void Clear()
    {
        objects.ForEach(o => o.ForEach(o2 => DestroyImmediate(o2)));
        objects.Clear();
    }

    public void Init()
    {
        // 기존 데이터 클리어
        if (objects != null)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                var list = objects[i];
                for (int j = 0; j < list.Count; j++)
                {
                    var obj = list[j];
                    if (obj != null)
                    {
                        DestroyImmediate(obj); // 기존 게임 오브젝트 삭제
                    }
                }
            }

            objects.Clear(); // 리스트 비우기
        }
        else
        {
            objects = new List<List<GameObject>>();
        }

        for (var y = 0; y < MapManager.Instance.mapSize.y; y++)
        {
            List<GameObject> list = new List<GameObject>();
            for(var x = 0; x < MapManager.Instance.mapSize.x; x++)
            {
                GameObject newGameObject = null;
                list.Add(newGameObject);
            }
            objects.Add(list);
        }
    }

    public void CloneInit()
    {
        if(objectsClone != null )
            objectsClone.Clear();

        objectsClone = new List<List<GameObject>>();

        for (var y = 0; y < MapManager.Instance.mapSize.y; y++)
        {
            List<GameObject> list = new List<GameObject>();
            for (var x = 0; x < MapManager.Instance.mapSize.x; x++)
            {
                GameObject newGameObject = objects[y][x];
                list.Add(newGameObject);
            }
            objectsClone.Add(list);
        }

        for (var y = 0; y < objects.Count; y++)
        {
            for (var x = 0; x < objects.Count; x++)
            {
                Debug.Log($"Object_{x}_{y}" + objectsClone[y][x]);
            }
        }
    }

    public void CopyObjects(int x, int y)
    {
        for (var i = 0; objectsClone.Count < y; ++i)
        {
            var ylist = new List<GameObject>();
            for (var j = 0; j < x; ++j)
            {
                GameObject newGameObject = objectsClone[y][x];
                ylist.Add(newGameObject);
            }
            objects.Add(ylist);
        }

        for (var k = 0; k < objects.Count; k++)
        {
            for (var l = 0; l < objects.Count; l++)
            {
                Debug.Log($"Object_{l}_{k}_ Copy" + objectsClone[k][l]);
            }
        }
    }
}
