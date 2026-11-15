using System;
using System.Collections.Generic;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Content
{
    [CreateAssetMenu(menuName = "NutriPick/Level Definition", fileName = "LD_NewLevel")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string levelId;
        public string displayName;
        public string sceneName;
        public string nextSceneName;
        public int levelIndex = 1;
        [TextArea] public string settingDescription;

        [Header("Content")]
        public FoodData[] foodPool;
        public int minItems = 3;
        public bool requiresDrink;
        public int minimumStands = 1;

        [Header("Pacing")]
        public float timeLimitSeconds = 180f;
        public float graceSeconds = 30f;
        public bool isFinalLevel;

        [Header("Learning")]
        public string[] learningFocus;

        public bool IsValid(out string error)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                error = "levelId is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                error = "displayName is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                error = "sceneName is required";
                return false;
            }

            if (levelIndex < 1 || levelIndex > 3)
            {
                error = "levelIndex must be 1-3";
                return false;
            }

            if (foodPool == null || foodPool.Length < 6 || foodPool.Length > 8)
            {
                error = "foodPool must contain 6-8 foods";
                return false;
            }

            var seen = new HashSet<string>();
            int healthy = 0;
            int lessHealthy = 0;
            bool hasDrink = false;

            foreach (var food in foodPool)
            {
                if (food == null)
                {
                    error = "foodPool contains a null entry";
                    return false;
                }

                if (!seen.Add(food.id))
                {
                    error = "foodPool contains duplicate food: " + food.id;
                    return false;
                }

                if (food.isDrink)
                    hasDrink = true;

                var rules = ScriptableObject.CreateInstance<NutritionRules>();
                var label = FoodClassifier.Resolve(food, rules);
                UnityEngine.Object.DestroyImmediate(rules);
                if (label == FoodHealthLabel.Healthy)
                    healthy++;
                else
                    lessHealthy++;
            }

            if (healthy < 2 || lessHealthy < 2)
            {
                error = "foodPool needs at least two healthy and two less healthy foods";
                return false;
            }

            if (minItems < 3 || minItems > 4 || minItems > foodPool.Length)
            {
                error = "minItems must be 3-4 and no larger than the pool";
                return false;
            }

            if (requiresDrink && !hasDrink)
            {
                error = "requiresDrink is true but the pool has no drink";
                return false;
            }

            if (minimumStands < 1)
            {
                error = "minimumStands must be at least 1";
                return false;
            }

            if (levelIndex == 3 && minimumStands < 3)
            {
                error = "Level 3 must have at least three stands";
                return false;
            }

            if (isFinalLevel && levelIndex != 3)
            {
                error = "isFinalLevel is only valid for Level 3";
                return false;
            }

            if (!isFinalLevel && string.IsNullOrWhiteSpace(nextSceneName))
            {
                error = "nextSceneName is required for non-final levels";
                return false;
            }

            if (timeLimitSeconds <= 0f || graceSeconds <= 0f)
            {
                error = "timeLimitSeconds and graceSeconds must be positive";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
