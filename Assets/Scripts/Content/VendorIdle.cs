using UnityEngine;

namespace NutriPick.Content
{
    /// <summary>Small idle motion so the primitive vendors do not look frozen.</summary>
    public class VendorIdle : MonoBehaviour
    {
        public float bobHeight = 0.02f;
        public float bobSpeed = 1.7f;
        public float swayDegrees = 2.5f;

        private Vector3 basePosition;
        private Quaternion baseRotation;

        private void Awake()
        {
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
        }

        private void LateUpdate()
        {
            float time = Time.time;
            transform.localPosition = basePosition + Vector3.up * (Mathf.Sin(time * bobSpeed) * bobHeight);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, Mathf.Sin(time * bobSpeed * 0.6f) * swayDegrees, 0f);
        }
    }
}
