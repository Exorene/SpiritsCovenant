using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

namespace SpiritsCovenant
{
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

    public class GameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject enemy;
        [SerializeField] private Slider playerHealth;
        [SerializeField] private Slider enemyHealth;
        [SerializeField] private GameManager_Battle battleManager; 

        [Header("Gameplay Settings")]
        [SerializeField] private float enemyAttackDamage = 3f;
        [SerializeField] private float playerMaxHealth = 100f;
        [SerializeField] private float enemyMaxHealth = 100f;
        [SerializeField] private float enemyScalingFactor = 1.2f;

        [Header("UI - Reward Screen")]
        [SerializeField] private GameObject rewardScreen;
        [SerializeField] private Button rewardButton1;
        [SerializeField] private Button rewardButton2;
        [SerializeField] private Button rewardButton3;
        [SerializeField] private TextMeshProUGUI rewardText1;
        [SerializeField] private TextMeshProUGUI rewardText2;
        [SerializeField] private TextMeshProUGUI rewardText3;

        [Header("UI - Skill Replacement Panel")]
        [SerializeField] private GameObject skillReplacementPanel;
        [SerializeField] private Button[] replaceButtons;  // assign exactly 4 buttons in Inspector
        [SerializeField] private TextMeshProUGUI[] replaceButtonTexts; // assign corresponding TMP texts

        [Header("UI - Skill Buttons")]
        [SerializeField] private Button skillButton1;
        [SerializeField] private Button skillButton2;
        [SerializeField] private Button skillButton3;
        [SerializeField] private Button skillButton4;

        private bool isPlayerTurn = true;
        private Animator anim;
        private bool rewardDisplayed = false;
        private bool enemyStunned = false;
        private Skill currentReward = null;

        [System.Serializable]
        public class Skill
        {
            public string skillName;
            public float damage;  // For Aqua Mend, treat as percentage heal (ie 5 means 5% heal)
            public int cooldown;
            [HideInInspector] public int currentCooldown;
            public string description;
        }

        [Header("All Possible Base Skills")]
        [SerializeField] private Skill[] allSkills;
        // order: 0 - Spirit Pulse, 1 - Fireball, 2 - Aqua Mend, 3 - Lightning Strike
        // Local copy of unlocked skills; synced w/ gamedata
        private List<Skill> unlockedSkills = new List<Skill>();

        void Start()
        {
            anim = GetComponent<Animator>();

            // Initialize player and enemy health to full
            playerHealth.maxValue = playerMaxHealth;
            playerHealth.value = playerMaxHealth;
             // enemyMaxHealth is the base HP for level 1; scale based on current level
            float scaleFactor = 1.0f + (GameData.currentLevel - 1) * 0.5f;  
            float scaledEnemyMaxHealth = enemyMaxHealth * scaleFactor;
            enemyHealth.maxValue = scaledEnemyMaxHealth;
            enemyHealth.value = scaledEnemyMaxHealth;
            float scaledEnemyAttack = enemyAttackDamage * scaleFactor;

            // If GameData already has skills, copy those skills; otherwise initialize with Spirit Pulse.
            if (GameData.unlockedSkills.Count > 0)
            {
                unlockedSkills = GameData.unlockedSkills;
            }
            else
            {
                unlockedSkills.Add(allSkills[0]);
                GameData.unlockedSkills = unlockedSkills;
            }

            // Setup skill buttons listeners.
            skillButton1.onClick.AddListener(() => OnSkillButtonClicked(0));
            skillButton2.onClick.AddListener(() => OnSkillButtonClicked(1));
            skillButton3.onClick.AddListener(() => OnSkillButtonClicked(2));
            skillButton4.onClick.AddListener(() => OnSkillButtonClicked(3));

            rewardScreen.SetActive(false);
            if (skillReplacementPanel != null)
                skillReplacementPanel.SetActive(false);
        }

        void Update()
        {
            if (playerHealth.value <= 0)
            {
                battleManager.LoseScene();
            }
            else if (enemyHealth.value <= 0 && !rewardDisplayed)
            {
                ShowRewardScreen();
            }
            RefreshSkillButtons();
        }

