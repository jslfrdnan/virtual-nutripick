using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NutriPick.Nutrition;
using NutriPick.Interaction;

namespace NutriPick.Content
{
    public enum FoodStandKind
    {
        Healthy,
        Junk
    }

    public class FoodStand : MonoBehaviour
    {
        public string displayName = "Stand";
        public FoodStandKind kind = FoodStandKind.Healthy;
        public FoodData[] foods;
        public Transform[] anchors;
        public TMP_Text signText;

        private readonly List<FoodItem> spawned = new List<FoodItem>();
        private bool hasSpawned;

        public IReadOnlyList<FoodItem> Spawned => spawned;

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(displayName) && signText != null)
                signText.text = displayName;

            SpawnFoods();
        }

        public void SpawnFoods()
        {
            if (hasSpawned || foods == null)
                return;

            hasSpawned = true;

            for (int i = 0; i < foods.Length; i++)
            {
                var food = foods[i];
                if (food == null || food.prefab == null)
                {
                    Debug.LogWarning($"[NutriPick] FoodStand '{displayName}' is missing a food or prefab at index {i}.");
                    continue;
                }

                var anchor = anchors != null && anchors.Length > 0
                    ? anchors[Mathf.Min(i, anchors.Length - 1)]
                    : transform;

                var instance = Instantiate(food.prefab, anchor.position, anchor.rotation);
                instance.name = food.id;
                var item = instance.GetComponent<FoodItem>();
                if (item == null)
                    item = instance.AddComponent<FoodItem>();

                item.data = food;
                item.SetHome(this, anchor.position, anchor.rotation);
                spawned.Add(item);
            }
        }

        public bool Contains(FoodData food)
        {
            if (foods == null)
                return false;

            foreach (var entry in foods)
            {
                if (entry == food)
                    return true;
            }

            return false;
        }

        public void ReturnFood(FoodItem item)
        {
            if (item == null)
                return;

            // Every portion remembers its own spot on the stall (the duplicate tempe,
            // ayam goreng, brokoli and pisang pieces sit side by side), so send it back
            // there instead of stacking every copy on the shared anchor.
            item.ReturnToHome();
        }
    }
}
