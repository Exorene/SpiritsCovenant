using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SpiritsCovenant
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject enemy;
        [SerializeField] private Slider playerHealth;
        [SerializeField] private Slider enemyHealth;
        [SerializeField] private GameManager_Battle manager;
        [SerializeField] private Button attackButton;
        [SerializeField] public Animator anim;
        
        [System.Serializable]
        public class Skills
        {
            public string name = "Spirit Pulse";
            public float damage = 2;
            public int cooldown = 0;
            [HideInInspector] public int currentCooldown = 0;
        }
        
        private Skills currentSkill = new Skills();
        private bool isPlayerTurn = true;

        void Start()
        {
            anim = GetComponent<Animator>();
            attackButton.onClick.AddListener(UseSkill);
            attackButton.gameObject.SetActive(false);
        }

        void Update()
        {
            if (playerHealth.value <= 0) manager.LoseScene();
            if (enemyHealth.value <= 0) manager.MapScene();
            
            attackButton.gameObject.SetActive(isPlayerTurn);
            attackButton.interactable = currentSkill.currentCooldown <= 0;
        }

        void UseSkill()
        {
            anim.SetTrigger("attackButton");
            if(currentSkill.currentCooldown > 0) return;
            
            enemyHealth.value -= currentSkill.damage;
            currentSkill.currentCooldown = currentSkill.cooldown;
            
            StartCoroutine(EnemyTurn());
        }

        IEnumerator EnemyTurn()
        {
            isPlayerTurn = false;
            anim.SetBool("isPlayerTurn", false);

            yield return new WaitForSeconds(0.5f);
            
            playerHealth.value -= 5;
            
            if(currentSkill.currentCooldown > 0)
                currentSkill.currentCooldown--;
            
            isPlayerTurn = true;
            anim.SetBool("isPlayerTurn", true);
        }
    }
}