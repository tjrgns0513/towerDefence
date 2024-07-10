using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletImpactEffect : MonoBehaviour
{
    public GameObject impactEffect;

    public void HitBulletImpactEffect()
    {
        StartCoroutine("ReturnImpactEffectToPool");
    }
    IEnumerator ReturnImpactEffectToPool()
    {
        yield return new WaitForSeconds(2f);
        ObjectPoolManager.Instance.ReturnObjectToPool(impactEffect, "ImpactEffect");
    }
}
