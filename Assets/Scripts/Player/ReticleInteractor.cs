using UnityEngine;
using NutriPick.Interaction;
using NutriPick.UI;
using NutriPick.Flow;

namespace NutriPick.Player
{
    public class ReticleInteractor : MonoBehaviour
    {
        public Camera interactionCamera;
        public HeldFoodCarrier carrier;
        public LevelFlowController flow;
        public FoodInfoCardPresenter infoCard;
        public InteractionPrompt prompt;
        public PlateController plate;
        public ServeStation serveStation;
        public float maxDistance = 3.0f;

        private PlateSlot highlightedSlot;

        private void Start()
        {
            if (interactionCamera == null)
                interactionCamera = GetComponentInChildren<Camera>();
            if (carrier == null)
                carrier = GetComponent<HeldFoodCarrier>();
            if (flow == null)
                flow = Object.FindAnyObjectByType<LevelFlowController>();
            if (infoCard == null)
                infoCard = Object.FindAnyObjectByType<FoodInfoCardPresenter>();
            if (prompt == null)
                prompt = Object.FindAnyObjectByType<InteractionPrompt>();
            if (plate == null)
                plate = Object.FindAnyObjectByType<PlateController>();
            if (serveStation == null)
                serveStation = Object.FindAnyObjectByType<ServeStation>();
        }

        private void Update()
        {
            if (flow == null || flow.IsPaused || flow.IsReportOpen || interactionCamera == null)
            {
                ClearTargets();
                return;
            }

            var ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
            FoodItem food = null;
            PlateSlot slot = null;
            ServeStation serve = null;

            if (Physics.Raycast(ray, out var hit, maxDistance, ~0, QueryTriggerInteraction.Collide))
            {
                food = hit.collider.GetComponentInParent<FoodItem>();
                slot = hit.collider.GetComponentInParent<PlateSlot>();
                serve = hit.collider.GetComponentInParent<ServeStation>();
            }

            if (carrier != null && carrier.IsCarrying)
                HandleCarrying(food, slot, serve);
            else
                HandleEmpty(food, serve);
        }

        private void HandleCarrying(FoodItem food, PlateSlot slot, ServeStation serve)
        {
            var carried = carrier.Current;
            if (carried == null)
                return;

            if (slot != null && !slot.IsOccupied && !slot.Locked)
            {
                SetHighlightedSlot(slot);
                prompt?.Show("A: Taruh");
                if (flow.Input.InteractPressed)
                    Place(slot);
                else if (flow.Input.CancelPressed)
                    Drop();
                else if (flow.Input.InspectPressed)
                    infoCard?.ShowFull(carried.data);
                return;
            }

            SetHighlightedSlot(null);

            if (serve != null)
            {
                prompt?.Show(flow.GetServeReason());
                if (flow.Input.InteractPressed)
                    flow.TryServe();
                else if (flow.Input.CancelPressed)
                    Drop();
                return;
            }

            prompt?.Show("A: Taruh · B: Lepas");
            if (flow.Input.CancelPressed)
                Drop();
            else if (flow.Input.InspectPressed)
                infoCard?.ShowFull(carried.data);
        }

        private void HandleEmpty(FoodItem food, ServeStation serve)
        {
            SetHighlightedSlot(null);

            if (food != null && !food.InteractionLocked)
            {
                infoCard?.ShowCompact(food.data);
                prompt?.Show("A: Ambil · X: Info");

                if (flow.Input.InteractPressed)
                    PickUp(food);
                else if (flow.Input.InspectPressed)
                    infoCard?.ShowFull(food.data);
                return;
            }

            infoCard?.Hide();

            if (serve != null)
            {
                prompt?.Show(flow.GetServeReason());
                if (flow.Input.InteractPressed)
                    flow.TryServe();
                return;
            }

            prompt?.Hide();
        }

        private void PickUp(FoodItem food)
        {
            if (food == null || food.InteractionLocked)
                return;

            if (food.PlatedSlot != null)
                food.PlatedSlot.Remove(food);

            carrier.Hold(food);
            infoCard?.ShowFull(food.data);
            flow.PlayPickup();
        }

        private void Place(PlateSlot slot)
        {
            var item = carrier.Current;
            if (item == null || slot == null)
                return;

            carrier.Release();
            if (slot.TryPlace(item))
            {
                infoCard?.Hide();
                prompt?.Hide();
                flow.PlayPlace();
            }
            else
            {
                item.Drop();
            }
        }

        private void Drop()
        {
            var item = carrier.Current;
            if (item == null)
                return;

            carrier.Release();
            item.Drop();
            infoCard?.Hide();
            prompt?.Hide();
            flow.PlayDrop();
        }

        private void SetHighlightedSlot(PlateSlot slot)
        {
            if (highlightedSlot == slot)
                return;

            if (highlightedSlot != null)
                highlightedSlot.SetHighlight(false);
            highlightedSlot = slot;
            if (highlightedSlot != null)
                highlightedSlot.SetHighlight(true);
        }

        private void ClearTargets()
        {
            SetHighlightedSlot(null);
            infoCard?.Hide();
            prompt?.Hide();
        }
    }
}
