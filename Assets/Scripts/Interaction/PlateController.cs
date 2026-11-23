using System;
using System.Collections.Generic;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Interaction
{
    public class PlateController : MonoBehaviour
    {
        public PlateSlot[] slots;
        public int minItems = 4;
        public bool requiresDrink;
        public bool IsLocked { get; private set; }

        public event Action PlateChanged;

        public IReadOnlyList<FoodData> Foods
        {
            get
            {
                var foods = new List<FoodData>();
                if (slots == null)
                    return foods;

                foreach (var slot in slots)
                {
                    if (slot != null && slot.Current != null && slot.Current.data != null)
                        foods.Add(slot.Current.data);
                }

                return foods;
            }
        }

        public int Count => Foods.Count;

        private void Awake()
        {
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<PlateSlot>(true);

            foreach (var slot in slots)
            {
                if (slot == null)
                    continue;

                slot.plate = this;
                slot.Changed += OnSlotChanged;
            }
        }

        private void OnDestroy()
        {
            if (slots == null)
                return;

            foreach (var slot in slots)
            {
                if (slot != null)
                    slot.Changed -= OnSlotChanged;
            }
        }

        private void OnSlotChanged(PlateSlot slot)
        {
            PlateChanged?.Invoke();
        }

        public bool CanServe(out string reason)
        {
            if (IsLocked)
            {
                reason = "Piring sudah disajikan.";
                return false;
            }

            var requirement = MealRequirement.Evaluate(Foods);
            if (!requirement.IsComplete)
            {
                reason = requirement.MissingReason;
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public MealRequirementState Requirement => MealRequirement.Evaluate(Foods);

        public void Lock()
        {
            IsLocked = true;
            if (slots == null)
                return;

            foreach (var slot in slots)
            {
                if (slot == null)
                    continue;

                slot.Locked = true;
                if (slot.Current != null)
                    slot.Current.InteractionLocked = true;
            }
        }

        public void Clear()
        {
            if (slots == null)
                return;

            foreach (var slot in slots)
            {
                if (slot != null && slot.Current != null)
                    slot.Remove(slot.Current);
            }

            IsLocked = false;
        }
    }
}
