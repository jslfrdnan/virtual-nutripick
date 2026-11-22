using System;
using UnityEngine;
using NutriPick.Interaction;

namespace NutriPick.Player
{
    public class HeldFoodCarrier : MonoBehaviour
    {
        public Transform holdAnchor;
        public FoodItem Current { get; private set; }

        public bool IsCarrying => Current != null;

        public event Action Changed;

        public void Hold(FoodItem item)
        {
            if (item == null)
                return;

            Current = item;
            item.SetCarried(true, holdAnchor);
            Changed?.Invoke();
        }

        public void Release()
        {
            if (Current == null)
                return;

            var item = Current;
            Current = null;
            item.SetCarried(false);
            Changed?.Invoke();
        }
    }
}
