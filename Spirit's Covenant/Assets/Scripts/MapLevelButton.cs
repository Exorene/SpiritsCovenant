using UnityEngine;
using UnityEngine.UI;
using SpiritsCovenant;

public class MapLevelButton : MonoBehaviour
{
    [SerializeField] private GameManager_Map manager;
    [SerializeField] private int levelNumber = 1;

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = levelNumber <= GameData.currentLevel;
            btn.onClick.AddListener(OnClickLevel);
        }
    }

    void OnClickLevel()
    {
        GameData.currentLevel = levelNumber;
        if (manager != null)
            manager.BattleScene();
        else
            Debug.LogWarning("MapLevelButton: No reference to GameManager_Map found.");
    }
}
