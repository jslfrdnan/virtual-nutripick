using System;
using System.Collections.Generic;
using UnityEngine;

namespace NutriPick.Nutrition
{
    [Serializable]
    public class FoodScore
    {
        public string foodId;
        public string displayName;
        public FoodHealthLabel healthLabel;
        public float healthValue;
    }

    [Serializable]
    public class MealResult
    {
        public int score = 50;
        public ScoreBand band = ScoreBand.Neutral;
        public string bandLabel = "Belum dinilai";
        public int healthyCount;
        public int lessHealthyCount;
        public List<FoodScore> itemScores = new List<FoodScore>();
        public float averageItemHealth;
        public float compositionScore;
        public FoodCompositionCategory compositionCovered;
        public string[] strengths = Array.Empty<string>();
        public string improvement = string.Empty;
        public HealthDimensions dimensions = new HealthDimensions();
        public bool isIncomplete;
        public string[] warnings = Array.Empty<string>();

        public bool IsNeutral => band == ScoreBand.Neutral;
    }

    [Serializable]
    public class HealthDimensions
    {
        public DimensionAssessment energy = new DimensionAssessment();
        public DimensionAssessment focus = new DimensionAssessment();
        public DimensionAssessment longTerm = new DimensionAssessment();
    }

    [Serializable]
    public class DimensionAssessment
    {
        public DimensionRating rating;
        public string label = string.Empty;
        public string summary = string.Empty;
    }

    public enum DimensionRating
    {
        Rendah,
        Cukup,
        Baik,
        Berlebih,
        PerluPerhatian
    }
}
