using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class RexusInputProbe : MonoBehaviour
{
    private InputDevice lastDevice;

    void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null && gamepad != lastDevice)
        {
            lastDevice = gamepad;
            Debug.Log($"RexusInputProbe: perangkat input terdeteksi -> {gamepad.displayName}");
        }
    }
}
