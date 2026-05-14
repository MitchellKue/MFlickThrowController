using UnityEngine;

[RequireComponent(typeof(CombinedEngineControllerUnit))]
public class TugInput_Keyboard : MonoBehaviour
{
    private CombinedEngineControllerUnit engineController;

    [Header("Keyboard Settings")]
    public float keyboardAngleSpeed = 45f;          // degrees per second (A/D, 4/6)
    public float keyboardThrustChangeRate = 0.5f;   // per second (W/S, 8/5), 0..1
    public KeyCode emergencyStopKey = KeyCode.Space;

    private void Awake()
    {
        engineController = GetComponent<CombinedEngineControllerUnit>();
    }

    private void Update()
    {
        var engines = engineController.engines;
        if (engines == null || engines.Length < 2)
            return;

        if (Input.GetKeyDown(emergencyStopKey))
        {
            engineController.EmergencyStop();
            return; // skip normal input this frame
        }

        HandleKeyboardSimple(engines);
    }

    private void HandleKeyboardSimple(TugEngine[] engines)
    {
        float dt = Time.deltaTime;

        TugEngine left = engines[0];
        TugEngine right = engines[1];

        // --- LEFT ENGINE: A/D rotate, W/S thrust ---

        float angleDeltaLeft = 0f;
        if (Input.GetKey(KeyCode.A)) angleDeltaLeft -= keyboardAngleSpeed * dt;
        if (Input.GetKey(KeyCode.D)) angleDeltaLeft += keyboardAngleSpeed * dt;

        left.SetTargetAngle(left.targetAngle + angleDeltaLeft);

        float leftTarget01 = left.GetTargetThrust01();
        if (Input.GetKey(KeyCode.W)) leftTarget01 += keyboardThrustChangeRate * dt;
        if (Input.GetKey(KeyCode.S)) leftTarget01 -= keyboardThrustChangeRate * dt;
        leftTarget01 = Mathf.Clamp01(leftTarget01);

        left.SetTargetThrust01(leftTarget01);

        Vector2 leftDir = AngleToStick(left.targetAngle);
        left.desiredStick = leftDir;        // magnitude 1 = max rotation speed
        left.desiredTrigger = leftTarget01; // 0..1

        // --- RIGHT ENGINE: Keypad4/6 rotate, Keypad8/5 thrust ---

        float angleDeltaRight = 0f;
        if (Input.GetKey(KeyCode.Keypad4)) angleDeltaRight -= keyboardAngleSpeed * dt;
        if (Input.GetKey(KeyCode.Keypad6)) angleDeltaRight += keyboardAngleSpeed * dt;

        right.SetTargetAngle(right.targetAngle + angleDeltaRight);

        float rightTarget01 = right.GetTargetThrust01();
        if (Input.GetKey(KeyCode.Keypad8)) rightTarget01 += keyboardThrustChangeRate * dt;
        if (Input.GetKey(KeyCode.Keypad5)) rightTarget01 -= keyboardThrustChangeRate * dt;
        rightTarget01 = Mathf.Clamp01(rightTarget01);

        right.SetTargetThrust01(rightTarget01);

        Vector2 rightDir = AngleToStick(right.targetAngle);
        right.desiredStick = rightDir;
        right.desiredTrigger = rightTarget01;
    }

    private Vector2 AngleToStick(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        // y = forward, x = right (same convention as before)
        return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
    }
}