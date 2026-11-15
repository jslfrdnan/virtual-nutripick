using UnityEngine;
using UnityEngine.InputSystem;

namespace NutriPick.Player
{
    public class GamepadInput : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool CancelPressed { get; private set; }
        public bool InspectPressed { get; private set; }
        public bool ServePressed { get; private set; }
        public bool PausePressed { get; private set; }
        public bool GamepadConnected => Gamepad.current != null;
        public bool HadGamepad { get; private set; }

        private void Update()
        {
            var gamepad = Gamepad.current;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (gamepad != null)
                HadGamepad = true;

            Vector2 move = gamepad != null ? gamepad.leftStick.ReadValue() : Vector2.zero;
            if (move.sqrMagnitude < 0.01f && keyboard != null)
            {
                float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                float y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
                move = new Vector2(x, y);
            }
            Move = Vector2.ClampMagnitude(move, 1f);

            Vector2 look = gamepad != null ? gamepad.rightStick.ReadValue() : Vector2.zero;
            if (look.sqrMagnitude < 0.01f && mouse != null)
                look = mouse.delta.ReadValue() * 0.05f;
            Look = look;

            InteractPressed =
                (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) ||
                (keyboard != null && keyboard.enterKey.wasPressedThisFrame) ||
                (mouse != null && mouse.leftButton.wasPressedThisFrame);

            CancelPressed =
                (gamepad != null && gamepad.buttonEast.wasPressedThisFrame) ||
                (mouse != null && mouse.rightButton.wasPressedThisFrame);

            InspectPressed =
                (gamepad != null && gamepad.buttonWest.wasPressedThisFrame) ||
                (keyboard != null && keyboard.tabKey.wasPressedThisFrame);

            ServePressed =
                (gamepad != null && gamepad.startButton.wasPressedThisFrame) ||
                (keyboard != null && keyboard.spaceKey.wasPressedThisFrame);

            PausePressed =
                (gamepad != null && gamepad.selectButton.wasPressedThisFrame) ||
                (keyboard != null && keyboard.escapeKey.wasPressedThisFrame);
        }
    }
}
