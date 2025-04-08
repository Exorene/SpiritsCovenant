using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Battle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoseScene()
    {
        SceneManager.LoadScene("LoseScene");
    }

    public void MapScene()
    {
        SceneManager.LoadScene("MapScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    //public void RewardScene();
    //{
        
    //}
}
