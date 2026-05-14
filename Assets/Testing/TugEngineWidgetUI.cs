using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TugEngineWidgetUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform backgroundRect;

    [Header("Engine Marker (actual engine feedback)")]
    public RectTransform engineMarkerRect;
    public Image engineMarkerImage;

    [Header("Input Marker (controls feedback)")]
    public RectTransform inputMarkerRect;
    public Image inputMarkerImage;

    [Header("Trail")]
    [Tooltip("Prefab for a single trail dot (UI Image under the background).")]
    public Image trailDotPrefab;

    [Tooltip("How often to spawn a trail dot (seconds).")]
    public float trailSpawnInterval = 0.05f;

    [Tooltip("How long a trail dot lives (seconds).")]
    public float trailLifetime = 0.5f;

    [Tooltip("Initial scale of trail dots.")]
    public float trailInitialScale = 0.4f;

    [Tooltip("Color of trail dots (alpha will be controlled by lifetime).")]
    public Color trailColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);

    [Header("Text")]
    public Text nameText;
    public Text angleText;
    public Text thrustText;
    public Text rotationText;
    public Text spinText;
    public Text limitsText;

    [Header("Config")]
    [Tooltip("Max fraction of background half-size the marker can reach.")]
    [Range(0f, 1f)] public float markerRadius = 0.45f; // 0–0.5 of background half-size

    [Tooltip("Minimum radius so ENGINE marker is visible even at 0 thrust (0 = center, 1 = edge).")]
    [Range(0f, 1f)] public float engineMinRadius01 = 0.2f;

    [Tooltip("If true, invert the ENGINE direction for UI (thrust dir vs move dir).")]
    public bool invertDirectionForUI = false;

    [Tooltip("Min alpha of input marker when input is nearly zero.")]
    [Range(0f, 1f)] public float inputMinAlpha = 0.05f;

    [Tooltip("Max alpha of input marker when input is full.")]
    [Range(0f, 1f)] public float inputMaxAlpha = 0.8f;

    [Header("Pulse (max thrust feedback)")]
    [Tooltip("Thrust above this value will trigger pulsing.")]
    [Range(0f, 1f)] public float pulseThreshold = 0.95f;

    [Tooltip("Pulse frequency in Hz (cycles per second) when at high thrust.")]
    public float pulseFrequency = 4f;

    [Tooltip("Pulse amplitude (extra scale). 0.1 = ±10% of base scale.")]
    public float pulseAmplitude = 0.2f;

    [Tooltip("Base scale of the engine marker (1,1 for no scaling).")]
    public Vector3 engineMarkerBaseScale = Vector3.one;

    // Bound data
    private TugEngine engine;

    // Trail bookkeeping
    private class TrailDot
    {
        public Image image;
        public RectTransform rect;
        public float spawnTime;
    }

    private readonly List<TrailDot> trailDots = new List<TrailDot>();
    private float lastTrailSpawnTime;

    public void Bind(TugEngine engine)
    {
        this.engine = engine;
        if (nameText != null)
            nameText.text = engine != null ? engine.engineName : "N/A";
    }

    private void Update()
    {
        if (engine == null || backgroundRect == null)
            return;

        UpdateEngineMarker();
        UpdateInputMarker();
        UpdateTrail();
        UpdateText();
    }

    // ------------------ ENGINE MARKER (actual engine feedback) ------------------

    private void UpdateEngineMarker()
    {
        if (engineMarkerRect == null)
            return;

        // 0° is world Z+. For UI we want 0° = up.
        // x = sin(angle), y = cos(angle) ⇒ 0° -> up, 90° -> right.
        float angleDeg = engine.currentAngle;      // actual engine angle
        float angleRad = angleDeg * Mathf.Deg2Rad;

        Vector2 dir = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));

        if (invertDirectionForUI)
            dir = -dir;

        dir.Normalize();

        // Current thrust (engine feedback)
        float currentThrust01 = Mathf.Clamp01(engine.GetTargetThrust01());

        // Always show at least engineMinRadius01 so angle is visible
        float radius01 = Mathf.Lerp(engineMinRadius01, 1f, currentThrust01);

        Vector2 halfSize = backgroundRect.rect.size * 0.5f;
        Vector2 offset = new Vector2(
            dir.x * halfSize.x * markerRadius * radius01,
            dir.y * halfSize.y * markerRadius * radius01
        );

        engineMarkerRect.anchoredPosition = offset;

        // Color based on CURRENT thrust: green → yellow → red
        if (engineMarkerImage != null)
        {
            float t = currentThrust01;
            Color low = Color.green;
            Color mid = Color.yellow;
            Color high = Color.red;

            Color thrustColor = t < 0.5f
                ? Color.Lerp(low, mid, t / 0.5f)
                : Color.Lerp(mid, high, (t - 0.5f) / 0.5f);

            engineMarkerImage.color = thrustColor;
        }

        // Pulse when near max thrust
        UpdateEngineMarkerPulse(currentThrust01);
    }

    private void UpdateEngineMarkerPulse(float currentThrust01)
    {
        if (engineMarkerRect == null)
            return;

        // Base scale when not pulsing
        Vector3 baseScale = engineMarkerBaseScale == Vector3.zero
            ? Vector3.one
            : engineMarkerBaseScale;

        if (currentThrust01 >= pulseThreshold)
        {
            // Pulse scale: 1 + sin(2π f t) * amp
            float time = Time.unscaledTime;   // UI not tied to game time
            float s = 1f + Mathf.Sin(time * Mathf.PI * 2f * pulseFrequency) * pulseAmplitude;
            engineMarkerRect.localScale = baseScale * s;
        }
        else
        {
            // Smoothly return to base scale when below threshold
            engineMarkerRect.localScale = Vector3.Lerp(
                engineMarkerRect.localScale,
                baseScale,
                Time.unscaledDeltaTime * 10f
            );
        }
    }

    // ------------------ INPUT MARKER (player input feedback) ------------------

    private void UpdateInputMarker()
    {
        if (inputMarkerRect == null || inputMarkerImage == null)
            return;

        // Direction from input stick (-1..1, -1..1)
        Vector2 dir = engine.desiredStick;
        float mag = dir.magnitude;

        if (mag > 0.0001f)
            dir /= mag;
        else
            dir = Vector2.zero;

        // Thrust target (0..1)
        float thrustTarget01 = Mathf.Clamp01(engine.desiredTrigger);

        // Radius is just based on thrust (pure input feedback)
        float radius01 = thrustTarget01;

        Vector2 halfSize = backgroundRect.rect.size * 0.5f;
        Vector2 offset = new Vector2(
            dir.x * halfSize.x * markerRadius * radius01,
            dir.y * halfSize.y * markerRadius * radius01
        );

        inputMarkerRect.anchoredPosition = offset;

        // Input strength: combine stick + trigger for alpha
        float inputStrength = Mathf.Clamp01(Mathf.Max(mag, thrustTarget01));
        float alpha = Mathf.Lerp(inputMinAlpha, inputMaxAlpha, inputStrength);

        // Grey with variable alpha
        Color baseGrey = Color.grey;
        baseGrey.a = alpha;
        inputMarkerImage.color = baseGrey;
    }

    // ------------------ TRAIL (behind engine marker) ------------------

    private void UpdateTrail()
    {
        if (trailDotPrefab == null || engineMarkerRect == null)
            return;

        float now = Time.unscaledTime;

        // Spawn new dot at interval
        if (now - lastTrailSpawnTime >= trailSpawnInterval)
        {
            lastTrailSpawnTime = now;
            SpawnTrailDot();
        }

        // Update & prune existing dots
        for (int i = trailDots.Count - 1; i >= 0; i--)
        {
            TrailDot dot = trailDots[i];
            float age = now - dot.spawnTime;
            float t = age / trailLifetime;

            if (t >= 1f)
            {
                Destroy(dot.image.gameObject);
                trailDots.RemoveAt(i);
                continue;
            }

            // Fade out alpha and shrink scale over time
            float alpha = Mathf.Lerp(trailColor.a, 0f, t);
            Color c = trailColor;
            c.a = alpha;
            dot.image.color = c;

            float scale = Mathf.Lerp(trailInitialScale, 0f, t);
            dot.rect.localScale = Vector3.one * scale;
        }
    }

    private void SpawnTrailDot()
    {
        // Instantiate under background so it lives in the same local space
        Image inst = Instantiate(trailDotPrefab, backgroundRect);
        RectTransform rt = inst.rectTransform;

        rt.anchorMin = engineMarkerRect.anchorMin;
        rt.anchorMax = engineMarkerRect.anchorMax;
        rt.pivot = engineMarkerRect.pivot;
        rt.anchoredPosition = engineMarkerRect.anchoredPosition;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * trailInitialScale;

        inst.color = trailColor;

        trailDots.Add(new TrailDot
        {
            image = inst,
            rect = rt,
            spawnTime = Time.unscaledTime
        });
    }

    // ------------------ TEXT ------------------

    private void UpdateText()
    {
        float currentAngleDeg = engine.currentAngle;
        float targetAngleDeg = engine.targetAngle;

        float currentThrust01 = Mathf.Clamp01(engine.GetTargetThrust01());
        float thrustTarget01 = Mathf.Clamp01(engine.desiredTrigger);
        float maxThrust = engine.maxThrust;

        float maxRotSpeedDeg = engine.maxEngineRotationSpeed;
        float baseRotSpeedDeg = engine.baseEngineRotationSpeed;

        float spinUpRate = engine.spinUpRate;
        float spinDownRate = engine.spinDownRate;

        float minAngleDeg = engine.minAngle;
        float maxAngleDeg = engine.maxAngle;

        if (angleText != null)
        {
            angleText.text = $"Angle: cur {currentAngleDeg:0.0}°  tgt {targetAngleDeg:0.0}°";
        }

        if (thrustText != null)
        {
            thrustText.text =
                $"Thrust: cur {currentThrust01:0.00}  tgt {thrustTarget01:0.00}  max {maxThrust:0.00}";
        }

        if (rotationText != null)
        {
            rotationText.text =
                $"RotSpeed: max {maxRotSpeedDeg:0.0}°/s  base {baseRotSpeedDeg:0.0}°/s";
        }

        if (spinText != null)
        {
            spinText.text = $"Spin Up/Down: {spinUpRate:0.00} / {spinDownRate:0.00}";
        }

        if (limitsText != null)
        {
            limitsText.text = $"Angle Limits: {minAngleDeg:0.0}° .. {maxAngleDeg:0.0}°";
        }
    }
}