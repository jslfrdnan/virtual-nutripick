using System.Linq;
using NutriPick.Flow;
using NutriPick.Quests;

namespace NutriPick.Tests
{
    public static class TestQuestUtility
    {
        public static QuestDefinition SelectQuest(LevelFlowController flow, string questId = "anak")
        {
            var quest = flow != null && flow.questOptions != null
                ? flow.questOptions.FirstOrDefault(option => option != null && option.id == questId)
                : null;

            if (quest != null)
                GameSession.Instance?.SelectQuest(quest);
            return quest;
        }

        public static void StartLevel(LevelFlowController flow, string questId = "anak")
        {
            SelectQuest(flow, questId);
            if (flow != null)
                flow.StartLevel();
        }
    }
}
