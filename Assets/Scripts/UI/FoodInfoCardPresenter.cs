using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NutriPick.Nutrition;
using NutriPick.Quests;
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
                fullPortion.text = food.portionLabel;
                fullPortion.gameObject.SetActive(!string.IsNullOrWhiteSpace(food.portionLabel));
            }
            if (fullBadge != null) fullBadge.text = Label(food);
            if (fullBadgeBackground != null) fullBadgeBackground.color = BadgeColor(food);
            if (fullStats != null)
            {
                fullStats.text = Stats(food);
                fullStats.gameObject.SetActive(true);
            }
            if (fullDescription != null) fullDescription.text = food.description;
        }

        private static string Stats(FoodData food)
        {
            return
                "ENERGI: " + QuestEvaluator.FormatValue(NutrientType.Calories, food.calories) + "\n" +
                "PROTEIN: " + QuestEvaluator.FormatValue(NutrientType.Protein, food.protein) + "\n" +
                "KARBO: " + QuestEvaluator.FormatValue(NutrientType.Carbohydrate, food.carbs) + "\n" +
                "LEMAK: " + QuestEvaluator.FormatValue(NutrientType.Fat, food.fat) + "\n" +
                "SERAT: " + QuestEvaluator.FormatValue(NutrientType.Fiber, food.fiber) + "\n" +
                "GULA: " + QuestEvaluator.FormatValue(NutrientType.AddedSugar, food.addedSugar) + "\n" +
                "NATRIUM: " + QuestEvaluator.FormatValue(NutrientType.Sodium, food.sodium) + "\n" +
                "VIT A: " + QuestEvaluator.FormatValue(NutrientType.VitaminA, food.vitaminA) + "\n" +
                "VIT C: " + QuestEvaluator.FormatValue(NutrientType.VitaminC, food.vitaminC) + "\n" +
                "VIT D: " + QuestEvaluator.FormatValue(NutrientType.VitaminD, food.vitaminD) + "\n" +
                "KALSIUM: " + QuestEvaluator.FormatValue(NutrientType.Calcium, food.calcium) + "\n" +
                "BESI: " + QuestEvaluator.FormatValue(NutrientType.Iron, food.iron);
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
