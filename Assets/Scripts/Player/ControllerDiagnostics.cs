using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NutriPick.Player
{
    [DisallowMultipleComponent]
    public class ControllerDiagnostics : MonoBehaviour
    {
        public bool visible;
        public Key toggleKey = Key.F9;

        private Vector2 scroll;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame)
                visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible)
                return;

            float width = Mathf.Min(720f, Screen.width - 20f);
            float height = Mathf.Min(640f, Screen.height - 20f);
            GUILayout.BeginArea(new Rect(10f, 10f, width, height), GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label($"<b>Controller diagnostics</b>  |  toggle: {toggleKey}");
            GUILayout.Label($"Gamepad.current: {Describe(Gamepad.current)}");
            GUILayout.Label($"Joystick.current: {Describe(Joystick.current)}");
            GUILayout.Label("Move the sticks and press each button; active controls are listed below.");

            foreach (var device in InputSystem.devices)
            {
                if (device is not Gamepad && device is not Joystick)
                    continue;

                GUILayout.Space(8f);
                GUILayout.Label($"<b>{device.displayName}</b>");
                GUILayout.Label($"layout={device.layout}  id={device.deviceId}  enabled={device.enabled}");

                foreach (var control in device.allControls)
                {
                    if (control == device)
                        continue;

                    object value = Read(control);
                    if (!IsActive(value))
                        continue;

                    GUILayout.Label($"{control.path} = {Format(value)}");
                }
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("Copy device report to clipboard"))
                GUIUtility.systemCopyBuffer = BuildReport();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static string Describe(InputDevice device)
        {
            return device == null ? "none" : $"{device.displayName} ({device.layout})";
        }

        private static object Read(InputControl control)
        {
            try
            {
                return control.ReadValueAsObject();
            }
            catch
            {
                return null;
            }
        }

        private static bool IsActive(object value)
        {
            return value switch
            {
                float f => Mathf.Abs(f) > 0.01f,
                Vector2 v => v.sqrMagnitude > 0.0001f,
                bool b => b,
                _ => value != null
            };
        }

        private static string Format(object value)
        {
            return value switch
            {
                float f => f.ToString("F2"),
                Vector2 v => $"({v.x:F2}, {v.y:F2})",
                _ => value?.ToString() ?? "null"
            };
        }

        private static string BuildReport()
        {
            var builder = new StringBuilder();
            builder.AppendLine("NutriPick controller report");
            builder.AppendLine($"Gamepad.current: {Describe(Gamepad.current)}");
            builder.AppendLine($"Joystick.current: {Describe(Joystick.current)}");

            foreach (var device in InputSystem.devices)
            {
                if (device is not Gamepad && device is not Joystick)
                    continue;

                builder.AppendLine();
                builder.AppendLine($"[{device.displayName}] layout={device.layout} id={device.deviceId} enabled={device.enabled}");
                foreach (var control in device.allControls)
                {
                    if (control == device)
                        continue;

                    object value = Read(control);
                    if (!IsActive(value))
                        continue;

                    builder.AppendLine($"{control.path} = {Format(value)}");
                }
            }

            return builder.ToString();
        }
    }
}
