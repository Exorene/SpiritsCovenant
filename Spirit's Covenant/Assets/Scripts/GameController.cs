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

    [System.Serializable]
    public struct SkillParticle
    {
        public string skillName;
        public ParticleSystem particlePrefab;
        public bool playOnPlayer;
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

        [Header("VFX")]
        [SerializeField] private SkillParticle[] skillParticles = new SkillParticle[6];

        [Header("Stats & Scaling")]
        [SerializeField] private float playerMaxHealth = 120f;
        [SerializeField] private float enemyMaxHealth = 12f;
        [SerializeField] private float enemyAttackDamage = 2f;
        [SerializeField] private float enemyScalingFactor = 1.05f;

        [Header("Drop Chances by Level")]
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
        private int enemyDebuffTurns;
        private float playerBuffPercent;
        private int playerBuffTurns;
        private bool isPlayerTurn = true;
        private Animator anim;
        private bool rewardDisplayed = false;
        private bool enemyStunned = false;
        private Skill currentReward = null;

        [System.Serializable]
        public class Skill
        {
            public string skillName;
            public Rarity rarity;
            public float damage;
            public int duration;
            public int cooldown;
            [HideInInspector] public int currentCooldown;
            public string description;
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
            playerHealth.value = playerMaxHealth;

            float scaleFactor = Mathf.Pow(enemyScalingFactor, GameData.currentLevel - 1);
            float scaledEnemyHealth = enemyMaxHealth * scaleFactor;
            enemyHealth.maxValue = scaledEnemyHealth;
            enemyHealth.value = scaledEnemyHealth;

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
            PlaySkillVFX(skill.skillName);

            if (skill.skillName == "Entrap")
            {
                enemyDebuffPercent = skill.damage / 100f;
                enemyDebuffTurns = skill.duration;
            }
            else if (skill.skillName == "Earth Shell")
            {
                playerBuffPercent = skill.damage / 100f;
                playerBuffTurns = skill.duration;
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
                    5f or 10f => 20f,
                    15f or 20f => 30f,
                    25f => 50f,
                    _ => 0f
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

            rewardText1.text = $"{r1.skillName} ({r1.rarity})\n{r1.description}";
            rewardText2.text = $"{r2.skillName} ({r2.rarity})\n{r2.description}";
            rewardText3.text = $"{r3.skillName} ({r3.rarity})\n{r3.description}";

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
            else
            {
                ShowSkillReplacementPanel();
            }
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
                    int btnIndex = i;
                    replaceButtons[i].onClick.RemoveAllListeners();
                    replaceButtons[i].onClick.AddListener(() => ReplaceSkill(btnIndex));
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
            int cum = weights.common;
            Rarity rarity = roll <= cum
                ? Rarity.Common
                : roll <= (cum += weights.uncommon)
                    ? Rarity.Uncommon
                    : roll <= (cum += weights.rare)
                        ? Rarity.Rare
                        : roll <= (cum += weights.epic)
                            ? Rarity.Epic
                            : Rarity.Legendary;

            var baseSkill = allSkills[Random.Range(0, allSkills.Length)];
            var reward = new Skill { skillName = baseSkill.skillName, rarity = rarity, currentCooldown = 0 };

            switch (reward.skillName)
            {
                case "Spirit Pulse":
                    if (rarity == Rarity.Common)      { reward.damage = 2;  reward.cooldown = 0; reward.duration = 0; reward.description = "Deals 2 damage."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 3;  reward.cooldown = 0; reward.duration = 0; reward.description = "Deals 3 damage."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 4;  reward.cooldown = 0; reward.duration = 0; reward.description = "Deals 4 damage."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 5;  reward.cooldown = 0; reward.duration = 0; reward.description = "Deals 5 damage."; }
                    else                                { reward.damage = 30; reward.cooldown = 3; reward.duration = 0; reward.description = "Deals 30 damage."; }
                    break;
                case "Fireball":
                    if (rarity == Rarity.Common)      { reward.damage = 5;  reward.cooldown = 0; reward.duration = 0; reward.description = "Deals 5 damage."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 0; reward.duration = 0; reward.description = "Deals 10 damage."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 15; reward.cooldown = 0; reward.duration = 0; reward.description = "Deals 15 damage."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 0; reward.description = "Deals 20 damage."; }
                    else                                { reward.damage = 25; reward.cooldown = 2; reward.duration = 0; reward.description = "Deals 25 damage."; }
                    break;
                case "Aqua Mend":
                    if (rarity == Rarity.Common)      { reward.damage = 5;  reward.cooldown = 0; reward.duration = 0; reward.description = "Heals 5%."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 0; reward.duration = 0; reward.description = "Heals 10%."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 15; reward.cooldown = 1; reward.duration = 0; reward.description = "Heals 15%."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 0; reward.description = "Heals 20%."; }
                    else                                { reward.damage = 25; reward.cooldown = 2; reward.duration = 0; reward.description = "Heals 25%."; }
                    break;
                case "Lightning Strike":
                    if (rarity == Rarity.Common)      { reward.damage = 5;  reward.cooldown = 1; reward.duration = 0; reward.description = "Deals 5%, 20% stun."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 1; reward.duration = 0; reward.description = "Deals 10%, 20% stun."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 15; reward.cooldown = 2; reward.duration = 0; reward.description = "Deals 15%, 30% stun."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 0; reward.description = "Deals 20%, 30% stun."; }
                    else                                { reward.damage = 25; reward.cooldown = 3; reward.duration = 0; reward.description = "Deals 25%, 50% stun."; }
                    break;
                case "Entrap":
                    if (rarity == Rarity.Common)      { reward.damage = 10; reward.cooldown = 1; reward.duration = 1; reward.description = "Defense Down 10% for 1 turn."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 1; reward.duration = 2; reward.description = "Defense Down 10% for 2 turns."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 2; reward.description = "Defense Down 20% for 2 turns."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 3; reward.description = "Defense Down 20% for 3 turns."; }
                    else                                { reward.damage = 30; reward.cooldown = 3; reward.duration = 3; reward.description = "Defense Down 30% for 3 turns."; }
                    break;
                case "Earth Shell":
                    if (rarity == Rarity.Common)      { reward.damage = 10; reward.cooldown = 1; reward.duration = 1; reward.description = "Defense Up 10% for 1 turn."; }
                    else if (rarity == Rarity.Uncommon){ reward.damage = 10; reward.cooldown = 1; reward.duration = 2; reward.description = "Defense Up 10% for 2 turns."; }
                    else if (rarity == Rarity.Rare)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 2; reward.description = "Defense Up 20% for 2 turns."; }
                    else if (rarity == Rarity.Epic)    { reward.damage = 20; reward.cooldown = 2; reward.duration = 3; reward.description = "Defense Up 20% for 3 turns."; }
                    else                                { reward.damage = 30; reward.cooldown = 3; reward.duration = 3; reward.description = "Defense Up 30% for 3 turns."; }
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

        private void PlaySkillVFX(string skillName)
        {
            foreach (var entry in skillParticles)
            {
                if (entry.skillName == skillName && entry.particlePrefab != null)
                {
                    Transform spawnPoint = entry.playOnPlayer ? player.transform : enemy.transform;
                    ParticleSystem instance = Instantiate(entry.particlePrefab, spawnPoint.position, Quaternion.identity);
                    instance.Play();
                    Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
                    return;
                }
            }
        }
    }
}
