using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "ScriptableObjects/Enemy", order = 2)]
public class EnemyType : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public float speed;
    public int maxHealth;
    public int rewardGold = 10;
    //public List<EnemyStat> enemyStat; 
}
