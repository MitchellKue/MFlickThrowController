using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.ComponentModel;

/// <summary>
/// Stores how many tickets the player has won (global wallet).
/// Simple singleton for easy access from machines / UI.
/// Also drives a TMP text label showing the ticket count and
/// plays SFX when tickets are added.
/// </summary>
public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [Header("Tickets")]
    [SerializeField]
    [ReadOnly(true)]
    private int totalTickets;

    [Header("UI")]
    [Tooltip("Label that displays the player's total ticket count.")]
    public TextMeshProUGUI ticketsText;

    [Header("Audio - Ticket Gain")]
    [Tooltip("AudioSource used to play the ticket gain sound.")]
    public AudioSource audioSource;

    [Tooltip("Sound played when tickets are added to the wallet.")]
    public AudioClip addTicketsSFX;

    [Range(0f, 1f)]
    [Tooltip("Volume for the ticket gain sound.")]
    public float addTicketsVolume = 1f;

    [Tooltip("Random pitch range applied when tickets are added (min, max).")]
    public Vector2 addTicketsPitchRange = new Vector2(0.95f, 1.05f);

    /// <summary>
    /// Fired whenever the ticket total changes: (newTotal).
    /// </summary>
    public UnityEvent<int> OnTicketsChanged;

    public int TotalTickets => totalTickets;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple PlayerWallet instances found. Destroying duplicate on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist across scenes

        InvokeTicketsChanged();
    }

    /// <summary>
    /// Add tickets to the wallet.
    /// Plays a sound when amount > 0.
    /// </summary>
    public void AddTickets(int amount)
    {
        if (amount <= 0) return;

        totalTickets = Mathf.Max(0, totalTickets + amount);
        InvokeTicketsChanged();
        PlayAddTicketsSFX();
    }

    /// <summary>
    /// Try to spend tickets. Returns true on success.
    /// (No SFX here unless you want to add one later.)
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
    /// (No SFX here; this is more of a system/debug action.)
    /// </summary>
    public void SetTickets(int newAmount)
    {
        totalTickets = Mathf.Max(0, newAmount);
        InvokeTicketsChanged();
    }

    private void InvokeTicketsChanged()
    {
        // Fire event
        OnTicketsChanged?.Invoke(totalTickets);

        // Update UI
        UpdateTicketsText();
    }

    private void UpdateTicketsText()
    {
        if (ticketsText == null) return;

        // Simple numeric display; customize formatting if you want.
        ticketsText.text = totalTickets.ToString();
    }

    private void PlayAddTicketsSFX()
    {
        if (audioSource == null || addTicketsSFX == null)
            return;

        // Save original pitch
        float originalPitch = audioSource.pitch;

        // Randomize pitch in range
        float minPitch = Mathf.Min(addTicketsPitchRange.x, addTicketsPitchRange.y);
        float maxPitch = Mathf.Max(addTicketsPitchRange.x, addTicketsPitchRange.y);
        float randomPitch = Random.Range(minPitch, maxPitch);

        audioSource.pitch = randomPitch;
        audioSource.PlayOneShot(addTicketsSFX, addTicketsVolume);

        // Restore pitch
        audioSource.pitch = originalPitch;
    }

    #region TESTING

    public Vector2 testAmount = new Vector2(100, 1000);

    public int GetRandomAmount(float a, float b)
    {
        int amount = (int)Random.Range(a, b);
        return amount;
    }

    [ContextMenu("Test Add Tickets")]
    public void TestAddTickets()
    {
        AddTickets(GetRandomAmount(testAmount.x, testAmount.y));
    }

    [ContextMenu("Test Spend Tickets")]
    public void TestSpendTickets()
    {
        SpendTickets(GetRandomAmount(testAmount.x, testAmount.y));
    }

    [ContextMenu("Test Set Tickets")]
    public void TestSetTickets()
    {
        SetTickets(GetRandomAmount(testAmount.x, testAmount.y));
    }

    #endregion
}