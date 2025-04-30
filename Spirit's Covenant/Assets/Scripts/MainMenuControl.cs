using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using SpiritsCovenant;
#endif

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

    public void QuitButton()
    {
        Application.Quit();

        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }
}
