using UnityEngine;

public class MenuControl : MonoBehaviour
{

    [SerializeField]
    GameManager_Lose manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ContinueButton()
    {
        manager.MapScene();
    }

    public void QuitButton()
    {
        manager.MainMenu();
    }
}
