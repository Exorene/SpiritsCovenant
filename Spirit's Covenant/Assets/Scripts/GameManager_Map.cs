using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Map : MonoBehaviour
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

    public void BattleScene()
    {
        SceneManager.LoadScene("Battle Scene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    //public void RewardScene();
    //{
        
    //}
}
