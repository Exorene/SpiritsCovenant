using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuControl : MonoBehaviour
{
    [SerializeField]
    GameManager_Main manager;

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
