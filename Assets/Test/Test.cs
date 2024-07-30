using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


public class Test : MonoBehaviour
{
    [MenuItem("Tools/Test")]
    // Start is called before the first frame update
    public static void TestList()
    {
        var testclasses = CreateList();
        List<TestClass> testclasses1 = Clone(testclasses);

        testclasses[0].testTypes[0] = StructureType.EnemyRoad;

        testclasses1[1].testTypes[0] = StructureType.EndPoint;

        //for (int i = 0; i < testclasses.Count; i++)
        //{
        //    Debug.Log(testclasses[i].testTypes[0]);
        //}

        //waypoint -> enemeyLoad
        //cube -> cube
        //Enemyload -> enemylaod

        //Debug.Log("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");

        //for (int i = 0; i < testclasses1.Count; i++)
        //{
        //    Debug.Log(testclasses1[i].testTypes[0]);
        //}
        //waypoint -> waypoint
        // cube -> endpoint
        // enemylaod -> enemylaod
        int []testArray = { 1, 2, 3 };
        foreach(var text in testArray)
        {
            Debug.Log(text);
        }
    }

    private static List<TestClass> Clone(List<TestClass> testclasses)
    {
        var types1 = testclasses[0].testTypes.ToList();
        var types2 = testclasses[1].testTypes.ToList();
        var types3 = testclasses[2].testTypes.ToList();

        var testclasses1 = new List<TestClass>();

        testclasses1.Add(new TestClass() { testTypes = types1 });
        testclasses1.Add(new TestClass() { testTypes = types2 });
        testclasses1.Add(new TestClass() { testTypes = types3 });
        return testclasses1;
    }

    public static List<TestClass> CreateList()
    {
        var testclasses = new List<TestClass>();

        testclasses.Add(new TestClass() { testTypes = new List<StructureType>() { StructureType.Waypoint} });
        testclasses.Add(new TestClass() { testTypes = new List<StructureType>() { StructureType.Cube} });
        testclasses.Add(new TestClass() { testTypes = new List<StructureType>() { StructureType.EnemyRoad} });

        return testclasses;
    }

    [Serializable]
    public class TestClass
    {
        public List<StructureType> testTypes;
    }


}
