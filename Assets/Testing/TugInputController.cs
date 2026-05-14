using UnityEngine;

[RequireComponent(typeof(CombinedEngineControllerUnit))]
public class TugInputController : MonoBehaviour
{
    private CombinedEngineControllerUnit engineController;

    [Header("Input")]
    public bool useKeyboardFallback = true;

    [Header("Keyboard Tuning")]
    public float keyboardAngleSpeed = 45f; // degrees per second when holding A/D or 4/6
    public float keyboardThrustChangeRate = 0.5f; // per second (0..1)

    private void Awake()
    {
        engineController = GetComponent<CombinedEngineControllerUnit>();
    }

    private void Update()
    {
        var engines = engineController.engines;
        if (engines == null || engines.Length < 2)
            return;

        // --- GAMEPAD (if present) ---
        float lX = Input.GetAxis("LeftStickX");
        float lY = Input.GetAxis("LeftStickY");
        float rX = Input.GetAxis("RightStickX");
        float rY = Input.GetAxis("RightStickY");
        float leftTrig = Input.GetAxis("LeftTrigger");   // 0..1
        float rightTrig = Input.GetAxis("RightTrigger"); // 0..1

        bool hasGamepadInput =
            Mathf.Abs(lX) > 0.01f || Mathf.Abs(lY) > 0.01f ||
            Mathf.Abs(rX) > 0.01f || Mathf.Abs(rY) > 0.01f ||
            leftTrig > 0.01f || rightTrig > 0.01f;

        if (hasGamepadInput || !useKeyboardFallback)
        {
            // Joystick mode: each stick directly controls each engine
            engines[0].desiredStick = new Vector2(lX, lY);
            engines[0].desiredTrigger = Mathf.Clamp01(leftTrig);

            engines[1].desiredStick = new Vector2(rX, rY);
            engines[1].desiredTrigger = Mathf.Clamp01(rightTrig);
        }
        else if (useKeyboardFallback)
        {
            HandleKeyboardSimple(engines);
        }
    }

    private void HandleKeyboardSimple(TugEngine[] engines)
    {
        float dt = Time.deltaTime;

        TugEngine left = engines[0];
        TugEngine right = engines[1];

        // --- LEFT ENGINE: A/D rotate, W/S thrust ---

        // 1) Angle
        float angleDeltaLeft = 0f;
        if (Input.GetKey(KeyCode.A)) angleDeltaLeft -= keyboardAngleSpeed * dt;
        if (Input.GetKey(KeyCode.D)) angleDeltaLeft += keyboardAngleSpeed * dt;

        left.SetTargetAngle(left.targetAngle + angleDeltaLeft);

        // 2) Thrust (0..1 normalized)
        float leftTarget01 = left.targetThrust / Mathf.Max(left.maxThrust, 0.0001f);
        if (Input.GetKey(KeyCode.W)) leftTarget01 += keyboardThrustChangeRate * dt;
        if (Input.GetKey(KeyCode.S)) leftTarget01 -= keyboardThrustChangeRate * dt;
        leftTarget01 = Mathf.Clamp01(leftTarget01);

        left.SetTargetThrust01(leftTarget01);

        // Feed into joystick-style inputs for Tick()
        Vector2 leftDir = AngleToStick(left.targetAngle);
        left.desiredStick = leftDir;          // magnitude 1 => rotate at max speed
        left.desiredTrigger = leftTarget01;   // 0..1

        // --- RIGHT ENGINE: 4/6 rotate, 8/5 thrust ---

        float angleDeltaRight = 0f;
        if (Input.GetKey(KeyCode.Keypad4)) angleDeltaRight -= keyboardAngleSpeed * dt;
        if (Input.GetKey(KeyCode.Keypad6)) angleDeltaRight += keyboardAngleSpeed * dt;

        right.SetTargetAngle(right.targetAngle + angleDeltaRight);

        float rightTarget01 = right.targetThrust / Mathf.Max(right.maxThrust, 0.0001f);
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
        // Same convention as before: y = forward, x = right
        return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
    }
}