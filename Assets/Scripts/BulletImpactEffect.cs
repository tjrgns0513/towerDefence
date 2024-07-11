using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ObjectPoolManager;

public class BulletImpactEffect : MonoBehaviour
{
    public GameObject impactEffectObj;

    private void Update()
    {
        if(!impactEffectObj.GetComponent<ParticleSystem>().IsAlive())
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(impactEffectObj, PoolObjectType.ImpactEffect);
        }
    }
}
