using UnityEngine;
using UnityEngine.UI;
using SpiritsCovenant; // So we can access GameData

public class MapLevelButton : MonoBehaviour
{
    [SerializeField] private GameManager_Map manager;
    [SerializeField] private int levelNumber = 1;

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClickLevel);
    }

    void OnClickLevel()
    {
        // Set currentLevel in GameData.
        GameData.currentLevel = levelNumber;
        // Load the battle scene.
        if (manager != null)
            manager.BattleScene();
        else
            Debug.LogWarning("MapLevelButton: No reference to GameManager_Map found.");
    }
}
