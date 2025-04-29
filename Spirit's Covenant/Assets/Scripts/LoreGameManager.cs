using UnityEngine;
using UnityEngine.SceneManagement;

public class LoreGameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BattleScene()
    {
        SceneManager.LoadScene("Battle Scene");
    }

    public void MapScene()
    {
        SceneManager.LoadScene("MapScene");
    }

    public void LoseScene()
    {
        SceneManager.LoadScene("LoseScene");
    }
}
