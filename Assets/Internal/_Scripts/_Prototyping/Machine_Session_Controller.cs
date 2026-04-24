using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Generic turn-tracking system.
/// Keeps track of current turns and a max turn value,
/// and directly updates some UI elements.
/// </summary>
public class Machine_Session_Controller : MonoBehaviour
{
    [Header("Turn Settings")]
    [Tooltip("Maximum number of turns for this context (e.g., per game).")]
    public int maxTurns = 3;

    

    [Header("Hard-Wired UI")]
    [Tooltip("Label to display turns as 'current / max'.")]
    public TextMeshProUGUI turnsText;

    [Tooltip("Optional slider to visualize turns (0-1 normalized).")]
    public Slider turnsSlider;

    [Tooltip("Optional image fill (e.g., radial / bar) to visualize turns (0-1).")]
    public Image turnsFillImage;

    [SerializeField]
    private int currentTurns;

    [Header("Events (Optional)")]
    public UnityEvent<int, int> OnTurnsChanged;      // (current, max)
    public UnityEvent OnTurnsDepleted;              // Called when turns reach 0

    private void Awake()
    {
        ResetTurnsToMax();
    }

    /// <summary>
    /// Current turns remaining (read-only).
    /// </summary>
    public int CurrentTurns => currentTurns;

    /// <summary>
    /// Sets current turns to maxTurns.
    /// </summary>
    public void ResetTurnsToMax()
    {
        currentTurns = Mathf.Max(0, maxTurns);
        InvokeTurnsChanged();
    }

    /// <summary>
    /// Set a new max and optionally reset current to that value.
    /// </summary>
    public void SetMaxTurns(int newMax, bool resetToMax = true)
    {
        maxTurns = Mathf.Max(0, newMax);
        if (resetToMax)
        {
            ResetTurnsToMax();
        }
        else
        {
            currentTurns = Mathf.Clamp(currentTurns, 0, maxTurns);
            InvokeTurnsChanged();
        }
    }

    /// <summary>
    /// Add N turns (can be negative for penalties if you prefer).
    /// </summary>
    public void AddTurns(int amount)
    {
        if (amount == 0) return;

        currentTurns = Mathf.Clamp(currentTurns + amount, 0, maxTurns);
        InvokeTurnsChanged();

        if (currentTurns == 0)
            OnTurnsDepleted?.Invoke();
    }

    /// <summary>
    /// Consume one turn. Returns true if a turn was successfully consumed.
    /// </summary>
    public bool ConsumeTurn()
    {
        return ConsumeTurns(1);
    }

    /// <summary>
    /// Consume N turns. Returns true if at least one turn was consumed.
    /// </summary>
    public bool ConsumeTurns(int amount)
    {
        if (amount <= 0)
            return false;

        if (currentTurns <= 0)
            return false;

        int oldTurns = currentTurns;
        currentTurns = Mathf.Max(0, currentTurns - amount);
        if (currentTurns != oldTurns)
        {
            InvokeTurnsChanged();

            if (currentTurns == 0)
                OnTurnsDepleted?.Invoke();
        }

        return true;
    }

    private void InvokeTurnsChanged()
    {
        // Fire events (if anything still listens)
        OnTurnsChanged?.Invoke(currentTurns, maxTurns);

        // Update hard-wired UI
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Text: "current / max"
        if (turnsText != null)
        {
            turnsText.text = $"{currentTurns} / {maxTurns}";
        }

        // Normalized value (avoid div by zero)
        float normalized = (maxTurns > 0) ? (float)currentTurns / maxTurns : 0f;

        // Slider
        if (turnsSlider != null)
        {
            turnsSlider.minValue = 0f;
            turnsSlider.maxValue = 1f;
            turnsSlider.value = normalized;
        }

        // Image fill
        if (turnsFillImage != null)
        {
            // Assumes Image.type = Filled
            turnsFillImage.fillAmount = normalized;
        }
    }
}