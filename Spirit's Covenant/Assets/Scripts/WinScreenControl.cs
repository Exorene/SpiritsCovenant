using UnityEngine;

public class WinScreenControl : MonoBehaviour
{

    [SerializeField]
    WinManager manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MainMenuButton()
    {
        manager.MainMenu();
    }
}
