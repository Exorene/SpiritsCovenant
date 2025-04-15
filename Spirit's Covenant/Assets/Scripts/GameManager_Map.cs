using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Map : MonoBehaviour
{
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
}
