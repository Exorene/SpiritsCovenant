using System.Collections.Generic;

namespace SpiritsCovenant
{
    public static class GameData
    {
        public static int currentLevel = 1;
        public static List<GameController.Skill> unlockedSkills = new List<GameController.Skill>();

        public static void Reset()
        {
            currentLevel = 1;
            unlockedSkills.Clear();
        }
    }
}
