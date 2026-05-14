using UnityEngine;
using UnityEngine.InputSystem;

public class TugInputSwitcher : MonoBehaviour
{
    public TugInput_Keyboard keyboardInput;
    public TugInput_Gamepad gamepadInput;

    [Header("Switching")]
    public float inactiveTimeout = 2f; // optional grace period

    private enum ActiveDevice { Keyboard, Gamepad }
    private ActiveDevice active = ActiveDevice.Keyboard;
    private float lastKeyboardTime;
    private float lastGamepadTime;

    private void Awake()
    {
        if (keyboardInput == null) keyboardInput = GetComponent<TugInput_Keyboard>();
        if (gamepadInput == null) gamepadInput = GetComponent<TugInput_Gamepad>();

        lastKeyboardTime = Time.time;
        SetActive(ActiveDevice.Keyboard); // default to keyboard
    }

    private void Update()
    {
        float t = Time.time;

        // --- Detect keyboard usage (very cheap and dumb) ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                lastKeyboardTime = t;
                if (active != ActiveDevice.Keyboard)
                    SetActive(ActiveDevice.Keyboard);
            }
        }

        // --- Detect gamepad usage via Input System ---
        if (Gamepad.current != null)
        {
            var g = Gamepad.current;
            bool gamepadUsed =
                g.leftStick.ReadValue().sqrMagnitude > 0.0001f ||
                g.rightStick.ReadValue().sqrMagnitude > 0.0001f ||
                g.leftTrigger.ReadValue() > 0.01f ||
                g.rightTrigger.ReadValue() > 0.01f ||
                g.buttonSouth.wasPressedThisFrame ||
                g.buttonNorth.wasPressedThisFrame ||
                g.buttonEast.wasPressedThisFrame ||
                g.buttonWest.wasPressedThisFrame;

            if (gamepadUsed)
            {
                lastGamepadTime = t;
                if (active != ActiveDevice.Gamepad)
                    SetActive(ActiveDevice.Gamepad);
            }
        }

        // OPTIONAL: if you want timeout‑based auto‑switching back to keyboard:
        // if (active == ActiveDevice.Gamepad &&
        //     t - lastGamepadTime > inactiveTimeout &&
        //     t - lastKeyboardTime < inactiveTimeout)
        // {
        //     SetActive(ActiveDevice.Keyboard);
        // }
    }

    private void SetActive(ActiveDevice device)
    {
        active = device;

        if (keyboardInput != null)
            keyboardInput.enabled = (device == ActiveDevice.Keyboard);

        if (gamepadInput != null)
            gamepadInput.enabled = (device == ActiveDevice.Gamepad);

        // Optional: debug log
        // Debug.Log("Active tug input: " + device);
    }
}