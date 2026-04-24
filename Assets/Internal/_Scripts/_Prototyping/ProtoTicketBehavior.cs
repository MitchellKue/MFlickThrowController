using UnityEngine;

/// <summary>
/// Basic behavior for a spawned ticket:
/// - Optionally destroys itself shortly after spawning.
/// - Optionally has a chance to be destroyed when it first hits something,
///   except against layers you choose to ignore.
/// </summary>
public class ProtoTicketBehavior : MonoBehaviour
{
    [Header("Lifetime")]
    [Tooltip("Seconds before this ticket is destroyed regardless of anything else.")]
    public float destroyAfterDelay = 3f;

    [Header("Collision")]
    [Range(0, 100)]
    [Tooltip("Percent chance this ticket is destroyed on first collision.")]
    public int destroyOnContactChance = 100;

    [Tooltip("Layers that should NOT trigger the destroy-on-contact behavior.")]
    public LayerMask ignoreDestroyOnLayers;

    private bool _hasCollided;

    private void Start()
    {
        // Hard-destroy after delay no matter what.
        if (destroyAfterDelay > 0f)
        {
            Destroy(gameObject, destroyAfterDelay);
        }
    }

    private bool IsIgnoredLayer(int layer)
    {
        return (ignoreDestroyOnLayers.value & (1 << layer)) != 0;
    }

    public void DestroyOnColBehavior()
    {
        // Only care about the first collision.
        if (_hasCollided)
            return;

        _hasCollided = true;

        // If chance is 0, never destroy on contact.
        if (destroyOnContactChance <= 0)
            return;

        int roll = Random.Range(0, 100); // 0–99
        if (roll < destroyOnContactChance)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If the other object is on an ignored layer, do nothing.
        if (IsIgnoredLayer(collision.gameObject.layer))
            return;

        DestroyOnColBehavior();
    }

    // If we use triggers instead of collisions (e.g., a floor trigger),
    // you can also hook this up:
    private void OnTriggerEnter(Collider other)
    {
        if (IsIgnoredLayer(other.gameObject.layer))
            return;

        DestroyOnColBehavior();
    }
}