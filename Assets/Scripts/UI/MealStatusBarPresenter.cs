using System.Text;
using TMPro;
using UnityEngine;
using NutriPick.Quests;

namespace NutriPick.UI
{
    public class MealStatusBarPresenter : MonoBehaviour
    {
        public TMP_Text statusLabel;
        public TMP_Text countsLabel;
        public Color neutralColor = new Color(0.62f, 0.62f, 0.62f);
        public Color doneColor = new Color(0.18f, 0.49f, 0.20f);

        public void SetNeutral(QuestDefinition quest, int levelIndex)
        {
            if (statusLabel != null)
            {
                statusLabel.text = Header(quest, levelIndex);
                statusLabel.color = neutralColor;
            }

            if (countsLabel != null)
            {
                countsLabel.text = quest != null
                    ? "PROTEIN [ ]  BUAH/SAYUR [ ]  GIZI [ ]  GIZI [ ]"
                    : "PILIH MAKANAN";
                countsLabel.color = neutralColor;
            }
        }

        public void SetEvaluation(QuestEvaluation evaluation)
        {
            if (evaluation == null)
                return;

            bool allPassed = evaluation.AllPassed;
            if (statusLabel != null)
            {
                statusLabel.text = Header(evaluation.quest, evaluation.levelIndex);
                statusLabel.color = allPassed ? doneColor : neutralColor;
            }

            if (countsLabel == null)
                return;

            var builder = new StringBuilder();
            foreach (var task in evaluation.tasks)
            {
                if (builder.Length > 0)
                    builder.Append("  ");
                builder.Append(task.ProgressLabel());
            }

            countsLabel.text = builder.ToString();
            countsLabel.color = allPassed ? doneColor : neutralColor;
        }

        private static string Header(QuestDefinition quest, int levelIndex)
        {
            if (quest == null)
                return "QUEST";

            string level = quest.levels != null && levelIndex >= 1 && levelIndex <= quest.levels.Length
                ? quest.levels[levelIndex - 1].label
                : "LEVEL " + levelIndex;
            return quest.displayName.ToUpperInvariant() + " · " + level.ToUpperInvariant();
        }
    }
}
