using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Color hoverColor;
    public Vector3 positionOffset;

    private Turret turret;

    private Renderer rend;
    private Color startColor;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }

    private void OnMouseDown()
    {
        if(turret != null)
        {
            Debug.Log("Can't build there!");
            return;
        }

        if(RewardManager.Instance.GetGold() < 50)
        {
            Debug.Log("Don't have money");
            return;
        }

        RewardManager.Instance.SubtractGold(50);

        var turretToBuild = BuildManager.Instance.GetTurretToBuild();
        var obj = Instantiate(turretToBuild, transform.position + positionOffset, transform.rotation) as GameObject;
        turret = obj.GetComponent<Turret>();
        if (turret)
            turret.Init();

        obj.transform.parent = gameObject.transform;
    }

    private void OnMouseEnter()
    {
        rend.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        rend.material.color = startColor;
    }
}
