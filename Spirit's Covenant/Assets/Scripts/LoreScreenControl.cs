using UnityEngine;
using UnityEngine.SceneManagement;
public class LoreScreenControl : MonoBehaviour
{

    [SerializeField]
    LoreGameManager manager;

    public void ContinueButton()
    {
        manager.MapScene();
    }
}
