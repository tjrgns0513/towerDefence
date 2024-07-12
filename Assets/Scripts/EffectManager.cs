using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ObjectPoolManager;

public class EffectManager : MonoBehaviour
{
    public GameObject impactEffectObj;
    public GameObject enemyDeathEffectObj;

    private void Update()
    {
        if(!impactEffectObj.GetComponent<ParticleSystem>().IsAlive())
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(impactEffectObj, PoolObjectType.ImpactEffect);
        }
        else if(!enemyDeathEffectObj.GetComponent<ParticleSystem>().IsAlive())
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(enemyDeathEffectObj, PoolObjectType.EnemyDeathEffect);
        }
    }
}
