using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SpiritsCovenant
{
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

    [System.Serializable]
    public struct RarityWeight
    {
        public int common;
        public int uncommon;
        public int rare;
        public int epic;
        public int legendary;
    }

    public class GameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject enemy;
        [SerializeField] private GameObject bossEnemyPrefab;
        [SerializeField] private Slider playerHealth;
        [SerializeField] private Slider enemyHealth;
        [SerializeField] private GameManager_Battle battleManager;

        [Header("Stats & Scaling (edit only in Inspector)")]
        [SerializeField] private float playerMaxHealth    = 120f;
        [SerializeField] private float enemyMaxHealth     = 12f;
        [SerializeField] private float enemyAttackDamage  = 2f;
        [SerializeField] private float enemyScalingFactor = 1.05f;

        [Header("Drop Chances by Level")]
        [Tooltip("one entry per level, in order")]
        [SerializeField] private RarityWeight[] rarityChances = new RarityWeight[10]
        {
            new RarityWeight{ common=60, uncommon=30, rare=7,  epic=2, legendary=1 },
            new RarityWeight{ common=55, uncommon=30, rare=8,  epic=4, legendary=3 },
            new RarityWeight{ common=50, uncommon=28, rare=10, epic=7, legendary=5 },
            new RarityWeight{ common=45, uncommon=25, rare=12, epic=10,legendary=8 },
            new RarityWeight{ common=40, uncommon=25, rare=15, epic=12,legendary=8 },
            new RarityWeight{ common=35, uncommon=25, rare=18, epic=15,legendary=7 },
            new RarityWeight{ common=30, uncommon=25, rare=20, epic=18,legendary=7 },
            new RarityWeight{ common=25, uncommon=25, rare=22, epic=20,legendary=8 },
            new RarityWeight{ common=20, uncommon=25, rare=25, epic=20,legendary=10},
            new RarityWeight{ common=15, uncommon=25, rare=25, epic=25,legendary=10}
        };

        [Header("UI - Reward Screen")]
        [SerializeField] private GameObject rewardScreen;
        [SerializeField] private Button rewardButton1;
        [SerializeField] private Button rewardButton2;
        [SerializeField] private Button rewardButton3;
        [SerializeField] private TextMeshProUGUI rewardText1;
        [SerializeField] private TextMeshProUGUI rewardText2;
        [SerializeField] private TextMeshProUGUI rewardText3;
        [SerializeField] private Button skipRewardButton;

        [Header("UI - Skill Replacement")]
        [SerializeField] private GameObject skillReplacementPanel;
        [SerializeField] private Button[] replaceButtons;
        [SerializeField] private TextMeshProUGUI[] replaceButtonTexts;

        [Header("UI - Skill Buttons")]
        [SerializeField] private Button skillButton1;
        [SerializeField] private Button skillButton2;
        [SerializeField] private Button skillButton3;
        [SerializeField] private Button skillButton4;

        private float enemyDebuffPercent;
        private int   enemyDebuffTurns;
        private float playerBuffPercent;
        private int   playerBuffTurns;

        private bool isPlayerTurn     = true;
        private Animator anim;
        private bool rewardDisplayed  = false;
        private bool enemyStunned     = false;
        private Skill currentReward   = null;

        [System.Serializable]
        public class Skill
        {
            public string   skillName;
            public Rarity   rarity;
            public float    damage;
            public int      duration;
            public int      cooldown;
            [HideInInspector] public int currentCooldown;
            public string   description;
        }

        [Header("All Possible Base Skills")]
        [SerializeField] private Skill[] allSkills;
        private List<Skill> unlockedSkills = new List<Skill>();

        void Start()
        {
            if (GameData.currentLevel >= 10 && bossEnemyPrefab != null)
            {
                var boss = Instantiate(
                    bossEnemyPrefab,
                    enemy.transform.position,
                    Quaternion.Euler(0f, 270f, 0f),
                    enemy.transform.parent
                );
                Destroy(enemy);
                enemy = boss;
            }

            anim = GetComponent<Animator>();

            playerHealth.maxValue = playerMaxHealth;
            playerHealth.value    = playerMaxHealth;

            float scaleFactor       = Mathf.Pow(enemyScalingFactor, GameData.currentLevel - 1);
            float scaledEnemyHealth = enemyMaxHealth * scaleFactor;
            enemyHealth.maxValue    = scaledEnemyHealth;
            enemyHealth.value       = scaledEnemyHealth;

            if (GameData.unlockedSkills.Count > 0)
                unlockedSkills = GameData.unlockedSkills;
            else
            {
                unlockedSkills.Add(allSkills[0]);
                GameData.unlockedSkills = unlockedSkills;
            }

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
                battleManager.LoseScene();
            else if (enemyHealth.value <= 0 && !rewardDisplayed)
                ShowRewardScreen();

            RefreshSkillButtons();
        }

        void OnSkillButtonClicked(int index)
        {
            if (index < unlockedSkills.Count)
                UseSkill(unlockedSkills[index]);
        }

        void UseSkill(Skill skill)
        {
            if (skill.currentCooldown > 0) return;

            anim.SetTrigger("attackButton");

            if (skill.skillName == "Entrap")
            {
                enemyDebuffPercent = skill.damage / 100f;
                enemyDebuffTurns   = skill.duration;
            }
            else if (skill.skillName == "Earth Shell")
            {
                playerBuffPercent = skill.damage / 100f;
                playerBuffTurns   = skill.duration;
            }
            else if (skill.skillName == "Aqua Mend")
            {
                float heal = playerMaxHealth * (skill.damage / 100f);
                playerHealth.value = Mathf.Min(playerHealth.value + heal, playerMaxHealth);
            }
            else if (skill.skillName == "Lightning Strike")
            {
                float raw = enemyHealth.maxValue * (skill.damage / 100f);
                enemyHealth.value -= raw * (1f + enemyDebuffPercent);

                float stunChance = skill.damage switch
                {
                    5f or 10f  => 20f,
                    15f or 20f => 30f,
                    25f        => 50f,
                    _          => 0f
                };

                if (Random.value * 100f < stunChance)
                    enemyStunned = true;
            }
            else
            {
                float raw = skill.damage;
                enemyHealth.value -= raw * (1f + enemyDebuffPercent);
            }

            skill.currentCooldown = skill.cooldown;
            StartCoroutine(EnemyTurn());
        }

        IEnumerator EnemyTurn()
        {
            isPlayerTurn = false;
            anim.SetBool("isPlayerTurn", false);

            yield return new WaitForSeconds(0.5f);

            if (enemyHealth.value > 0 && !enemyStunned)
            {
                float raw     = enemyAttackDamage * Mathf.Pow(enemyScalingFactor, GameData.currentLevel - 1);
                float damage  = raw * (1f - playerBuffPercent);
                playerHealth.value -= damage;
            }

            enemyStunned = false;

            if (enemyDebuffTurns > 0) enemyDebuffTurns--;
            if (enemyDebuffTurns == 0) enemyDebuffPercent = 0f;

            if (playerBuffTurns > 0) playerBuffTurns--;
            if (playerBuffTurns == 0) playerBuffPercent = 0f;

            foreach (var s in unlockedSkills)
                if (s.currentCooldown > 0) s.currentCooldown--;

            isPlayerTurn = true;
            anim.SetBool("isPlayerTurn", true);
        }

        void RefreshSkillButtons()
        {
            Button[] buttons = { skillButton1, skillButton2, skillButton3, skillButton4 };

            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];

                if (isPlayerTurn && i < unlockedSkills.Count)
                {
                    button.gameObject.SetActive(true);

                    var txt = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt)
                    {
                        int cd = unlockedSkills[i].currentCooldown;
                        txt.text = unlockedSkills[i].skillName + (cd > 0 ? $" ({cd})" : "");
                        txt.color = Color.white;
                    }

                    button.interactable = unlockedSkills[i].currentCooldown <= 0;
                    button.image.color  = GetRarityColor(unlockedSkills[i]);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        void ShowRewardScreen()
        {
            rewardScreen.SetActive(true);
            rewardDisplayed = true;

            var r1 = GenerateRandomReward();
            var r2 = GenerateRandomReward();
            var r3 = GenerateRandomReward();

            rewardButton1.onClick.RemoveAllListeners();
            rewardButton2.onClick.RemoveAllListeners();
            rewardButton3.onClick.RemoveAllListeners();
            skipRewardButton.onClick.RemoveAllListeners();

            rewardText1.text  = $"{r1.skillName} ({r1.rarity})\n{r1.description}";
            rewardText1.color = Color.white;

            rewardText2.text  = $"{r2.skillName} ({r2.rarity})\n{r2.description}";
            rewardText2.color = Color.white;

            rewardText3.text  = $"{r3.skillName} ({r3.rarity})\n{r3.description}";
            rewardText3.color = Color.white;

            rewardButton1.image.color = GetRarityColor(r1);
            rewardButton2.image.color = GetRarityColor(r2);
            rewardButton3.image.color = GetRarityColor(r3);

            rewardButton1.onClick.AddListener(() => ConfirmRewardSelection(r1));
            rewardButton2.onClick.AddListener(() => ConfirmRewardSelection(r2));
            rewardButton3.onClick.AddListener(() => ConfirmRewardSelection(r3));
            skipRewardButton.onClick.AddListener(SkipReward);
        }

        void ConfirmRewardSelection(Skill reward)
        {
            currentReward = reward;

            if (unlockedSkills.Count < 4)
            {
                unlockedSkills.Add(reward);
                GameData.unlockedSkills = unlockedSkills;
                LoadMapScene();
            }
            else ShowSkillReplacementPanel();
        }

        void ShowSkillReplacementPanel()
        {
            skillReplacementPanel.SetActive(true);

            for (int i = 0; i < replaceButtons.Length; i++)
            {
                replaceButtons[i].gameObject.SetActive(true);

                if (i < unlockedSkills.Count)
                {
                    replaceButtonTexts[i].text    = unlockedSkills[i].skillName;
                    replaceButtons[i].image.color = GetRarityColor(unlockedSkills[i]);

                    int buttonIndex = i;
                    replaceButtons[i].onClick.RemoveAllListeners();
                    replaceButtons[i].onClick.AddListener(() => ReplaceSkill(buttonIndex));
                    replaceButtons[i].interactable = true;
                }
                else
                {
                    replaceButtonTexts[i].text = "Empty";
                    replaceButtons[i].onClick.RemoveAllListeners();
                    replaceButtons[i].interactable = false;
                }
            }
        }

        void ReplaceSkill(int index)
        {
            unlockedSkills[index] = currentReward;
            GameData.unlockedSkills = unlockedSkills;
            skillReplacementPanel.SetActive(false);
            LoadMapScene();
        }

        void SkipReward()
        {
            LoadMapScene();
        }

        void LoadMapScene()
        {
            GameData.currentLevel++;
            battleManager.MapScene();
        }

        Skill GenerateRandomReward()
        {
            int levelIndex = Mathf.Clamp(GameData.currentLevel, 1, rarityChances.Length) - 1;
            var weights = rarityChances[levelIndex];

            int roll = Random.Range(1, 101);
            int cumulativeChance = weights.common;
            Rarity rarity = Rarity.Common;
            if (roll <= cumulativeChance)                             { rarity = Rarity.Common; }
            else if (roll <= (cumulativeChance += weights.uncommon))  { rarity = Rarity.Uncommon; }
            else if (roll <= (cumulativeChance += weights.rare))      { rarity = Rarity.Rare; }
            else if (roll <= (cumulativeChance += weights.epic))      { rarity = Rarity.Epic; }
            else                                                      { rarity = Rarity.Legendary; }

            var baseSkill = allSkills[Random.Range(0, allSkills.Length)];
            var reward = new Skill
            {
                skillName       = baseSkill.skillName,
                rarity          = rarity,
                currentCooldown = 0
            };

            switch (reward.skillName)
            {
                case "Spirit Pulse":
                    if (rarity == Rarity.Common)        { reward.damage = 2;  reward.cooldown = 0; reward.duration = 0; reward.description = "Shoots a small orb of energy. Deals 2 damage."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 3;  reward.cooldown = 0; reward.duration = 0; reward.description = "Shoots a small orb of energy. Deals 3 damage."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 4;  reward.cooldown = 0; reward.duration = 0; reward.description = "Shoots a small orb of energy. Deals 4 damage."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 5;  reward.cooldown = 0; reward.duration = 0; reward.description = "Shoots a concentrated orb. Deals 5 damage."; }
                    else                                { reward.damage = 30; reward.cooldown = 3; reward.duration = 0; reward.description = "Shoots a massive orb. Deals 30 damage."; }
                    break;
                case "Fireball":
                    if (rarity == Rarity.Common)        { reward.damage = 5;  reward.cooldown = 0; reward.duration = 0; reward.description = "Hurls a fireball. Deals 5 damage."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 0; reward.duration = 0; reward.description = "Hurls a fireball. Deals 10 damage."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 15; reward.cooldown = 0; reward.duration = 0; reward.description = "Hurls a fireball. Deals 15 damage."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 0; reward.description = "Hurls a fireball. Deals 20 damage."; }
                    else                                { reward.damage = 25; reward.cooldown = 2; reward.duration = 0; reward.description = "Hurls a massive fireball. Deals 25 damage."; }
                    break;
                case "Aqua Mend":
                    if (rarity == Rarity.Common)        { reward.damage = 5;  reward.cooldown = 0; reward.duration = 0; reward.description = "Heals 5% health."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 0; reward.duration = 0; reward.description = "Heals 10% health."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 15; reward.cooldown = 1; reward.duration = 0; reward.description = "Heals 15% health."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 0; reward.description = "Heals 20% health."; }
                    else                                { reward.damage = 25; reward.cooldown = 2; reward.duration = 0; reward.description = "Heals 25% health."; }
                    break;
                case "Lightning Strike":
                    if (rarity == Rarity.Common)        { reward.damage = 5;  reward.cooldown = 1; reward.duration = 0; reward.description = "Strikes with lightning. Deals 5% damage, 20% chance to stun."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 1; reward.duration = 0; reward.description = "Strikes with lightning. Deals 10% damage, 20% chance to stun."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 15; reward.cooldown = 2; reward.duration = 0; reward.description = "Strikes with lightning. Deals 15% damage, 30% chance to stun."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 0; reward.description = "Strikes with lightning. Deals 20% damage, 30% chance to stun."; }
                    else                                { reward.damage = 25; reward.cooldown = 3; reward.duration = 0; reward.description = "Strikes with devastating lightning. Deals 25% damage, 50% chance to stun."; }
                    break;
                case "Entrap":
                    if (rarity == Rarity.Common)        { reward.damage = 10; reward.cooldown = 1; reward.duration = 1; reward.description = "Roots the foe: Defense Down 10% for 1 turn."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 1; reward.duration = 2; reward.description = "Roots the foe: Defense Down 10% for 2 turns."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 2; reward.description = "Roots the foe: Defense Down 20% for 2 turns."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 3; reward.description = "Roots the foe: Defense Down 20% for 3 turns."; }
                    else                                { reward.damage = 30; reward.cooldown = 3; reward.duration = 3; reward.description = "Roots the foe: Defense Down 30% for 3 turns."; }
                    break;
                case "Earth Shell":
                    if (rarity == Rarity.Common)        { reward.damage = 10; reward.cooldown = 1; reward.duration = 1; reward.description = "Shields you: Defense Up 10% for 1 turn."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 1; reward.duration = 2; reward.description = "Shields you: Defense Up 10% for 2 turns."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 2; reward.description = "Shields you: Defense Up 20% for 2 turns."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 3; reward.description = "Shields you: Defense Up 20% for 3 turns."; }
                    else                                { reward.damage = 30; reward.cooldown = 3; reward.duration = 3; reward.description = "Shields you: Defense Up 30% for 3 turns."; }
                    break;
            }

            return reward;
        }

        private Color GetRarityColor(Skill skill)
        {
            return skill.rarity switch
            {
                Rarity.Common    => Color.gray,
                Rarity.Uncommon  => new Color(0f, 0.6f, 0f, 1f),
                Rarity.Rare      => Color.blue,
                Rarity.Epic      => new Color(0.5f, 0f, 0.5f, 1f),
                Rarity.Legendary => new Color(1f, 0.5f, 0f, 1f),
                _                => Color.white
            };
        }
    }
}
