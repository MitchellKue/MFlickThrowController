using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CombinedEngineControllerUnit))]
public class TugInput_Gamepad : MonoBehaviour
{
    private CombinedEngineControllerUnit engineController;

    [Header("Gamepad Input Actions")]
    public InputActionReference leftEngineDirection;  // Vector2
    public InputActionReference rightEngineDirection; // Vector2
    public InputActionReference leftEngineThrust;     // Axis (float)
    public InputActionReference rightEngineThrust;    // Axis (float)
    public InputActionReference emergencyStop;        // Button

    private void Awake()
    {
        engineController = GetComponent<CombinedEngineControllerUnit>();
    }

    private void OnEnable()
    {
        // Enable all actions if assigned
        EnableAction(leftEngineDirection);
        EnableAction(rightEngineDirection);
        EnableAction(leftEngineThrust);
        EnableAction(rightEngineThrust);
        EnableAction(emergencyStop);

        if (emergencyStop != null && emergencyStop.action != null)
            emergencyStop.action.performed += OnEmergencyStop;
    }

    private void OnDisable()
    {
        if (emergencyStop != null && emergencyStop.action != null)
            emergencyStop.action.performed -= OnEmergencyStop;

        DisableAction(leftEngineDirection);
        DisableAction(rightEngineDirection);
        DisableAction(leftEngineThrust);
        DisableAction(rightEngineThrust);
        DisableAction(emergencyStop);
    }

    private void OnEmergencyStop(InputAction.CallbackContext ctx)
    {
        engineController.EmergencyStop();
    }

    private void Update()
    {
        var engines = engineController.engines;
        if (engines == null || engines.Length < 2)
            return;

        Vector2 leftDir = ReadVector2(leftEngineDirection);
        Vector2 rightDir = ReadVector2(rightEngineDirection);
        float leftThrustVal = ReadFloat(leftEngineThrust);
        float rightThrustVal = ReadFloat(rightEngineThrust);

        // Feed directly into existing engine API
        engines[0].desiredStick = leftDir;
        engines[0].desiredTrigger = Mathf.Clamp01(leftThrustVal);

        engines[1].desiredStick = rightDir;
        engines[1].desiredTrigger = Mathf.Clamp01(rightThrustVal);
    }

    // --- Helpers ---

    private static void EnableAction(InputActionReference reference)
    {
        if (reference != null && reference.action != null)
            reference.action.Enable();
    }

    private static void DisableAction(InputActionReference reference)
    {
        if (reference != null && reference.action != null)
            reference.action.Disable();
    }

    private static Vector2 ReadVector2(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return Vector2.zero;
        return reference.action.ReadValue<Vector2>();
    }

    private static float ReadFloat(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return 0f;
        return reference.action.ReadValue<float>();
    }
}