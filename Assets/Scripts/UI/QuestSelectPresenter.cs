using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NutriPick.Quests;

namespace NutriPick.UI
{
    public class QuestSelectPresenter : MonoBehaviour
    {
        public GameObject root;
        public Button[] questButtons;
        public TMP_Text[] questTitles;
        public TMP_Text[] questDetails;

        private QuestDefinition[] options;
        private Action<QuestDefinition> onSelected;

        public bool IsOpen => root != null && root.activeSelf;

        public void Show(QuestDefinition[] quests, Action<QuestDefinition> selected)
        {
            if (root == null)
                root = gameObject;

            options = quests;
            onSelected = selected;
            root.SetActive(true);

            int count = quests != null ? quests.Length : 0;
            for (int i = 0; i < questButtons.Length; i++)
            {
                var button = questButtons[i];
                if (button == null)
                    continue;

                bool active = i < count && quests[i] != null;
                button.gameObject.SetActive(active);
                if (!active)
                    continue;

                int index = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Select(index));

                if (questTitles != null && i < questTitles.Length && questTitles[i] != null)
                    questTitles[i].text = quests[i].displayName.ToUpperInvariant();
                if (questDetails != null && i < questDetails.Length && questDetails[i] != null)
                    questDetails[i].text = BuildDetails(quests[i]);
            }

            ConfigureNavigation(count);
            SelectFirst();
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void Select(int index)
        {
            if (options == null || index < 0 || index >= options.Length || options[index] == null)
                return;

            var chosen = options[index];
            Hide();
            var callback = onSelected;
            onSelected = null;
            callback?.Invoke(chosen);
        }

        private void SelectFirst()
        {
            if (EventSystem.current == null || questButtons == null)
                return;

            foreach (var button in questButtons)
            {
                if (button == null || !button.gameObject.activeSelf)
                    continue;

                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(button.gameObject);
                return;
            }
        }

        private void ConfigureNavigation(int count)
        {
            if (questButtons == null)
                return;

            for (int i = 0; i < questButtons.Length; i++)
            {
                var button = questButtons[i];
                if (button == null)
                    continue;

                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnLeft = i > 0 ? questButtons[i - 1] : null;
                navigation.selectOnRight = i < count - 1 ? questButtons[i + 1] : null;
                navigation.selectOnUp = null;
                navigation.selectOnDown = null;
                button.navigation = navigation;
            }
        }

        private static string BuildDetails(QuestDefinition quest)
        {
            var builder = new StringBuilder();
            builder.Append("Usia ").Append(quest.ageRange);
            if (quest.levels == null)
                return builder.ToString();

            foreach (var level in quest.levels)
            {
                if (level == null)
                    continue;

                builder.Append('\n').Append('\n');
                builder.Append(level.label.ToUpperInvariant()).Append("  ");
                builder.Append(QuestEvaluator.CategoryLabel(level.requiredA)).Append(" + ");
                builder.Append(QuestEvaluator.CategoryLabel(level.requiredB));
                builder.Append('\n');
                builder.Append(DescribeTarget(level.targetA)).Append(" · ");
                builder.Append(DescribeTarget(level.targetB));
            }

            return builder.ToString();
        }

        private static string DescribeTarget(QuestNutrientTarget target)
        {
            if (target == null)
                return "—";

            return QuestEvaluator.NutrientLabel(target.nutrient) + " ≥ " +
                QuestEvaluator.FormatAmount(target.minimum, target.nutrient);
        }
    }
}
