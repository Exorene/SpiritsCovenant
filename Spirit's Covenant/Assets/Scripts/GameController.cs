using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SpiritsCovenant
{
    public class GameController : MonoBehaviour
    {
        Animator anim;
        [SerializeField] private GameObject player = null;
        [SerializeField] private GameObject enemy = null;
        [SerializeField] private Slider playerHealth = null;
        [SerializeField] private Slider enemyHealth = null;
        [SerializeField] GameManager_Battle manager;
        [System.Serializable]
        public class Skills
        {
            public string name;
            public float damage;
            public int cooldown;
            [HideInInspector] public int currentCooldown;
        }
        
        [SerializeField] private Skills[] currentSkills = new Skills[4];
        [SerializeField] private GameObject skillButtonPrefab;
        [SerializeField] private Transform skillsPanel;

        private bool isPlayerTurn = true;

        void Start()
        {
            anim = GetComponent<Animator>();
            anim.SetBool("Collected", false);
            
            currentSkills[0] = new Skills {
                name = "Spirit Pulse",
                damage = 2,
                cooldown = 0
            };
            
            CreateSkillButtons();
        }

        void CreateSkillButtons()
        {
            foreach(Transform child in skillsPanel)
            {
                Destroy(child.gameObject);
            }
            
            for(int i = 0; i < currentSkills.Length; i++)
            {
                if(currentSkills[i] == null) continue;
                
                GameObject btn = Instantiate(skillButtonPrefab, skillsPanel);
                btn.GetComponentInChildren<Text>().text = currentSkills[i].name;
                int index = i;
                btn.GetComponent<Button>().onClick.AddListener(() => UseSkill(index));
            }
        }

        void UseSkill(int skillIndex)
        {
            Skills skill = currentSkills[skillIndex];
            if(skill.currentCooldown > 0) return;
            
            enemyHealth.value -= skill.damage;
            skill.currentCooldown = skill.cooldown;
            ChangeTurn();
        }

        private void ChangeTurn()
        {
            isPlayerTurn = !isPlayerTurn;
            skillsPanel.gameObject.SetActive(isPlayerTurn);

            if (!isPlayerTurn)
            {
                StartCoroutine(EnemyTurn());
            }
            else
            {
                foreach(Skills skill in currentSkills)
                {
                    if(skill != null && skill.currentCooldown > 0)
                        skill.currentCooldown--;
                }
                CreateSkillButtons();
            }
        }

        private void Attack(GameObject target, float damage)
        {
            if (target == enemy)
            {
                enemyHealth.value -= damage;
            }
            else
            {
                playerHealth.value -= damage;
            }

            ChangeTurn();
        }

        private IEnumerator EnemyTurn()
        {
            yield return new WaitForSeconds(1.5f);
            Attack(player, 7);
        }
    }
}