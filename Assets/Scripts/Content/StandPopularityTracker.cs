using System;
using System.Collections.Generic;
using UnityEngine;
using NutriPick.Interaction;
using NutriPick.Nutrition;

namespace NutriPick.Content
{
    /// <summary>
    /// Counts how many plated foods came from each open stand and raises events for the
    /// in-world sales board and the stand vendors.
    /// </summary>
    public class StandPopularityTracker : MonoBehaviour
    {
        public PlateController plate;
        public FoodStand[] stands;

        public int HealthyCount { get; private set; }
        public int JunkCount { get; private set; }

        public event Action CountsChanged;
        public event Action<FoodStand, int> StandCountChanged;
        public event Action<FoodStand> StandPicked;

        private readonly Dictionary<FoodStand, int> counts = new Dictionary<FoodStand, int>();

        private void Start()
        {
            if (plate == null)
                plate = FindFirstObjectByType<PlateController>();
            if (stands == null || stands.Length == 0)
                stands = FindObjectsByType<FoodStand>(FindObjectsInactive.Exclude);

            if (plate != null)
                plate.PlateChanged += RecountFromPlate;

            RecountFromPlate();
        }

        private void OnDestroy()
        {
            if (plate != null)
                plate.PlateChanged -= RecountFromPlate;
        }

        public int GetCount(FoodStand stand)
        {
            if (stand == null)
                return 0;

            return counts.TryGetValue(stand, out int value) ? value : 0;
        }

        public string WinnerLabel
        {
            get
            {
                if (HealthyCount == JunkCount)
                    return "Unggul: seri";

                return HealthyCount > JunkCount
                    ? "Unggul: Healthy Food"
                    : "Unggul: Junk Food";
            }
        }

        public void NotifyPickup(FoodStand stand)
        {
            if (stand != null)
                StandPicked?.Invoke(stand);
        }

        public void RecountFromPlate()
        {
            if (plate == null)
                return;

            Recount(plate.Foods);
        }

        public void Recount(IReadOnlyList<FoodData> foods)
        {
            var previous = new Dictionary<FoodStand, int>(counts);

            if (stands != null)
            {
                foreach (var stand in stands)
                {
                    if (stand != null)
                        counts[stand] = 0;
                }
            }

            if (foods != null)
            {
                foreach (var food in foods)
                {
                    if (food == null || stands == null)
                        continue;

                    foreach (var stand in stands)
                    {
                        if (stand != null && stand.Contains(food))
                        {
                            counts[stand] = GetCount(stand) + 1;
                            break;
                        }
                    }
                }
            }

            HealthyCount = 0;
            JunkCount = 0;

            foreach (var pair in counts)
            {
                if (pair.Key == null)
                    continue;

                if (pair.Key.kind == FoodStandKind.Healthy)
                    HealthyCount += pair.Value;
                else
                    JunkCount += pair.Value;

                if (!previous.TryGetValue(pair.Key, out int oldCount) || oldCount != pair.Value)
                    StandCountChanged?.Invoke(pair.Key, pair.Value);
            }

            CountsChanged?.Invoke();
        }
    }
}
