using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public GameObject startGameUI;
    public GameObject gameOverUI;
    public GameObject[] gameObjectsToDisable; // 게임오브젝트 상호작용제어

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ShowStartGameUI();
    }

    public void StartGame()
    {
        startGameUI.SetActive(false);
        SetGameObjectsInteractable(true);
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Retry()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    void ShowStartGameUI()
    {
        startGameUI.SetActive(true);
        SetGameObjectsInteractable(false);
        Time.timeScale = 0f;
    }

    void SetGameObjectsInteractable(bool isInteractable)
    {
        foreach (GameObject obj in gameObjectsToDisable)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = isInteractable;
            }
        }
    }
}
