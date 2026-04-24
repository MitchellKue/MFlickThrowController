using UnityEngine;

/// <summary>
/// Provides a base for how score is converted into ticket value.
/// This is implemented as a singleton.
/// </summary>
public class ProtoTicketCalculator : MonoBehaviour
{
    // Static instance
    public static ProtoTicketCalculator Instance { get; private set; }

    [Header("Ticket Conversion")]
    [Tooltip("Number of tickets awarded per point of machine score.")]
    public float ticketConversionRate = 1.0f;

    private void Awake()
    {
        // Enforce singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                $"Duplicate {nameof(ProtoTicketCalculator)} found on {gameObject.name}. " +
                "Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Converts a machine score into tickets using the conversion rate.
    /// </summary>
    public int CalculateConversion(int machineScore)
    {
        // Basic safety: negative scores yield 0 tickets
        if (machineScore <= 0 || ticketConversionRate <= 0f)
            return 0;

        float tickets = machineScore * ticketConversionRate;
        //return Mathf.FloorToInt(tickets); // or 
        return Mathf.RoundToInt(tickets); 
    }
}