using System.ComponentModel;
using TMPro;
using UnityEngine;

/// <summary>
/// acts as the score system for a single arcade machine
/// </summary>
public class ProtoScoreSystem : MonoBehaviour
{ 
    // sfx
    public AudioSource audioSource;
    public AudioClip scoreUpSFX;
    public AudioClip scoreDownSFX;
    public AudioClip scoreResetSFX;
    public float audioVolume;

    #region SCORE BACKEND

    [SerializeField][ReadOnly(true)] private int currentScoreValue;

    /// <summary>
    /// adds the specified amount of score
    /// </summary>
    /// <param name="amount"> the amount of score to add</param>
    public void AddAmountToScore(int amount)
    {
        Debug.Log($"Added {amount} amount to score");
        currentScoreValue += amount;
        UpdateScoreUI();
    }

    /// <summary>
    /// removes the specified amount of score
    /// </summary>
    /// <param name="amount"> the amount of score to remove </param>
    public void RemoveAmountFromScore(int amount)
    {
        Debug.Log($"Removed {amount} amount from score");
        currentScoreValue -= amount;
        UpdateScoreUI();
    }

    /// <summary>
    /// resets the score to 0
    /// </summary>
    public void ResetScore()
    {
        currentScoreValue = 0;
        UpdateScoreUI();
    }
    #endregion

    #region SCORE USER INTERFACE

    public TMP_Text hudScoreText;

    /// <summary>
    /// updates the ui text (aka updates this machines score UI, probably will be a worldspace canvas)
    /// </summary>
    public void UpdateScoreUI()
    {
        //check for text reference
        if (hudScoreText == null)
        {
            return;
        }

        hudScoreText.text = currentScoreValue.ToString();
    }
    #endregion

    #region SCORE TESTING
    public Vector2 test_randomScoreRange;

    /// <summary>
    /// adds a random amount of score to the players current score
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
    /// removes a random amount of score from theplayers current score
    /// </summary>
    [ContextMenu("Remove Random Score")]
    public void TestRemoveRandomScore()
    {
        // get a random amount, cast to int
        var rand = (int)Random.Range(test_randomScoreRange.x, test_randomScoreRange.y);
        // add the amount to the current score value
        RemoveAmountFromScore(rand);
    }
    
    /// <summary>
    /// resets the player score to 0
    /// </summary>
    [ContextMenu("Reset Score")]
    public void TestResetScore()
    {
        ResetScore();
    }
    #endregion
}
