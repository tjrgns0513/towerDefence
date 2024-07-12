using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;

    private int playerLife = 3;
    public Text playerLifeText;

    public static PlayerManager Instance
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
        playerLifeText.text = playerLife.ToString();
    }

    public void PlayerLife(int life)
    {
        playerLife -= life;

        playerLifeText.text = playerLife.ToString();

        if (playerLife == 0)
        {
            PlayerDie();
        }
    }
    public void PlayerDie()
    {
        GameManager.Instance.GameOver();
    }
}
