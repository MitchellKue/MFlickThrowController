using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.ComponentModel;

/// <summary>
/// Stores how many tickets the vendor currently has.
/// Singleton so prize machines / POS can access it easily.
/// Optionally drives a TMP text label showing vendor tickets.
/// </summary>
public class VendorWallet : MonoBehaviour
{
    public static VendorWallet Instance { get; private set; }

    [Header("Tickets")]
    [SerializeField]
    [ReadOnly(true)]
    private int totalTickets;

    [Header("UI (Optional)")]
    [Tooltip("Label that displays the vendor's total ticket count. " +
             "You can also update this from VendorPOS instead.")]
    public TextMeshProUGUI ticketsText;

    /// <summary>
    /// Fired whenever the ticket total changes: (newTotal).
    /// </summary>
    public UnityEvent<int> OnTicketsChanged;

    public int TotalTickets => totalTickets;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple VendorWallet instances found. Destroying duplicate on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional

        InvokeTicketsChanged();
    }

    /// <summary>
    /// Add tickets into the vendor wallet.
    /// </summary>
    public void AddTickets(int amount)
    {
        if (amount <= 0) return;

        totalTickets = Mathf.Max(0, totalTickets + amount);
        InvokeTicketsChanged();
    }

    /// <summary>
    /// Try to spend tickets from the vendor. Returns true on success.
    /// </summary>
    public bool SpendTickets(int amount)
    {
        if (amount <= 0) return false;
        if (amount > totalTickets) return false;

        totalTickets -= amount;
        InvokeTicketsChanged();
        return true;
    }

    /// <summary>
    /// Set tickets to a specific value.
    /// </summary>
    public void SetTickets(int newAmount)
    {
        totalTickets = Mathf.Max(0, newAmount);
        InvokeTicketsChanged();
    }

    private void InvokeTicketsChanged()
    {
        OnTicketsChanged?.Invoke(totalTickets);
        UpdateTicketsText();
    }

    private void UpdateTicketsText()
    {
        if (ticketsText == null) return;
        ticketsText.text = totalTickets.ToString();
    }
}