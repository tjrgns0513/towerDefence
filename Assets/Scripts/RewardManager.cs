using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    private static RewardManager instance = null;
    public Text goldText;
    private int gold = 100;

    public static RewardManager Instance
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
    private void Update()
    {
        goldText.text = gold.ToString();
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }

    public int GetGold()
    {
        return gold;
    }

    public void SubtractGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }
}
