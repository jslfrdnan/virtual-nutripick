using System;
using UnityEngine;

namespace NutriPick.Nutrition
{
    [CreateAssetMenu(menuName = "NutriPick/Nutrition Rules", fileName = "NutritionRules")]
    public class NutritionRules : ScriptableObject
    {
        [Header("Plate score weights")]
        [Range(0f, 1f)] public float itemWeight = 0.50f;
        [Range(0f, 1f)] public float compositionWeight = 0.50f;

        [Header("Item health targets")]
        public float fiberTargetGrams = 8f;
        public float proteinTargetGrams = 20f;
        public float sugarTargetGrams = 15f;
        public float sodiumTargetMg = 600f;
        public float fiberWeight = 0.15f;
        public float proteinWeight = 0.15f;
        public float sugarWeight = 0.20f;
        public float sodiumWeight = 0.15f;
        public float processingWeight = 0.10f;
        public float friedWeight = 0.05f;
        public float unsweetenedDrinkBonus = 0.15f;
        [Range(0f, 1f)] public float itemHealthMin = 0.05f;
        [Range(0f, 1f)] public float itemHealthMax = 0.95f;

        [Header("Composition")]
        [Range(0f, 1f)] public float coreWeight = 0.70f;
        [Range(0f, 1f)] public float varietyWeight = 0.30f;
        public float missingCorePenalty = 0.15f;
        [Range(0f, 100f)] public float emptyPlateValue = 50f;

        [Header("Classification thresholds")]
        public float healthySugarMax = 10f;
        public float healthySodiumMax = 500f;
        public float healthyDrinkSugarMax = 5f;

        [Header("Report thresholds")]
        public float proteinStrengthMin = 20f;
        public float fiberStrengthMin = 6f;
        public float sugarStrengthMax = 5f;
        public float sodiumStrengthMax = 600f;
        public int varietyStrengthMin = 4;
        public float improvementSugarMax = 10f;
        public float improvementSodiumMax = 800f;
        public float improvementProteinMin = 15f;
        public float improvementFiberMin = 5f;
        public float calorieHigh = 800f;
        public float energyLow = 400f;
        public float energyHigh = 700f;
        public float focusProteinMin = 20f;
        public float focusFiberMin = 6f;
        public float focusSugarMax = 10f;
        public float longTermFiberMin = 6f;
        public float longTermSodiumMax = 600f;
        public int longTermUltraMax = 1;
        public float longTermSodiumHigh = 1000f;
        public int longTermUltraHigh = 2;
        public float longTermSugarHigh = 20f;

        [Header("Bands")]
        public ScoreBandDefinition[] bands = new[]
        {
            new ScoreBandDefinition { min = 0, max = 39, band = ScoreBand.NeedsImprovement, label = "Perlu perbaikan", color = new Color(0.776f, 0.157f, 0.157f) },
            new ScoreBandDefinition { min = 40, max = 69, band = ScoreBand.Adequate, label = "Cukup", color = new Color(0.976f, 0.659f, 0.145f) },
            new ScoreBandDefinition { min = 70, max = 100, band = ScoreBand.Balanced, label = "Seimbang", color = new Color(0.180f, 0.490f, 0.196f) }
        };

        public ScoreBandDefinition GetBand(int score)
        {
            if (bands != null)
            {
                foreach (var band in bands)
                {
                    if (band != null && score >= band.min && score <= band.max)
                        return band;
                }
            }

            return new ScoreBandDefinition
            {
                min = 0,
                max = 100,
                band = ScoreBand.Neutral,
                label = "Belum dinilai",
                color = new Color(0.62f, 0.62f, 0.62f)
            };
        }

        public bool IsValid(out string error)
        {
            if (Mathf.Abs(itemWeight + compositionWeight - 1f) > 0.001f)
            {
                error = "itemWeight + compositionWeight must equal 1";
                return false;
            }

            if (fiberTargetGrams <= 0f || proteinTargetGrams <= 0f || sugarTargetGrams <= 0f || sodiumTargetMg <= 0f)
            {
                error = "nutrition targets must be positive";
                return false;
            }

            if (bands == null || bands.Length == 0)
            {
                error = "at least one score band is required";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public class ScoreBandDefinition
    {
        public int min;
        public int max;
        public ScoreBand band;
        public string label;
        public Color color = Color.gray;
    }

    public enum ScoreBand
    {
        Neutral,
        NeedsImprovement,
        Adequate,
        Balanced
    }
}
