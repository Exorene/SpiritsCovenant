using UnityEngine;
using UnityEngine.SceneManagement;
using SpiritsCovenant;  

public class WinManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MainMenu()
    {
        GameData.Reset();
        SceneManager.LoadScene("MainMenu");
    }
}
