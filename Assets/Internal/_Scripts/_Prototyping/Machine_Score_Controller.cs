using System.ComponentModel;
using TMPro;
using UnityEngine;

/// <summary>
/// Acts as the score system for a single arcade machine.
/// Handles score logic, UI, and SFX for score changes.
/// </summary>
public class Machine_Score_Controller : MonoBehaviour
{
    #region AUDIO

    [Header("Score Audio")]
    [Tooltip("AudioSource used to play score SFX.")]
    public AudioSource audioSource;

    [Tooltip("Played when score goes up.")]
    public AudioClip scoreUpSFX;

    [Tooltip("Played when score goes down.")]
    public AudioClip scoreDownSFX;

    [Tooltip("Played when score is reset.")]
    public AudioClip scoreResetSFX;

    [Range(0f, 1f)]
    [Tooltip("Volume for score SFX.")]
    public float audioVolume = 1f;

    [Tooltip("Random pitch range applied to score SFX (min, max).")]
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    #endregion

    #region SCORE BACKEND

    [SerializeField]
    [ReadOnly(true)]
    private int currentScoreValue;

    /// <summary>
    /// Current score value for this machine (read-only).
    /// </summary>
    public int CurrentScoreValue => currentScoreValue;

    /// <summary>
    /// Adds the specified amount of score.
    /// </summary>
    /// <param name="amount">The amount of score to add.</param>
    public void AddAmountToScore(int amount)
    {
        Debug.Log($"Added {amount} amount to score");
        currentScoreValue += amount;
        UpdateScoreUI();
        PlayScoreUpSFX();
    }

    /// <summary>
    /// Removes the specified amount of score.
    /// </summary>
    /// <param name="amount">The amount of score to remove.</param>
    public void RemoveAmountFromScore(int amount)
    {
        Debug.Log($"Removed {amount} amount from score");
        currentScoreValue -= amount;
        UpdateScoreUI();
        PlayScoreDownSFX();
    }

    /// <summary>
    /// Resets the score to 0.
    /// </summary>
    public void ResetScore()
    {
        currentScoreValue = 0;
        UpdateScoreUI();
        PlayScoreResetSFX();
    }

    #endregion

    #region SCORE USER INTERFACE

    [Header("Score UI")]
    [Tooltip("UI text that displays this machine's score (e.g., worldspace canvas).")]
    public TMP_Text hudScoreText;

    /// <summary>
    /// Updates the UI text (this machine's score UI).
    /// </summary>
    public void UpdateScoreUI()
    {
        // check for text reference
        if (hudScoreText == null)
            return;

        hudScoreText.text = currentScoreValue.ToString();
    }

    #endregion

    #region AUDIO HELPERS

    private void PlayScoreUpSFX()
    {
        PlaySFX(scoreUpSFX);
    }

    private void PlayScoreDownSFX()
    {
        PlaySFX(scoreDownSFX);
    }

    private void PlayScoreResetSFX()
    {
        PlaySFX(scoreResetSFX);
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
        audioSource.PlayOneShot(clip, audioVolume);
        audioSource.pitch = originalPitch;
    }

    #endregion

    #region SCORE TESTING

    [Header("Testing")]
    public Vector2 test_randomScoreRange;

    /// <summary>
    /// Adds a random amount of score to the player's current score.
    /// </summary>
    [ContextMenu("Add Random Score")]
    public void TestAddRandomScore()
    {
        // get a random amount, cast to int
        var rand = (int)Random.Range(test_randomScoreRange.x, test_randomScoreRange.y);
        // add the amount to the current score value
        AddAmountToScore(rand);
    }

    /// <summary>
    /// Removes a random amount of score from the player's current score.
    /// </summary>
    [ContextMenu("Remove Random Score")]
    public void TestRemoveRandomScore()
    {
        // get a random amount, cast to int
        var rand = (int)Random.Range(test_randomScoreRange.x, test_randomScoreRange.y);
        // remove the amount from the current score value
        RemoveAmountFromScore(rand);
    }

    /// <summary>
    /// Resets the player score to 0.
    /// </summary>
    [ContextMenu("Reset Score")]
    public void TestResetScore()
    {
        ResetScore();
    }

    #endregion
}