using UnityEngine;
using UnityEngine.Events;

namespace NutriPick.Interaction
{
    public class ServeStation : MonoBehaviour
    {
        public UnityEvent onServe = new UnityEvent();
        public bool IsEnabled { get; private set; } = true;

        public void RequestServe()
        {
            if (IsEnabled)
                onServe?.Invoke();
        }

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;

            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                var material = renderer.material;
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", enabled ? new Color(0.18f, 0.49f, 0.20f) : new Color(0.35f, 0.35f, 0.35f));
            }
        }
    }
}
