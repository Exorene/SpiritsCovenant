using UnityEngine;
using UnityEditor;
using SpiritsCovenant;

public class MainMenuControl : MonoBehaviour
{
    [SerializeField]
    GameManager_Main manager;

    void Awake()
    {
        GameData.Reset();
    }

    public void MapButton()
    {
        manager.LoreScene();
    }

    public void CreditsButton()
    {
        manager.Credits();
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
