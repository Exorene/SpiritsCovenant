using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditControl : MonoBehaviour
{

    [SerializeField]
    CreditManager manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReturnButton()
    {
        manager.MainMenu();
    }
}
