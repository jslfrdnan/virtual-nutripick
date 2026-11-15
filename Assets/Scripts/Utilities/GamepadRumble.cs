using UnityEngine;
using UnityEngine.InputSystem;

namespace NutriPick.Utilities
{
    public static class GamepadRumble
    {
        private static float stopTime;

        public static void Pulse(float low, float high, float duration)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null)
                return;

            gamepad.SetMotorSpeeds(low, high);
            stopTime = Time.unscaledTime + Mathf.Max(0.01f, duration);
            EnsureRunner();
        }

        private static void EnsureRunner()
        {
            if (Object.FindAnyObjectByType<RumbleRunner>() != null)
                return;

            var go = new GameObject("NutriPickRumbleRunner");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<RumbleRunner>();
        }

        private class RumbleRunner : MonoBehaviour
        {
            private void Update()
            {
                if (Time.unscaledTime >= stopTime)
                {
                    Gamepad.current?.SetMotorSpeeds(0f, 0f);
                    Destroy(gameObject);
                }
            }

            private void OnDestroy()
            {
                Gamepad.current?.SetMotorSpeeds(0f, 0f);
            }
        }
    }
}
