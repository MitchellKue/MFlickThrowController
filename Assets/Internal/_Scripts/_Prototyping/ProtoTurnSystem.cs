using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generic turn-tracking system.
/// Keeps track of current turns and a max turn value,
/// and provides methods to add/remove/reset.
/// </summary>
public class ProtoTurnSystem : MonoBehaviour
{
    [Header("Turn Settings")]
    [Tooltip("Maximum number of turns for this context (e.g., per game).")]
    public int maxTurns = 3;

    [SerializeField]
    [ReadOnly(true)]
    private int currentTurns;

    [Header("Events")]
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
        OnTurnsChanged?.Invoke(currentTurns, maxTurns);
    }
}