        void OnSkillButtonClicked(int skillIndex)
        {
            if (skillIndex >= 0 && skillIndex < unlockedSkills.Count)
                UseSkill(unlockedSkills[skillIndex]);
        }

        void UseSkill(Skill skill)
        {
            if (skill.currentCooldown > 0)
                return;

            anim.SetTrigger("attackButton");

            // For Aqua Mend, heal instead of damaging enemy.
            if (skill.skillName == "Aqua Mend")
            {
                float healAmount = playerMaxHealth * (skill.damage / 100f);
                playerHealth.value = Mathf.Min(playerHealth.value + healAmount, playerMaxHealth);
            }
            else if (skill.skillName == "Lightning Strike")
            {
                //Stun chance of lightning strike
                float percentDamage = skill.damage;
                enemyHealth.value -= enemyHealth.maxValue * (percentDamage / 100f);
                float stunChance = 0f;
                if (Mathf.Approximately(skill.damage, 3f) || Mathf.Approximately(skill.damage, 5f))
                    stunChance = 20f;
                else if (Mathf.Approximately(skill.damage, 7f) || Mathf.Approximately(skill.damage, 9f))
                    stunChance = 30f;
                else if (Mathf.Approximately(skill.damage, 12f))
                    stunChance = 50f;
                float roll = Random.Range(0f, 100f);
                if (roll < stunChance)
                    enemyStunned = true;
            }
            else
            {
                enemyHealth.value -= skill.damage;
            }

            skill.currentCooldown = skill.cooldown;
            HideAllSkillButtons();
            StartCoroutine(EnemyTurn());
        }

        IEnumerator EnemyTurn()
        {
            isPlayerTurn = false;
            anim.SetBool("isPlayerTurn", false);
            yield return new WaitForSeconds(0.5f);

            if (enemyHealth.value > 0 && !enemyStunned)
            {
                //if enemy stunned, deal 0 damage
                float scaledEnemyDamage = enemyAttackDamage * Mathf.Pow(enemyScalingFactor, GameData.currentLevel - 1);
                playerHealth.value -= scaledEnemyDamage;
            }
            enemyStunned = false;

            foreach (var s in unlockedSkills)
            {
                if (s.currentCooldown > 0)
                    s.currentCooldown--;
            }
            isPlayerTurn = true;
            anim.SetBool("isPlayerTurn", true);
        }

