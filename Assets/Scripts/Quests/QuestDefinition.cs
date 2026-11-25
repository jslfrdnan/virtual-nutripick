using UnityEngine;

namespace NutriPick.Quests
{
    [CreateAssetMenu(menuName = "NutriPick/Quest Definition", fileName = "QD_NewQuest")]
    public class QuestDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public string ageRange;
        [TextArea] public string description;

        [Header("Group")]
        public QuestGroup group;

        [Header("Daily AKG")]
        public AkgProfile dailyAkg = new AkgProfile();

        [Header("Tasks per level")]
        public QuestLevelTasks[] levels = new QuestLevelTasks[3];
    }
}
