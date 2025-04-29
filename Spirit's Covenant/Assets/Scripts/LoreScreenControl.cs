using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class LoreScreenControl : MonoBehaviour
{

    [SerializeField]
    LoreGameManager manager;

    public void ContinueButton()
    {
        manager.MapScene();
    }
}
