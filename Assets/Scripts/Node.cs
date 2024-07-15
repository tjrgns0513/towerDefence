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

        //ÅÍ·¿Áþ´Âºñ¿ë 50°ñµåº¸´Ù Àû´Ù¸é
        if(RewardManager.Instance.GetGold() < 50)
        {
            return;
        }

        //ÅÍ·¿Áþ´Âºñ¿ë Â÷°¨
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
