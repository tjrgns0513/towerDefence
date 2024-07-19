using System;
using UnityEngine;

public class Node : MonoBehaviour
{
    private Turret turret;
    private Renderer rend;
    public Color hoverColor;
    public Color startColor;
    public Vector3 positionOffset;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }

    private void OnMouseDown()
    {
        if(turret != null)
        {
            return;
        }

        //터렛짓는비용 50골드보다 적다면
        if(RewardManager.Instance.GetGold() < 50)
        {
            return;
        }

        //터렛짓는비용 차감
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
