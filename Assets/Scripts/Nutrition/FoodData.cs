using UnityEngine;

namespace NutriPick.Nutrition
{
    [CreateAssetMenu(menuName = "NutriPick/Food Data", fileName = "FD_NewFood")]
    public class FoodData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Classification")]
        public FoodCompositionCategory composition;
        public ProcessingLevel processing;
        public bool isFried;
        public bool isDrink;
        public HealthyOverride healthyOverride;

        [Header("Nutrition (per portion)")]
        public int calories;
        public float protein;
        public float carbs;
        public float fat;
        public float fiber;
        public float addedSugar;
        public float sodium;
        public string portionLabel;

        [Header("Protein")]
        public ProteinType proteinType;
        public bool hasProteinType;

        [Header("Presentation")]
        public Sprite icon;
        public GameObject prefab;

        public bool HasCategory(FoodCompositionCategory category)
        {
            return (composition & category) != 0;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                error = "id is required";
                return false;
            }

            if (id != id.Trim().ToLowerInvariant() || id.Contains(" "))
            {
                error = "id must be lowercase snake_case";
                return false;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                error = "displayName is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                error = "description is required";
                return false;
            }

            if (composition == FoodCompositionCategory.None)
            {
                error = "composition must have at least one flag";
                return false;
            }

            if (isDrink != HasCategory(FoodCompositionCategory.Drink))
            {
                error = "isDrink must match the Drink flag";
                return false;
            }

            if (calories < 0 || protein < 0 || carbs < 0 || fat < 0 || fiber < 0 || addedSugar < 0 || sodium < 0)
            {
                error = "nutrition values must be non-negative";
                return false;
            }

            if (string.IsNullOrWhiteSpace(portionLabel))
            {
                error = "portionLabel is required";
                return false;
            }

            if (prefab == null)
            {
                error = "prefab is required";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }

    [System.Flags]
    public enum FoodCompositionCategory
    {
        None = 0,
        Vegetable = 1,
        Protein = 2,
        Carbohydrate = 4,
        Fruit = 8,
        Drink = 16,
        Snack = 32
    }

    public enum ProcessingLevel
    {
        Minimal,
        Processed,
        UltraProcessed
    }

    public enum HealthyOverride
    {
        None,
        ForceHealthy,
        ForceLessHealthy
    }

    public enum FoodHealthLabel
    {
        Healthy,
        LessHealthy
    }

    public enum ProteinType
    {
        Hewani,
        Nabati
    }
}
