using UnityEngine;

/// <summary>
/// Spawns a number of ticket prefabs into the scene.
/// Intended to be owned by a machine.
/// The machine passes in its score; this generator uses the ticket
/// calculator to determine how many tickets to spawn, and then
/// spawns them over time with some random force.
/// </summary>
public class Machine_Ticket_Controller : MonoBehaviour
{
    [Header("Ticket Prefab & Spawn")]
    [Tooltip("The ticket prefab that will be instantiated.")]
    public GameObject ticketPrefab;

    [Tooltip("Where tickets should spawn from (e.g., the ticket chute).")]
    public Transform ticketSpawnPoint;

    [Header("Spawn Direction & Force")]
    [Tooltip("Base direction tickets will be pushed in (local or world space).")]
    public Vector3 ticketSpawnDirection = Vector3.forward;

    [Tooltip("Random force range applied along the spawn direction (min, max).")]
    public Vector2 ticketSpawnForceRange = new Vector2(2f, 5f);

    [Tooltip("Random sideways spread applied per ticket (world-space).")]
    public float sidewaysSpread = 0.1f;

    [Header("Spawn Timing")]
    [Tooltip("Random time between ticket spawns (min, max) in seconds.")]
    public Vector2 ticketSpawnIntervalRange = new Vector2(0.03f, 0.12f);

    [Header("Audio (One-Shots)")]
    [Tooltip("AudioSource used to play one-shot ticket dispense sounds (start / single / end).")]
    public AudioSource oneShotAudioSource;

    [Space]
    [Tooltip("Sound played once when a dispense job first starts.")]
    public AudioClip dispenseStartSFX;
    [Range(0f, 1f)]
    public float dispenseStartVolume = 1f;

    [Tooltip("Sound played each time a ticket is spawned.")]
    public AudioClip singleDispenseSFX;
    [Range(0f, 1f)]
    public float singleDispenseVolume = 1f;

    [Tooltip("Sound played once when the current dispense job completes.")]
    public AudioClip dispenseEndSFX;
    [Range(0f, 1f)]
    public float dispenseEndVolume = 1f;

    [Header("Audio (Loop While Dispensing)")]
    [Tooltip("Separate AudioSource used for the looping machine hum while dispensing.")]
    public AudioSource loopAudioSource;

    [Tooltip("Looping sound played while tickets are being dispensed.")]
    public AudioClip dispensingLoopSFX;
    [Range(0f, 1f)]
    public float dispensingLoopVolume = 0.7f;

    private int _ticketsRemaining;
    private bool _isSpawning;
    private bool _loopActive;

    /// <summary>
    /// Called by the machine's logic when the game is over.
    /// </summary>
    /// <param name="machineScore">Final score from this machine.</param>
    public void GenerateTicketsFromScore(int machineScore)
    {
        if (ProtoTicketCalculator.Instance == null)
        {
            Debug.LogWarning("No ProtoTicketCalculator instance found. Cannot generate tickets from score.");
            return;
        }

        int ticketCount = ProtoTicketCalculator.Instance.CalculateConversion(machineScore);
        GenerateTickets(ticketCount);
    }

    /// <summary>
    /// Directly spawn a specified number of tickets.
    /// </summary>
    public void GenerateTickets(int ticketCount)
    {
        if (ticketCount <= 0)
            return;

        bool startingNewJob = !_isSpawning;

        _ticketsRemaining += ticketCount;

        if (startingNewJob)
        {
            PlayDispenseStartSFX();
            StartCoroutine(StartLoopAfterStartSFX());
            StartCoroutine(SpawnTicketsRoutine());
        }
    }

    private System.Collections.IEnumerator SpawnTicketsRoutine()
    {
        _isSpawning = true;

        while (_ticketsRemaining > 0)
        {
            SpawnSingleTicket();
            _ticketsRemaining--;

            float delay = Random.Range(ticketSpawnIntervalRange.x, ticketSpawnIntervalRange.y);
            yield return new WaitForSeconds(delay);
        }

        // Job complete
        PlayDispenseEndSFX();
        StopDispensingLoop();

        _isSpawning = false;
    }

    private void SpawnSingleTicket()
    {
        if (ticketPrefab == null || ticketSpawnPoint == null)
        {
            Debug.LogWarning("Ticket prefab or spawn point not set on ProtoTicketGenerator.");
            return;
        }

        // Instantiate ticket
        GameObject ticketInstance = Instantiate(
            ticketPrefab,
            ticketSpawnPoint.position,
            ticketSpawnPoint.rotation
        );
        ticketInstance.transform.parent = null;

        // Apply force if it has a Rigidbody
        Rigidbody rb = ticketInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = ticketSpawnDirection.normalized;
            float forceMagnitude = Random.Range(ticketSpawnForceRange.x, ticketSpawnForceRange.y);

            // Optional sideways jitter (for a "messy" stack)
            Vector3 sidewaysOffset = new Vector3(
                Random.Range(-sidewaysSpread, sidewaysSpread),
                0f,
                Random.Range(-sidewaysSpread, sidewaysSpread)
            );

            Vector3 force = (direction + sidewaysOffset).normalized * forceMagnitude;
            rb.AddForce(force, ForceMode.Impulse);
        }

        PlaySingleDispenseSFX();
    }

    private void PlayDispenseStartSFX()
    {
        if (oneShotAudioSource == null || dispenseStartSFX == null)
            return;

        oneShotAudioSource.PlayOneShot(dispenseStartSFX, dispenseStartVolume);
    }

    private void PlaySingleDispenseSFX()
    {
        if (oneShotAudioSource == null || singleDispenseSFX == null)
            return;

        oneShotAudioSource.PlayOneShot(singleDispenseSFX, singleDispenseVolume);
    }

    private void PlayDispenseEndSFX()
    {
        if (oneShotAudioSource == null || dispenseEndSFX == null)
            return;

        oneShotAudioSource.PlayOneShot(dispenseEndSFX, dispenseEndVolume);
    }

    private System.Collections.IEnumerator StartLoopAfterStartSFX()
    {
        float delay = 0f;

        if (dispenseStartSFX != null)
        {
            delay = dispenseStartSFX.length;
        }

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        StartDispensingLoop();
    }

    private void StartDispensingLoop()
    {
        if (_loopActive)
            return;

        if (loopAudioSource == null || dispensingLoopSFX == null)
            return;

        loopAudioSource.clip = dispensingLoopSFX;
        loopAudioSource.volume = dispensingLoopVolume;
        loopAudioSource.loop = true;
        loopAudioSource.Play();
        _loopActive = true;
    }

    private void StopDispensingLoop()
    {
        if (!_loopActive)
            return;

        if (loopAudioSource != null)
        {
            loopAudioSource.Stop();
            loopAudioSource.clip = null;
        }

        _loopActive = false;
    }

    // Future version: line renderer / spline to visually represent
    // a coiled strip of tickets on the ground.
    public int testTicketCount = 10;

    [ContextMenu("Generate Test Tickets")]
    public void TestGenerateTickets()
    {
        GenerateTickets(testTicketCount);
    }
}