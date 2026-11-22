using UnityEngine;

namespace NutriPick.Utilities
{
    /// <summary>Keeps a world-space label facing the player camera.</summary>
    public class Billboard : MonoBehaviour
    {
        public bool yawOnly = true;

        private Camera targetCamera;

        private void OnEnable()
        {
            targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
                return;

            Vector3 direction = transform.position - targetCamera.transform.position;
            if (yawOnly)
                direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
