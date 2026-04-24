using Kojiko.MCharacterController.Camera;
using Kojiko.MCharacterController.Core;
using UnityEngine;

/// <summary>
/// Represents an interactable machine in the world.
/// Handles occupying / releasing the local player.
/// </summary>
public class Machine_Interaction_Controller : MonoBehaviour
{
    [Header("Machine State")]
    [Tooltip("True if a player is currently using this machine.")]
    public bool isOccupied = false;

    [Header("Interaction")]
    [Tooltip("Where we place the player when they start using this machine.")]
    public Transform interactionSpot;
    public GameObject occupiedIndicatorImage;    // enabled when a player is occupying this machine

    // Cached player components while occupied
    private GameObject playerGO;
    private MCharacter_Motor_Controller playerMotorModule;
    private CameraRig_Base playerCameraModule;

    [Header("Visuals")]
    [Tooltip("Canvas that should be enabled when the player is near this machine.")]
    public Canvas machineCanvas;   // assign in inspector

    private void Awake()
    {
        // Optional: auto-find canvas if not assigned
        if (machineCanvas == null)
        {
            machineCanvas = GetComponentInChildren<Canvas>(true);
        }

        if (occupiedIndicatorImage != null)
        {
            occupiedIndicatorImage.SetActive(false);
        }

        // Start hidden by default (only show when focused)
        SetHighlighted(false);
    }

    /// <summary>
    /// Attempt to occupy this machine with the given player GameObject.
    /// Returns true if occupation succeeded, false if already occupied or invalid.
    /// </summary>
    public bool TryOccupy(GameObject player)
    {
        // already in use
        if (isOccupied)
            return false;

        if (player == null)
            return false;

        playerGO = player;

        

        // Cache and disable player motor
        playerMotorModule = playerGO.GetComponent<MCharacter_Motor_Controller>();
        if (playerMotorModule != null)
        {
            playerMotorModule.canUse = false;
            
        }

        // Move player to interaction spot (if defined)
        if (interactionSpot != null)
        {
            playerMotorModule.TeleportToPoint(interactionSpot);
        }

        // Cache and disable player camera controller
        playerCameraModule = playerGO.GetComponent<CameraRig_FPV>();
        if (playerCameraModule != null)
        {
            playerCameraModule.gameObject.SetActive(false);
        }

        isOccupied = true;

        if (occupiedIndicatorImage != null)
        {
            occupiedIndicatorImage.SetActive(true);
        }

        return true;
    }

    /// <summary>
    /// Attempt to release the currently occupying player.
    /// Returns true if release succeeded.
    /// </summary>
    public bool TryRelease()
    {
        if (!isOccupied)
            return false;

        // Re-enable player motor
        if (playerMotorModule != null)
        {
            playerMotorModule.canUse = true;
        }

        // Re-enable camera controller
        if (playerCameraModule != null)
        {
            playerCameraModule.gameObject.SetActive(true);
        }

        // Clear references
        playerMotorModule = null;
        playerCameraModule = null;
        playerGO = null;

        isOccupied = false;

        if (occupiedIndicatorImage != null)
        {
            occupiedIndicatorImage.SetActive(false);
        }

        return true;
    }

    /// <summary>
    /// Convenience: can this machine be interacted with right now?
    /// </summary>
    public bool CanInteract()
    {
        return !isOccupied;
    }

    /// <summary>
    /// Called when this machine becomes / stops being the closest machine
    /// to the player. Use this to toggle your canvas.
    /// </summary>
    public void SetHighlighted(bool isHighlighted)
    {
        if (machineCanvas != null)
        {
            machineCanvas.enabled = isHighlighted;
        }
    }
}