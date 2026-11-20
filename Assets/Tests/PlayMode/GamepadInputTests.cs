using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;
using NutriPick.Player;

namespace NutriPick.Tests
{
    public class GamepadInputTests : InputTestFixture
    {
        private GameObject testObject;
        private GamepadInput input;

        public override void Setup()
        {
            base.Setup();

            testObject = new GameObject("GamepadInputTests");
            input = testObject.AddComponent<GamepadInput>();
            input.enabled = false;
            input.moveDeadzone = 0f;
            input.lookDeadzone = 0f;
            input.joystickProfile = null;
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(testObject);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator GamepadDrivesMoveLookAndInteract()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            Set(gamepad.leftStick, new Vector2(1f, 0f));
            Set(gamepad.rightStick, new Vector2(0f, 1f));
            Press(gamepad.buttonSouth);
            yield return null;

            input.Poll();

            Assert.That(input.Move.x, Is.EqualTo(1f).Within(0.05f));
            Assert.That(input.Move.y, Is.EqualTo(0f).Within(0.05f));
            Assert.That(input.Look.x, Is.EqualTo(0f).Within(0.05f));
            Assert.That(input.Look.y, Is.EqualTo(1f).Within(0.05f));
            Assert.IsTrue(input.InteractPressed);
        }

        [UnityTest]
        public IEnumerator GenericJoystickProfileDrivesMoveAndTrigger()
        {
            var joystick = InputSystem.AddDevice<Joystick>();
            var profile = ScriptableObject.CreateInstance<JoystickProfile>();
            profile.deviceNameContains = string.Empty;
            profile.moveStickPath = "stick";
            profile.lookStickPath = string.Empty;
            profile.lookXPath = string.Empty;
            profile.lookYPath = string.Empty;
            profile.interactButtonPath = "trigger";
            input.joystickProfile = profile;

            Set(joystick.stick, new Vector2(1f, 0f));
            Press(joystick.trigger);
            yield return null;

            input.Poll();

            Assert.That(input.Move.x, Is.EqualTo(1f).Within(0.05f));
            Assert.That(input.Move.y, Is.EqualTo(0f).Within(0.05f));
            Assert.IsTrue(input.InteractPressed);

            Object.DestroyImmediate(profile);
        }

        [UnityTest]
        public IEnumerator GamepadWinsWhenBothDevicesArePresent()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var joystick = InputSystem.AddDevice<Joystick>();
            var profile = ScriptableObject.CreateInstance<JoystickProfile>();
            profile.moveStickPath = "stick";
            profile.interactButtonPath = "trigger";
            input.joystickProfile = profile;

            Set(gamepad.leftStick, new Vector2(1f, 0f));
            Set(joystick.stick, new Vector2(0f, 1f));
            yield return null;

            input.Poll();

            Assert.That(input.Move.x, Is.EqualTo(1f).Within(0.05f));
            Assert.That(input.Move.y, Is.EqualTo(0f).Within(0.05f));

            Object.DestroyImmediate(profile);
        }

        [UnityTest]
        public IEnumerator KeyboardFallbackStillMoves()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            Press(keyboard.wKey);
            yield return null;

            input.Poll();

            Assert.That(input.Move.y, Is.EqualTo(1f).Within(0.02f));
        }
    }
}
