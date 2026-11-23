using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NutriPick.Nutrition;
using NutriPick.Utilities;

namespace NutriPick.UI
{
    public class FoodInfoCardPresenter : MonoBehaviour
    {
        public GameObject compactRoot;
        public GameObject fullRoot;
        public TMP_Text compactName;
        public TMP_Text compactBadge;
        public Image compactBadgeBackground;
        public TMP_Text compactCalories;
        public TMP_Text fullName;
        public TMP_Text fullPortion;
        public TMP_Text fullBadge;
        public Image fullBadgeBackground;
        public TMP_Text fullStats;
        public TMP_Text fullDescription;
        public NutritionRules rules;

        private static NutritionRules fallbackRules;

        public void ShowCompact(FoodData food)
        {
            if (food == null)
                return;
            if (compactRoot != null) compactRoot.SetActive(true);
            if (fullRoot != null) fullRoot.SetActive(false);
            if (compactName != null) compactName.text = food.displayName;
            if (compactBadge != null) compactBadge.text = Label(food);
            if (compactBadgeBackground != null) compactBadgeBackground.color = BadgeColor(food);
            if (compactCalories != null)
            {
                compactCalories.text = string.Empty;
                compactCalories.gameObject.SetActive(false);
            }
        }

        public void ShowFull(FoodData food)
        {
            if (food == null)
                return;
            if (compactRoot != null) compactRoot.SetActive(false);
            if (fullRoot != null) fullRoot.SetActive(true);
            if (fullName != null) fullName.text = food.displayName;
            if (fullPortion != null)
            {
                fullPortion.text = string.Empty;
                fullPortion.gameObject.SetActive(false);
            }
            if (fullBadge != null) fullBadge.text = Label(food);
            if (fullBadgeBackground != null) fullBadgeBackground.color = BadgeColor(food);
            if (fullStats != null)
            {
                fullStats.text = string.Empty;
                fullStats.gameObject.SetActive(false);
            }
            if (fullDescription != null) fullDescription.text = food.description;
        }

        public void Hide()
        {
            if (compactRoot != null) compactRoot.SetActive(false);
            if (fullRoot != null) fullRoot.SetActive(false);
        }

        private string Label(FoodData food)
        {
            var activeRules = ResolveRules();
            return FoodClassifier.Resolve(food, activeRules) == FoodHealthLabel.Healthy
                ? "Sehat"
                : "Kurang sehat";
        }

        private Color BadgeColor(FoodData food)
        {
            var activeRules = ResolveRules();
            return FoodClassifier.Resolve(food, activeRules) == FoodHealthLabel.Healthy
                ? new Color(0.18f, 0.49f, 0.20f)
                : new Color(0.98f, 0.66f, 0.15f);
        }

        private NutritionRules ResolveRules()
        {
            if (rules != null)
                return rules;
            if (fallbackRules == null)
                fallbackRules = ScriptableObject.CreateInstance<NutritionRules>();
            return fallbackRules;
        }
    }
}
