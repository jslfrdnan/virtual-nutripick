using System;
using UnityEngine;

namespace NutriPick.Interaction
{
    public class PlateSlot : MonoBehaviour
    {
        public PlateController plate;
        public Transform snapPoint;
        public Collider trigger;
        public GameObject highlight;

        public FoodItem Current { get; private set; }
        public bool IsOccupied => Current != null;
        public bool Locked { get; set; }

        public event Action<PlateSlot> Changed;

        private void Awake()
        {
            if (snapPoint == null)
                snapPoint = transform;
            if (trigger == null)
                trigger = GetComponent<Collider>();
            if (highlight != null)
                highlight.SetActive(false);
        }

        public bool TryPlace(FoodItem food)
        {
            if (Locked || IsOccupied || food == null)
                return false;

            Current = food;
            food.PlaceOnSlot(this);
            SetHighlight(false);
            Changed?.Invoke(this);
            return true;
        }

        public void Remove(FoodItem food)
        {
            if (Current != food)
                return;

            Current = null;
            food.RemoveFromSlot();
            Changed?.Invoke(this);
        }

        public void SetHighlight(bool visible)
        {
            if (highlight != null)
                highlight.SetActive(visible);
        }
    }
}
