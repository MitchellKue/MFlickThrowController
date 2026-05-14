using UnityEngine;

[System.Serializable]
public class TugEngine
{
    [Header("Setup")]
    public string engineName;
    public Vector3 localOffset = new Vector3(-1f, 0f, -2f); // relative to boat center
    public float maxThrust = 50f;
    public float maxEngineRotationSpeed = 60f; // degrees per second at full stick
    public float baseEngineRotationSpeed = 30f; // deg/s at minimal stick magnitude
    public float minStickForRotation = 0.1f;
    public float spinUpRate = 40f;  // units of thrust per second
    public float spinDownRate = 60f;

    [Header("Limits")]
    public bool limitRotation = false;
    public float minAngle = -180f;
    public float maxAngle = 180f;

    [Header("Runtime (read-only)")]
    public float currentAngle; // degrees, local yaw
    public float targetAngle;
    public float currentThrust;  // 0..maxThrust
    public float targetThrust;   // 0..maxThrust

    // Input from player/controller this frame
    [HideInInspector] public Vector2 desiredStick; // x,y
    [HideInInspector] public float desiredTrigger; // 0..1

    public void SetTargetAngle(float angle)
    {
        if (limitRotation)
            targetAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        else
            targetAngle = angle;
    }

    public void SetTargetThrust01(float t)
    {
        targetThrust = Mathf.Clamp01(t) * maxThrust;
    }

    public float GetTargetThrust01()
    {
        if (maxThrust <= 0.0001f) return 0f;
        return Mathf.Clamp01(targetThrust / maxThrust);
    }

    public void EmergencyStop(bool centerAngle)
    {
        SetTargetThrust01(0f);
        if (centerAngle)
            SetTargetAngle(0f);
    }

    public void Tick(Transform hullTransform, float deltaTime)
    {
        // --- 1. Update target angle from stick ---
        if (desiredStick.sqrMagnitude > minStickForRotation * minStickForRotation)
        {
            float stickAngle = Mathf.Atan2(desiredStick.x, desiredStick.y) * Mathf.Rad2Deg;
            // Stick forward (0°) = world forward; we treat this as local engine angle.
            targetAngle = stickAngle;

            if (limitRotation)
                targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

            float stickMag = Mathf.Clamp01(desiredStick.magnitude);
            float speed = Mathf.Lerp(baseEngineRotationSpeed, maxEngineRotationSpeed, stickMag);

            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, speed * deltaTime);
        }

        // --- 2. Update thrust from trigger with spin-up/down ---
        targetThrust = Mathf.Clamp01(desiredTrigger) * maxThrust;

        float rate = (targetThrust > currentThrust) ? spinUpRate : spinDownRate;
        currentThrust = Mathf.MoveTowards(currentThrust, targetThrust, rate * deltaTime);
    }

    public void ComputeForce(Transform hullTransform, out Vector3 worldForce, out Vector3 worldPosition)
    {
        // Direction in local space
        Quaternion localRot = Quaternion.Euler(0f, currentAngle, 0f);
        Vector3 localDir = localRot * Vector3.forward;

        // To world
        Vector3 worldDir = hullTransform.TransformDirection(localDir);
        worldForce = worldDir * currentThrust;

        worldPosition = hullTransform.TransformPoint(localOffset);
    }
}