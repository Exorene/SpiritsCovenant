using UnityEngine;

public class MapControl : MonoBehaviour
{
    [SerializeField] GameManager_Map manager;
    
    public void LevelButton()
    {
        manager.BattleScene();
    }
}
