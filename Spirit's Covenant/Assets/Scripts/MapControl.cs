using UnityEngine;

public class MapControl : MonoBehaviour
{
    [SerializeField]
    GameManager_Map manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LevelOne()
    {
        manager.BattleScene();
    }
}
