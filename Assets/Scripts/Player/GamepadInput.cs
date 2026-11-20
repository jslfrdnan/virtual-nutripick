using UnityEngine;
using UnityEngine.InputSystem;

namespace NutriPick.Player
{
    public class GamepadInput : MonoBehaviour
    {
        [Header("Generic joystick")]
        [Tooltip("Optional profile for a Rexus pad that Windows exposes as a generic Joystick.")]
        public JoystickProfile joystickProfile;

        [Header("Stick feel")]
        [Range(0f, 0.5f)] public float moveDeadzone = 0.15f;
        [Range(0f, 0.5f)] public float lookDeadzone = 0.15f;
        public bool invertLookY;

        [Header("Mouse")]
        public float mouseLookSensitivity = 0.05f;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool CancelPressed { get; private set; }
        public bool InspectPressed { get; private set; }
        public bool ServePressed { get; private set; }
        public bool PausePressed { get; private set; }
        public bool ControllerConnected => Gamepad.current != null || ResolveJoystick() != null;
        public bool GamepadConnected => ControllerConnected;
        public bool HadGamepad { get; private set; }

        private void Update()
        {
            Poll();
        }

        public void Poll()
        {
            var gamepad = Gamepad.current;
            var joystick = ResolveJoystick();
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (gamepad != null || joystick != null)
                HadGamepad = true;

            Vector2 padMove = Vector2.zero;
            Vector2 padLook = Vector2.zero;
            bool interact = false;
            bool cancel = false;
            bool inspect = false;
            bool serve = false;
            bool pause = false;

            if (gamepad != null)
            {
                padMove = gamepad.leftStick.ReadValue();
                padLook = gamepad.rightStick.ReadValue();
                interact = gamepad.buttonSouth.wasPressedThisFrame;
                cancel = gamepad.buttonEast.wasPressedThisFrame;
                inspect = gamepad.buttonWest.wasPressedThisFrame;
                serve = gamepad.startButton.wasPressedThisFrame;
                pause = gamepad.selectButton.wasPressedThisFrame;
            }
            else if (joystick != null && joystickProfile != null)
            {
                joystickProfile.TryReadMove(joystick, out padMove);
                joystickProfile.TryReadLook(joystick, out padLook);
                interact = joystickProfile.WasInteractPressed(joystick);
                cancel = joystickProfile.WasCancelPressed(joystick);
                inspect = joystickProfile.WasInspectPressed(joystick);
                serve = joystickProfile.WasServePressed(joystick);
                pause = joystickProfile.WasPausePressed(joystick);
            }

            padMove = ApplyDeadzone(padMove, moveDeadzone);
            padLook = ApplyDeadzone(padLook, lookDeadzone);
            if (invertLookY)
                padLook.y = -padLook.y;

            Vector2 keyboardMove = Vector2.zero;
            if (keyboard != null)
            {
                float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                float y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
                keyboardMove = new Vector2(x, y);
            }

            Vector2 mouseLook = mouse != null
                ? mouse.delta.ReadValue() * mouseLookSensitivity
                : Vector2.zero;

            Move = padMove.sqrMagnitude > 0.0001f
                ? Vector2.ClampMagnitude(padMove, 1f)
                : Vector2.ClampMagnitude(keyboardMove, 1f);
            Look = padLook.sqrMagnitude > 0.0001f ? padLook : mouseLook;

            InteractPressed =
                interact ||
                (keyboard != null && keyboard.enterKey.wasPressedThisFrame) ||
                (mouse != null && mouse.leftButton.wasPressedThisFrame);

            CancelPressed =
                cancel ||
                (mouse != null && mouse.rightButton.wasPressedThisFrame);

            InspectPressed =
                inspect ||
                (keyboard != null && keyboard.tabKey.wasPressedThisFrame);

            ServePressed =
                serve ||
                (keyboard != null && keyboard.spaceKey.wasPressedThisFrame);

            PausePressed =
                pause ||
                (keyboard != null && keyboard.escapeKey.wasPressedThisFrame);
        }

        private Joystick ResolveJoystick()
        {
            return joystickProfile != null ? joystickProfile.Resolve() : null;
        }

        private static Vector2 ApplyDeadzone(Vector2 value, float deadzone)
        {
            var magnitude = value.magnitude;
            if (magnitude <= deadzone)
                return Vector2.zero;

            return value / magnitude * Mathf.InverseLerp(deadzone, 1f, magnitude);
        }
    }
}
