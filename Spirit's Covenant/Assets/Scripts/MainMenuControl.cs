using UnityEngine;

public class MainMenuControl : MonoBehaviour
{

    [SerializeField]
    GameManager_Main manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MapButton()
    {
        manager.MapScene();
    }
}
