using UnityEngine;
using TMPro;

/// <summary>
/// Represents a prize the player can purchase at the redemption counter.
/// Clicking this object attempts to spend tickets from the VendorWallet
/// and spawns the prize at a specified location if successful.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PrizeItem : MonoBehaviour
{
    [Header("Prize Settings")]
    [Tooltip("Ticket cost required to purchase this prize.")]
    public int ticketCost = 100;

    [Tooltip("Prefab to spawn when this prize is purchased.")]
    public GameObject prizePrefab;

    [Tooltip("Where to spawn the prize. If null, will spawn at this object's position.")]
    public Transform spawnPoint;

    [Header("Wallet")]
    [Tooltip("Vendor wallet that stores the tickets used for purchases. " +
             "If null, will try to use VendorWallet.Instance.")]
    public VendorWallet vendorWallet;

    [Header("UI (Optional)")]
    [Tooltip("Label that shows the cost (e.g. 'Cost: 100').")]
    public TextMeshProUGUI costLabel;

    [Header("Audio (Optional)")]
    [Tooltip("AudioSource used to play purchase / fail SFX.")]
    public AudioSource audioSource;

    [Tooltip("SFX played on successful purchase.")]
    public AudioClip purchaseSFX;

    [Tooltip("SFX played when purchase fails (not enough tickets).")]
    public AudioClip purchaseFailedSFX;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private void Awake()
    {
        if (vendorWallet == null)
            vendorWallet = VendorWallet.Instance;

        UpdateCostLabel();
    }

    private void OnValidate()
    {
        // Keep label up to date in editor when you tweak the cost.
        UpdateCostLabel();
    }

    private void UpdateCostLabel()
    {
        if (costLabel == null) return;
        costLabel.text = ticketCost.ToString();
    }

    /// <summary>
    /// Called when the player clicks on this prize in the world.
    /// Requires a collider on the same GameObject and a Camera with raycasts enabled.
    /// </summary>
    private void OnMouseDown()
    {
        TryPurchase();
    }

    /// <summary>
    /// Public method so you can hook this up to a UI button if needed.
    /// </summary>
    public void TryPurchase()
    {
        if (vendorWallet == null)
        {
            Debug.LogWarning($"[PrizeItem] No VendorWallet available for {name}.");
            PlayFailSFX();
            return;
        }

        if (ticketCost <= 0)
        {
            Debug.LogWarning($"[PrizeItem] Ticket cost for {name} is <= 0. Allowing free purchase.");
            SpawnPrize();
            PlayPurchaseSFX();
            return;
        }

        // Try to spend tickets from the vendor
        bool success = vendorWallet.SpendTickets(ticketCost);
        if (success)
        {
            Debug.Log($"[PrizeItem] Purchased {name} for {ticketCost} tickets.");
            SpawnPrize();
            PlayPurchaseSFX();
        }
        else
        {
            Debug.Log($"[PrizeItem] Not enough tickets to buy {name}. Cost: {ticketCost}, Vendor has: {vendorWallet.TotalTickets}");
            PlayFailSFX();
        }
    }

    private void SpawnPrize()
    {
        if (prizePrefab == null)
        {
            Debug.LogWarning($"[PrizeItem] No prizePrefab assigned on {name}. Nothing to spawn.");
            return;
        }

        Vector3 pos;
        Quaternion rot;

        if (spawnPoint != null)
        {
            pos = spawnPoint.position;
            rot = spawnPoint.rotation;
        }
        else
        {
            pos = transform.position;
            rot = transform.rotation;
        }

        Instantiate(prizePrefab, pos, rot);
    }

    private void PlayPurchaseSFX()
    {
        PlaySFX(purchaseSFX);
    }

    private void PlayFailSFX()
    {
        PlaySFX(purchaseFailedSFX);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip, sfxVolume);
    }
}