        bool IsSkillAvailable(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= unlockedSkills.Count)
                return false;
            return unlockedSkills[skillIndex].currentCooldown <= 0;
        }

        void RefreshSkillButtons()
        {
            Button[] buttons = new Button[] { skillButton1, skillButton2, skillButton3, skillButton4 };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (isPlayerTurn && i < unlockedSkills.Count)
                {
                    buttons[i].gameObject.SetActive(true);
                    TextMeshProUGUI btnText = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                        btnText.text = unlockedSkills[i].skillName;
                    buttons[i].interactable = IsSkillAvailable(i);
                    buttons[i].image.color = GetRarityColor(unlockedSkills[i]);
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }
        }

        void HideAllSkillButtons()
        {
            Button[] buttons = new Button[] { skillButton1, skillButton2, skillButton3, skillButton4 };
            foreach (Button btn in buttons)
                btn.gameObject.SetActive(false);
        }

        void ShowRewardScreen()
        {
            rewardScreen.SetActive(true);
            rewardDisplayed = true;

            // Generate three random rewards.
            Skill reward1 = GenerateRandomReward();
            Skill reward2 = GenerateRandomReward();
            Skill reward3 = GenerateRandomReward();

            rewardButton1.onClick.RemoveAllListeners();
            rewardButton2.onClick.RemoveAllListeners();
            rewardButton3.onClick.RemoveAllListeners();

            rewardText1.text = $"{reward1.skillName} ({GetRarityString(reward1)})\n{reward1.description}";
            rewardText2.text = $"{reward2.skillName} ({GetRarityString(reward2)})\n{reward2.description}";
            rewardText3.text = $"{reward3.skillName} ({GetRarityString(reward3)})\n{reward3.description}";
            rewardButton1.image.color = GetRarityColor(reward1);
            rewardText1.color = Color.white;
            rewardButton2.image.color = GetRarityColor(reward2);
            rewardText2.color = Color.white;
            rewardButton3.image.color = GetRarityColor(reward3);
            rewardText3.color = Color.white;

            // When a reward button is clicked, call ConfirmRewardSelection
            rewardButton1.onClick.AddListener(() => ConfirmRewardSelection(reward1));
            rewardButton2.onClick.AddListener(() => ConfirmRewardSelection(reward2));
            rewardButton3.onClick.AddListener(() => ConfirmRewardSelection(reward3));
        }

        // Called when a reward is chosen
        void ConfirmRewardSelection(Skill reward)
        {
            currentReward = reward;
            // If player has fewer than 4 skills, add without replacement.
            if (unlockedSkills.Count < 4)
            {
                unlockedSkills.Add(reward);
                GameData.unlockedSkills = unlockedSkills;
                LoadMapScene();
            }
            else
            {
                // else, show the replacement UI so the player can choose which skill to replace.
                ShowSkillReplacementPanel();
            }
        }

        // Show a panel listing the four unlocked skills so the player can choose one to replace.
        void ShowSkillReplacementPanel()
        {
            if (skillReplacementPanel == null) return;
            skillReplacementPanel.SetActive(true);
            for (int i = 0; i < replaceButtons.Length; i++)
            {
                replaceButtons[i].gameObject.SetActive(true);
                if (i < unlockedSkills.Count)
                {
                    if (i < replaceButtonTexts.Length && replaceButtonTexts[i] != null)
                        replaceButtonTexts[i].text = unlockedSkills[i].skillName;
                    int index = i;
                    replaceButtons[i].onClick.RemoveAllListeners();
                    replaceButtons[i].onClick.AddListener(() => ReplaceSkill(index));
                    replaceButtons[i].interactable = true;
                }
                else
                {
                    if (i < replaceButtonTexts.Length && replaceButtonTexts[i] != null)
                        replaceButtonTexts[i].text = "Empty";
                    replaceButtons[i].onClick.RemoveAllListeners();
                    replaceButtons[i].interactable = false;
                }
            }
        }

        // Replace the skill with the one selected
        void ReplaceSkill(int index)
        {
            unlockedSkills[index] = currentReward;
            GameData.unlockedSkills = unlockedSkills;
            skillReplacementPanel.SetActive(false);
            LoadMapScene();
        }

        // Load the map scene and increment current level.
        void LoadMapScene()
        {
            GameData.currentLevel++;
            battleManager.MapScene();
        }

        Skill GenerateRandomReward()
        {
            int skillIndex = Random.Range(0, allSkills.Length);
            int rarityValue = Random.Range(0, 5); // 0:Common, 1:Uncommon, 2:Rare, 3:Epic, 4:Legendary
            Rarity rarity = (Rarity)rarityValue;
            Skill baseSkill = allSkills[skillIndex];
            Skill reward = new Skill();
            reward.skillName = baseSkill.skillName;
            switch (reward.skillName)
            {
                case "Spirit Pulse":
                    switch (rarity)
                    {
                        case Rarity.Common:
                            reward.damage = 2; reward.cooldown = 0;
                            reward.description = "Shoots a small orb of energy. Deals 2 damage."; break;
                        case Rarity.Uncommon:
                            reward.damage = 3; reward.cooldown = 0;
                            reward.description = "Shoots a small orb of energy. Deals 3 damage."; break;
                        case Rarity.Rare:
                            reward.damage = 4; reward.cooldown = 0;
                            reward.description = "Shoots a small orb of energy. Deals 4 damage."; break;
                        case Rarity.Epic:
                            reward.damage = 5; reward.cooldown = 0;
                            reward.description = "Shoots a small orb of energy. Deals 5 damage."; break;
                        case Rarity.Legendary:
                            reward.damage = 90; reward.cooldown = 3;
                            reward.description = "Shoots a MASSIVE orb of conecentrated energy. Deals 90 damage."; break;
                    }
                    break;
                case "Fireball":
                    switch (rarity)
                    {
                        case Rarity.Common:
                            reward.damage = 5; reward.cooldown = 0;
                            reward.description = "Hurls a fireball. Deals 5 damage."; break;
                        case Rarity.Uncommon:
                            reward.damage = 10; reward.cooldown = 0;
                            reward.description = "Hurls a fireball. Deals 10 damage."; break;
                        case Rarity.Rare:
                            reward.damage = 15; reward.cooldown = 0;
                            reward.description = "Hurls a fireball. Deals 15 damage."; break;
                        case Rarity.Epic:
                            reward.damage = 20; reward.cooldown = 1;
                            reward.description = "Hurls a fireball. Deals 20 damage."; break;
                        case Rarity.Legendary:
                            reward.damage = 25; reward.cooldown = 1;
                            reward.description = "Hurls a fireball. Deals 25 damage."; break;
                    }
                    break;
                case "Aqua Mend":
                    switch (rarity)
                    {
                        case Rarity.Common:
                            reward.damage = 5; reward.cooldown = 0;
                            reward.description = "Heals 5% health."; break;
                        case Rarity.Uncommon:
                            reward.damage = 10; reward.cooldown = 0;
                            reward.description = "Heals 10% health."; break;
                        case Rarity.Rare:
                            reward.damage = 15; reward.cooldown = 1;
                            reward.description = "Heals 15% health."; break;
                        case Rarity.Epic:
                            reward.damage = 20; reward.cooldown = 2;
                            reward.description = "Heals 20% health."; break;
                        case Rarity.Legendary:
                            reward.damage = 25; reward.cooldown = 2;
                            reward.description = "Heals 25% health."; break;
                    }
                    break;
                case "Lightning Strike":
                    switch (rarity)
                    {
                        case Rarity.Common:
                            reward.damage = 3; reward.cooldown = 1;
                            reward.description = "Strikes with lightning. Deals 5% damage, 20% chance to stun."; break;
                        case Rarity.Uncommon:
                            reward.damage = 5; reward.cooldown = 1;
                            reward.description = "Strikes with lightning. Deals 10% damage, 20% chance to stun."; break;
                        case Rarity.Rare:
                            reward.damage = 7; reward.cooldown = 2;
                            reward.description = "Strikes with lightning. Deals 15% damage, 30% chance to stun."; break;
                        case Rarity.Epic:
                            reward.damage = 9; reward.cooldown = 2;
                            reward.description = "Strikes with lightning. Deals 20% damage, 30% chance to stun."; break;
                        case Rarity.Legendary:
                            reward.damage = 12; reward.cooldown = 3;
                            reward.description = "Strikes with lightning. Deals 25% damage, 50% chance to stun."; break;
                    }
                    break;
            }
            reward.currentCooldown = 0;
            return reward;
        }

        string GetRarityString(Skill skill)
        {
            if (skill.skillName == "Spirit Pulse")
            {
                if (skill.damage == 2) return "Common";
                if (skill.damage == 3) return "Uncommon";
                if (skill.damage == 4) return "Rare";
                if (skill.damage == 5) return "Epic";
                if (skill.damage == 90) return "Legendary";
            }
            else if (skill.skillName == "Fireball")
            {
                if (skill.damage == 5) return "Common";
                if (skill.damage == 10) return "Uncommon";
                if (skill.damage == 15) return "Rare";
                if (skill.damage == 20) return "Epic";
                if (skill.damage == 25) return "Legendary";
            }
            else if (skill.skillName == "Aqua Mend")
            {
                if (skill.damage == 5) return "Common";
                if (skill.damage == 10) return "Uncommon";
                if (skill.damage == 15) return "Rare";
                if (skill.damage == 20) return "Epic";
                if (skill.damage == 25) return "Legendary";
            }
            else if (skill.skillName == "Lightning Strike")
            {
                if (skill.damage == 5) return "Common";
                if (skill.damage == 10) return "Uncommon";
                if (skill.damage == 15) return "Rare";
                if (skill.damage == 20) return "Epic";
                if (skill.damage == 25) return "Legendary";
            }
            return "";
        }

        private Color GetRarityColor(Skill skill)
        {
            string rarity = GetRarityString(skill);
            switch(rarity)
            {
                case "Common": return Color.white;
                case "Uncommon": return Color.green;
                case "Rare": return Color.blue;
                case "Epic": return new Color(0.5f, 0f, 0.5f, 1f);
                case "Legendary": return new Color(1f, 0.5f, 0f, 1f);
                default: return Color.white;
            }
        }
    }
}
