using UnityEngine;

public class BuildManager : MonoBehaviour
{
    private static BuildManager instance;
    private GameObject turretToBuild;
    public GameObject standardTurretPrefab;

    public static BuildManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    
    private void Start()
    {
        turretToBuild = standardTurretPrefab;
    }

    

    public GameObject GetTurretToBuild()
    {
        return turretToBuild;
    }

}
