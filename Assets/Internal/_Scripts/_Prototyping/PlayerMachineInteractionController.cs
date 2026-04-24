using UnityEngine;

/// <summary>
/// Handles player interaction with nearby ProtoMachineInteractable machines.
/// </summary>
public class PlayerMachineInteractionController : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("How far to check for machines.")]
    public float detectionRadius = 3f;

    [Tooltip("How often (in seconds) to do the sphere check.")]
    public float checkInterval = 0.2f;

    [Tooltip("Layer mask used to filter which objects are considered machines.")]
    public LayerMask machineLayerMask;

    [Tooltip("Optional: origin for sphere checks. If null, uses this transform's position.")]
    public Transform detectionOrigin;

    [Header("Input")]
    [Tooltip("Key to start/stop interacting with a machine.")]
    public KeyCode interactKey = KeyCode.E;

    /// <summary>
    /// The closest machine currently in range (and not occupied), or null.
    /// Use this to drive UI prompts.
    /// </summary>
    public ProtoMachineInteractable CurrentMachineInRange { get; private set; }

    /// <summary>
    /// Machine the player is currently occupying, or null.
    /// </summary>
    public ProtoMachineInteractable OccupiedMachine { get; private set; }

    private float _checkTimer = 0f;

    private void Update()
    {
        HandleDetectionTimer();
        HandleInteractionInput();
    }

    #region Detection

    private void HandleDetectionTimer()
    {
        _checkTimer -= Time.deltaTime;
        if (_checkTimer <= 0f)
        {
            _checkTimer = checkInterval;
            UpdateMachineInRange();
        }
    }

    private void UpdateMachineInRange()
    {
        Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;

        // Find colliders in radius
        Collider[] hits = Physics.OverlapSphere(origin, detectionRadius, machineLayerMask, QueryTriggerInteraction.Collide);

        ProtoMachineInteractable previousMachine = CurrentMachineInRange;
        ProtoMachineInteractable closestMachine = null;
        float closestSqrDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            ProtoMachineInteractable machine = hit.GetComponentInParent<ProtoMachineInteractable>();
            if (machine == null)
                continue;

            // Skip machines that are already occupied by someone else
            if (!machine.CanInteract() && machine != OccupiedMachine)
                continue;

            float sqrDist = (machine.transform.position - origin).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closestMachine = machine;
            }
        }

        // Only one machine at a time: the closest one
        CurrentMachineInRange = closestMachine;

        // toggle canvas on focus change 
        if (previousMachine != CurrentMachineInRange)
        {
            if (previousMachine != null)
                previousMachine.SetHighlighted(false);

            if (CurrentMachineInRange != null)
                CurrentMachineInRange.SetHighlighted(true);
        }

    }

    #endregion

    #region Interaction

    private void HandleInteractionInput()
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        // If currently using a machine, pressing interact attempts to exit
        if (OccupiedMachine != null)
        {
            bool released = OccupiedMachine.TryRelease();
            if (released)
            {
                OccupiedMachine = null;
            }

            return;
        }

        // Otherwise, if we have a machine in range, attempt to occupy
        if (CurrentMachineInRange != null && CurrentMachineInRange.CanInteract())
        {
            bool occupied = CurrentMachineInRange.TryOccupy(gameObject);
            if (occupied)
            {
                OccupiedMachine = CurrentMachineInRange;
            }
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
        Gizmos.DrawWireSphere(origin, detectionRadius);
    }

    #endregion
}