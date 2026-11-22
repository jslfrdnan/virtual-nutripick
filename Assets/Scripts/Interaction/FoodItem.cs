using UnityEngine;
using NutriPick.Nutrition;
using NutriPick.Content;

namespace NutriPick.Interaction
{
    public class FoodItem : MonoBehaviour
    {
        public FoodData data;
        public FoodStand homeStand;
        public Rigidbody body;
        public Collider itemCollider;

        public PlateSlot PlatedSlot { get; private set; }
        public bool IsCarried { get; private set; }
        public bool InteractionLocked { get; set; }

        private Vector3 homePosition;
        private Quaternion homeRotation;

        private void Awake()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();
            if (itemCollider == null)
                itemCollider = GetComponent<Collider>();

            // Foods placed directly in a scene never go through SetHome, so remember
            // their starting spot here; foods spawned by a stand overwrite this in SetHome.
            homePosition = transform.position;
            homeRotation = transform.rotation;
        }

        public void SetHome(FoodStand stand, Vector3 position, Quaternion rotation)
        {
            homeStand = stand;
            homePosition = position;
            homeRotation = rotation;
            ResetToHome(position, rotation);
        }

        public void SetCarried(bool carried, Transform holdAnchor = null)
        {
            IsCarried = carried;

            if (carried)
            {
                if (PlatedSlot != null)
                    PlatedSlot.Remove(this);

                if (body != null)
                {
                    StopMotion();
                    body.isKinematic = true;
                }
                if (itemCollider != null)
                    itemCollider.enabled = false;

                transform.SetParent(holdAnchor, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(null);
                if (body != null)
                    body.isKinematic = false;
                if (itemCollider != null)
                    itemCollider.enabled = true;
            }
        }

        public void PlaceOnSlot(PlateSlot slot)
        {
            PlatedSlot = slot;
            IsCarried = false;
            transform.SetParent(slot.snapPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (body != null)
            {
                StopMotion();
                body.isKinematic = true;
            }
            if (itemCollider != null)
                itemCollider.enabled = true;
        }

        public void RemoveFromSlot()
        {
            PlatedSlot = null;
            IsCarried = false;
            transform.SetParent(null);

            if (body != null)
            {
                body.isKinematic = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            if (itemCollider != null)
                itemCollider.enabled = true;
        }

        public void Drop()
        {
            if (PlatedSlot != null)
                PlatedSlot.Remove(this);

            IsCarried = false;
            transform.SetParent(null);

            if (body != null)
            {
                body.isKinematic = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            if (itemCollider != null)
                itemCollider.enabled = true;

            CancelInvoke();
            Invoke(nameof(CheckLost), 1f);
        }

        public void ReturnHome()
        {
            CancelInvoke();
            if (homeStand != null)
                homeStand.ReturnFood(this);
            else
                ResetToHome(homePosition, homeRotation);
        }

        public void ResetToHome(Vector3 position, Quaternion rotation)
        {
            CancelInvoke();
            PlatedSlot = null;
            IsCarried = false;
            InteractionLocked = false;
            transform.SetParent(null);
            transform.position = position;
            transform.rotation = rotation;

            if (body != null)
            {
                StopMotion();
                body.isKinematic = true;
            }

            if (itemCollider != null)
                itemCollider.enabled = true;

            homePosition = position;
            homeRotation = rotation;
        }

        private void StopMotion()
        {
            if (body == null || body.isKinematic)
                return;

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        private void CheckLost()
        {
            if (Vector3.Distance(transform.position, homePosition) > 15f || transform.position.y < -0.5f)
                ReturnHome();
        }
    }
}
