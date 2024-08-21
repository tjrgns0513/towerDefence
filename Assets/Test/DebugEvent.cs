using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[ExecuteInEditMode]
public class DebugEvent : MonoBehaviour
{

    void Start()
    {
        Debug.Log("Start");
    }

    private void Awake()
    {
        Debug.Log("Awake");
    }

    private void OnEnable()
    {
        Debug.Log("Enable");
    }

    private void OnDisable()
    {
        Debug.Log("Disable");
    }

    private void OnDestroy()
    {
        Debug.Log("Destroy");
    }
}
