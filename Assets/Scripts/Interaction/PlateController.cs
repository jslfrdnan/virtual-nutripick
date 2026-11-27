using System;
using System.Collections.Generic;
using UnityEngine;
using NutriPick.Nutrition;
using NutriPick.Quests;

namespace NutriPick.Interaction
{
    public class PlateController : MonoBehaviour
    {
        public PlateSlot[] slots;
        public int minItems = 5;
        public bool requiresDrink;
        public QuestDefinition quest;
        public int questLevelIndex = 1;
        public bool IsLocked { get; private set; }

        public int Capacity => slots != null ? slots.Length : 0;

        public int RequiredItems
        {
            get
            {
                if (slots != null && slots.Length > 0)
                    return Mathf.Clamp(minItems, 1, slots.Length);

                return Mathf.Max(1, minItems);
            }
        }

        public float VisualScale { get; private set; } = 1f;

        public event Action PlateChanged;

        private Transform plateVisual;
        private Vector3 plateVisualBaseScale;

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

        public QuestEvaluation Evaluation => QuestEvaluator.Evaluate(Foods, quest, questLevelIndex);

        private void Awake()
        {
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<PlateSlot>(true);

            plateVisual = transform.Find("PlateVisual");
            if (plateVisual != null)
                plateVisualBaseScale = plateVisual.localScale;

            foreach (var slot in slots)
            {
                if (slot == null)
                    continue;

                slot.plate = this;
                slot.Changed += OnSlotChanged;
            }
        }

        public void SetVisualScale(float scale)
        {
            if (scale <= 0f)
                scale = 1f;

            VisualScale = scale;
            if (plateVisual != null && plateVisualBaseScale != Vector3.zero)
                plateVisual.localScale = plateVisualBaseScale * scale;
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

            int required = RequiredItems;

            if (Count < required)
            {
                reason = $"Tambahkan {required - Count} makanan lagi";
                return false;
            }

            if (quest != null)
            {
                var evaluation = Evaluation;
                if (!evaluation.AllPassed)
                {
                    reason = evaluation.MissingReason;
                    return false;
                }
            }
            else if (Count < RequiredItems)
            {
                reason = $"Tambahkan {RequiredItems - Count} makanan lagi";
                return false;
            }

            reason = string.Empty;
            return true;
        }

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
