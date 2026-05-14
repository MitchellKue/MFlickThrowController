using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CombinedEngineControllerUnit : MonoBehaviour
{
    public Transform centerReference; // center of hull; if null, use transform
    public TugEngine[] engines;

    [Header("Physics Tuning")]
    public float linearDrag = 0.5f;
    public float angularDrag = 1.0f;
    public float maxLinearSpeed = 15f;
    public float maxAngularSpeedDeg = 90f;

    [Header("Emergency Stop")]
    public bool emergencyStopCentersEngines = true;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Transform hull = centerReference != null ? centerReference : transform;

        // 1. Tick engines and apply forces
        if (engines != null)
        {
            foreach (var engine in engines)
            {
                engine.Tick(hull, dt);

                engine.ComputeForce(hull, out var worldForce, out var worldPos);
                rb.AddForceAtPosition(worldForce, worldPos, ForceMode.Force);
            }
        }

        // 2. Custom drag
        ApplyCustomDrag(dt);

        // 3. Clamp speeds (arcade control)
        ClampVelocities();
    }

    public void EmergencyStop()
    {
        if (engines == null) return;

        foreach (var engine in engines)
        {
            engine.EmergencyStop(emergencyStopCentersEngines);
        }
    }

    private void ApplyCustomDrag(float dt)
    {
        Vector3 v = rb.linearVelocity;
        Vector3 horizontal = new Vector3(v.x, 0f, v.z);
        Vector3 vertical = new Vector3(0f, v.y, 0f);

        horizontal *= Mathf.Clamp01(1f - linearDrag * dt);
        vertical = Vector3.zero; // we stay in 2D plane (for now)

        rb.linearVelocity = horizontal + vertical;

        Vector3 av = rb.angularVelocity;
        Vector3 yaw = new Vector3(0f, av.y, 0f);
        yaw *= Mathf.Clamp01(1f - angularDrag * dt);
        rb.angularVelocity = yaw;
    }

    private void ClampVelocities()
    {
        Vector3 v = rb.linearVelocity;
        Vector3 horizontal = new Vector3(v.x, 0f, v.z);
        float speed = horizontal.magnitude;

        if (speed > maxLinearSpeed)
        {
            horizontal = horizontal.normalized * maxLinearSpeed;
            rb.linearVelocity = new Vector3(horizontal.x, 0f, horizontal.z);
        }

        float maxAngularSpeedRad = maxAngularSpeedDeg * Mathf.Deg2Rad;
        Vector3 av = rb.angularVelocity;
        if (Mathf.Abs(av.y) > maxAngularSpeedRad)
        {
            av.y = Mathf.Sign(av.y) * maxAngularSpeedRad;
            rb.angularVelocity = av;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (engines == null || engines.Length == 0)
            return;

        Transform hull = centerReference != null ? centerReference : transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(hull.position, 0.2f); // hull center indicator

        foreach (var engine in engines)
        {
            // Engine position in world
            Vector3 worldPos = hull.TransformPoint(engine.localOffset);

            // Draw engine position
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(worldPos, 0.15f);

            // Approximate thrust vector in Scene view
            Quaternion localRot = Quaternion.Euler(0f, engine.currentAngle, 0f);
            Vector3 localDir = localRot * Vector3.forward;
            Vector3 worldDir = hull.TransformDirection(localDir);

            float thrustScale = Mathf.InverseLerp(0f, engine.maxThrust, engine.currentThrust);
            float gizmoLength = Mathf.Lerp(0.5f, 2.0f, thrustScale);

            Vector3 end = worldPos + worldDir * gizmoLength;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldPos, end);
            Gizmos.DrawSphere(end, 0.08f);
        }
    }
}