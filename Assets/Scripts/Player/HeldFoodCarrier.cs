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

        private void Awake()
        {
            EnsureAnchor();
        }

        public void Hold(FoodItem item)
        {
            if (item == null)
                return;

            EnsureAnchor();
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

        private void EnsureAnchor()
        {
            if (holdAnchor != null)
                return;

            var camera = Camera.main;
            Transform parent = camera != null ? camera.transform : transform;
            var anchor = new GameObject("HoldAnchor").transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = new Vector3(0f, -0.15f, 0.5f);
            anchor.localRotation = Quaternion.identity;
            holdAnchor = anchor;
        }
    }
}
