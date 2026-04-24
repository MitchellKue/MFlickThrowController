using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Represents the cash register / POS object.
/// Handles player clicks to transfer tickets between PlayerWallet and VendorWallet,
/// displays vendor ticket count, and plays SFX for transfers.
/// </summary>
[RequireComponent(typeof(Collider))]
public class VendorPOS : MonoBehaviour
{
    [Header("References")]
    public PlayerWallet playerWallet;
    public VendorWallet vendorWallet;

    [Tooltip("Text that shows how many tickets are currently stored with the vendor.")]
    public TextMeshProUGUI vendorTicketsText;

    [Header("Transfer Settings")]
    [Tooltip("Tickets transferred per single click.")]
    public int ticketsPerClick = 1;

    [Tooltip("Tickets transferred per second while holding the button.")]
    public float ticketsPerSecond = 10f;

    [Header("Audio")]
    [Tooltip("AudioSource used to play transfer SFX.")]
    public AudioSource audioSource;

    [Tooltip("SFX when tickets move from Player → Vendor (throwing tickets at vendor).")]
    public AudioClip transferToVendorSFX;

    [Tooltip("SFX when tickets move from Vendor → Player (taking tickets back).")]
    public AudioClip transferToPlayerSFX;

    [Range(0f, 1f)]
    [Tooltip("Volume for transfer SFX.")]
    public float transferVolume = 1f;

    [Tooltip("Random pitch range for transfer SFX (min, max).")]
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    // Internal
    private float _continuousTransferTimerLeft;   // for left button (to vendor)
    private float _continuousTransferTimerRight;  // for right button (to player)

    private void Awake()
    {
        if (playerWallet == null)
            playerWallet = PlayerWallet.Instance;

        if (vendorWallet == null)
            vendorWallet = VendorWallet.Instance;

        if (vendorWallet != null)
        {
            // Update our register text whenever vendor tickets change
            vendorWallet.OnTicketsChanged.AddListener(UpdateVendorTicketsUI);
            UpdateVendorTicketsUI(vendorWallet.TotalTickets);
        }
    }

    private void OnDestroy()
    {
        if (vendorWallet != null)
        {
            vendorWallet.OnTicketsChanged.RemoveListener(UpdateVendorTicketsUI);
        }
    }

    private void Update()
    {
        // Continuous transfer requires mouse over the register.
        // We use OnMouseOver to gate it (see below).
        // Nothing in Update itself; logic is in OnMouseOver.
    }

    /// <summary>
    /// Unity callback when mouse is over this object (requires a Collider).
    /// We use this for click + hold behavior.
    /// </summary>
    private void OnMouseOver()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // LEFT MOUSE (Player → Vendor)
        if (Input.GetMouseButtonDown(0))
        {
            // Single click
            TransferPlayerToVendor(ticketsPerClick);
        }
        if (Input.GetMouseButton(0))
        {
            // Continuous transfer while held
            _continuousTransferTimerLeft += dt;
            float interval = 1f / Mathf.Max(0.01f, ticketsPerSecond);

            while (_continuousTransferTimerLeft >= interval)
            {
                _continuousTransferTimerLeft -= interval;

                if (!TransferPlayerToVendor(1))
                    break; // Stop if we ran out of tickets
            }
        }
        else
        {
            _continuousTransferTimerLeft = 0f;
        }

        // RIGHT MOUSE (Vendor → Player)
        if (Input.GetMouseButtonDown(1))
        {
            // Single click
            TransferVendorToPlayer(ticketsPerClick);
        }
        if (Input.GetMouseButton(1))
        {
            _continuousTransferTimerRight += dt;
            float interval = 1f / Mathf.Max(0.01f, ticketsPerSecond);

            while (_continuousTransferTimerRight >= interval)
            {
                _continuousTransferTimerRight -= interval;

                if (!TransferVendorToPlayer(1))
                    break; // Stop if vendor out of tickets
            }
        }
        else
        {
            _continuousTransferTimerRight = 0f;
        }
    }

    /// <summary>
    /// Transfer tickets from PlayerWallet → VendorWallet.
    /// Returns true if at least one ticket was transferred.
    /// </summary>
    private bool TransferPlayerToVendor(int amount)
    {
        if (amount <= 0) return false;
        if (playerWallet == null || vendorWallet == null) return false;

        // Try to spend from player
        if (!playerWallet.SpendTickets(amount))
            return false;

        // Add to vendor
        vendorWallet.AddTickets(amount);

        PlayTransferToVendorSFX();
        return true;
    }

    /// <summary>
    /// Transfer tickets from VendorWallet → PlayerWallet.
    /// Returns true if at least one ticket was transferred.
    /// </summary>
    private bool TransferVendorToPlayer(int amount)
    {
        if (amount <= 0) return false;
        if (playerWallet == null || vendorWallet == null) return false;

        // Try to spend from vendor
        if (!vendorWallet.SpendTickets(amount))
            return false;

        // Add back to player
        playerWallet.AddTickets(amount);

        PlayTransferToPlayerSFX();
        return true;
    }

    private void UpdateVendorTicketsUI(int value)
    {
        if (vendorTicketsText == null) return;
        vendorTicketsText.text = value.ToString();
    }

    #region Audio Helpers

    private void PlayTransferToVendorSFX()
    {
        PlaySFX(transferToVendorSFX);
    }

    private void PlayTransferToPlayerSFX()
    {
        PlaySFX(transferToPlayerSFX);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        float originalPitch = audioSource.pitch;

        float minPitch = Mathf.Min(pitchRange.x, pitchRange.y);
        float maxPitch = Mathf.Max(pitchRange.x, pitchRange.y);
        float randomPitch = Random.Range(minPitch, maxPitch);

        audioSource.pitch = randomPitch;
        audioSource.PlayOneShot(clip, transferVolume);
        audioSource.pitch = originalPitch;
    }

    #endregion
}