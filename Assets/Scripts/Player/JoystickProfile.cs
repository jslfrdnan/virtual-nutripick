using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace NutriPick.Player
{
    [CreateAssetMenu(menuName = "NutriPick/Joystick Profile", fileName = "RexusJoystickProfile")]
    public class JoystickProfile : ScriptableObject
    {
        [Header("Device")]
        [Tooltip("Optional display-name filter. Leave empty to use Joystick.current. Example: Rexus or USB Gamepad.")]
        public string deviceNameContains = string.Empty;

        [Header("Movement")]
        [Tooltip("Relative control path for a 2D stick, usually 'stick'.")]
        public string moveStickPath = "stick";
        [Tooltip("Optional X axis path if the device exposes separate axes instead of a StickControl.")]
        public string moveXPath = string.Empty;
        [Tooltip("Optional Y axis path if the device exposes separate axes instead of a StickControl.")]
        public string moveYPath = string.Empty;

        [Header("Look")]
        [Tooltip("Relative control path for a second 2D stick. Leave empty for a one-stick pad.")]
        public string lookStickPath = string.Empty;
        [Tooltip("Optional horizontal look axis, for example 'z' or 'rz'.")]
        public string lookXPath = string.Empty;
        [Tooltip("Optional vertical look axis, for example 'rz' or 'ry'.")]
        public string lookYPath = string.Empty;

        [Header("Buttons")]
        public string interactButtonPath = "button1";
        public string cancelButtonPath = "button2";
        public string inspectButtonPath = "button3";
        public string serveButtonPath = "button9";
        public string pauseButtonPath = "button8";

        public Joystick Resolve()
        {
            if (!string.IsNullOrWhiteSpace(deviceNameContains))
            {
                foreach (var device in InputSystem.devices)
                {
                    if (device is Joystick joystick &&
                        joystick.displayName.IndexOf(deviceNameContains, StringComparison.OrdinalIgnoreCase) >= 0)
                        return joystick;
                }

                return null;
            }

            return Joystick.current;
        }

        public bool TryReadMove(Joystick joystick, out Vector2 value)
        {
            return TryReadStick(joystick, moveStickPath, moveXPath, moveYPath, out value);
        }

        public bool TryReadLook(Joystick joystick, out Vector2 value)
        {
            return TryReadStick(joystick, lookStickPath, lookXPath, lookYPath, out value);
        }

        public bool WasInteractPressed(Joystick joystick) => WasPressed(joystick, interactButtonPath);
        public bool WasCancelPressed(Joystick joystick) => WasPressed(joystick, cancelButtonPath);
        public bool WasInspectPressed(Joystick joystick) => WasPressed(joystick, inspectButtonPath);
        public bool WasServePressed(Joystick joystick) => WasPressed(joystick, serveButtonPath);
        public bool WasPausePressed(Joystick joystick) => WasPressed(joystick, pauseButtonPath);

        public static T Find<T>(InputControl device, string path) where T : InputControl
        {
            if (device == null || string.IsNullOrWhiteSpace(path))
                return null;

            if (path[0] == '<')
                return InputControlPath.TryFindControl<T>(device, path);

            InputControl current = device;
            foreach (var part in path.Split('/'))
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                InputControl next = null;
                foreach (var child in current.children)
                {
                    if (string.Equals(child.name, part, StringComparison.OrdinalIgnoreCase))
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null && part.Length > 2 && part[0] == '{' && part[part.Length - 1] == '}')
                {
                    var usage = part.Substring(1, part.Length - 2);
                    foreach (var child in current.children)
                    {
                        foreach (var childUsage in child.usages)
                        {
                            if (string.Equals(childUsage.ToString(), usage, StringComparison.OrdinalIgnoreCase))
                            {
                                next = child;
                                break;
                            }
                        }

                        if (next != null)
                            break;
                    }
                }

                if (next == null)
                    return null;

                current = next;
            }

            return current as T;
        }

        public static bool WasPressed(InputControl device, string path)
        {
            var button = Find<ButtonControl>(device, path);
            return button != null && button.wasPressedThisFrame;
        }

        private static bool TryReadStick(
            InputControl device,
            string stickPath,
            string xPath,
            string yPath,
            out Vector2 value)
        {
            value = Vector2.zero;
            if (device == null)
                return false;

            var stick = Find<StickControl>(device, stickPath);
            if (stick != null)
            {
                value = stick.ReadValue();
                return true;
            }

            var x = Find<AxisControl>(device, xPath);
            var y = Find<AxisControl>(device, yPath);
            if (x != null && y != null)
            {
                value = new Vector2(x.ReadValue(), y.ReadValue());
                return true;
            }

            return false;
        }
    }
}
