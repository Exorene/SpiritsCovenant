using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Battle : MonoBehaviour
{
    // (You can remove Start/Update if you don't need them)
    void Start() { }

    void Update() { }

    public void LoseScene()
    {
        SceneManager.LoadScene("LoseScene");
    }

    public void WinScene()
    {
        SceneManager.LoadScene("WinScene");
    }

    public void MapScene()
    {
        SceneManager.LoadScene("MapScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // If you later want a dedicated reward scene:
    // public void RewardScene()
    // {
    //     SceneManager.LoadScene("RewardScene");
    // }
}
