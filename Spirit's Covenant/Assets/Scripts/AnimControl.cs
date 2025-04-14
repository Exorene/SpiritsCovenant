using UnityEngine;

public class AnimControl : MonoBehaviour
{

    [SerializeField] Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("isPlayerTurn", true);
        anim.SetBool("isPlayerTurn", false);
        anim.SetTrigger("attackButton");
    }
}
