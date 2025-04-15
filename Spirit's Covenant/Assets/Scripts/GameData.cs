using System.Collections.Generic;

namespace SpiritsCovenant
{
    public static class GameData
    {
        public static int currentLevel = 1;
        // The player's unlocked skills
        public static List<GameController.Skill> unlockedSkills = new List<GameController.Skill>();
    }
}
