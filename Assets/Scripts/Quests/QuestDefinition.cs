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

        [Header("Plate Rules")]
        [Tooltip("Minimum foods required on the serving plate. 0 uses the level's minItems.")]
        public int plateMinItems;

        [Tooltip("Visual scale of the serving plate. 1 = normal size.")]
        [Range(0.5f, 1.5f)] public float plateScale = 1f;

        [Header("Daily AKG")]
        public AkgProfile dailyAkg = new AkgProfile();

        [Header("Tasks per level")]
        public QuestLevelTasks[] levels = new QuestLevelTasks[3];
    }
}
