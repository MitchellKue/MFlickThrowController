using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Global manager for showing a simple roguelike buff selection screen.
/// - Randomizes 3 BuffData cards from a list
/// - Handles click selection, reroll, skip
/// - Auto-selects if timer expires
/// - Fires events for open/close and selection
/// - Ignores extra calls while active
/// </summary>
public class RoguelikeCardManager : MonoBehaviour
{
    public static RoguelikeCardManager Instance { get; private set; }

    [Header("Buff Database")]
    [Tooltip("All available global buffs for random selection.")]
    public List<BuffData> allBuffs = new List<BuffData>();

    [Header("UI Root")]
    [Tooltip("Root canvas/panel for the roguelike screen.")]
    public GameObject screenRoot;

    [Tooltip("Three card views used to display random buffs.")]
    public RoguelikeCardView[] cardViews = new RoguelikeCardView[3];

    [Header("Buttons")]
    public Button rerollButton;
    public Button skipButton;

    [Header("Timing")]
    [Tooltip("How long the player has to pick a card before auto-select (seconds).")]
    public float selectionTimeoutSeconds = 10f;

    [Tooltip("How long to focus on selected card before closing (seconds).")]
    public float focusDurationSeconds = 2f;

    [Header("Events (hooks for machines)")]
    public UnityEvent OnRoguelikeScreenOpened;
    public UnityEvent OnRoguelikeScreenClosed;

    /// <summary>
    /// Called when a buff is chosen (either by click, auto-select, or reroll + click).
    /// </summary>
    [System.Serializable]
    public class BuffSelectedEvent : UnityEvent<BuffData> { }
    public BuffSelectedEvent OnBuffSelected;

    private bool _isScreenActive;
    private Coroutine _activeRoutine;
    private BuffData[] _currentBuffChoices = new BuffData[3];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (screenRoot != null)
            screenRoot.SetActive(false);

        SetupButtons();
        SetupCardClickHandlers();
    }

    private void SetupButtons()
    {
        if (rerollButton != null)
            rerollButton.onClick.AddListener(HandleRerollClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(HandleSkipClicked);
    }

    private void SetupCardClickHandlers()
    {
        foreach (var view in cardViews)
        {
            if (view == null) continue;
            view.onClicked = HandleCardClicked;
        }
    }

    // --- PUBLIC API ---

    /// <summary>
    /// Called by machines to show the roguelike buff selection screen.
    /// Simple MVP: ignores call if a screen is already active.
    /// </summary>
    public void RequestRoguelikeScreen()
    {
        if (_isScreenActive)
            return;

        if (allBuffs == null || allBuffs.Count == 0)
        {
            Debug.LogWarning("[RoguelikeCardManager] No buffs configured.");
            return;
        }

        _activeRoutine = StartCoroutine(RunRoguelikeFlow());
    }

    // --- FLOW ---

    private IEnumerator RunRoguelikeFlow()
    {
        _isScreenActive = true;

        if (screenRoot != null)
            screenRoot.SetActive(true);

        OnRoguelikeScreenOpened?.Invoke();

        // Randomize 3 cards and show them
        RollNewCards();

        // Selection phase
        BuffData selectedBuff = null;
        RoguelikeCardView selectedView = null;

        float elapsed = 0f;

        // Wait for user to pick, skip, or timeout
        while (selectedBuff == null && elapsed < selectionTimeoutSeconds)
        {
            // selection and skip are driven by UI callbacks that set selectedBuff / selectedView
            elapsed += Time.deltaTime;
            yield return null;
        }

        // If still nothing selected (no click, no skip), auto-pick a random card
        if (selectedBuff == null)
        {
            int idx = Random.Range(0, _currentBuffChoices.Length);
            selectedBuff = _currentBuffChoices[idx];
            selectedView = cardViews[idx];
        }

        // Focus on selected card (if not a skip)
        if (selectedBuff != null)
        {
            FocusOnSelectedCard(selectedView);

            // Let systems know what was chosen (even if we're not using it yet)
            OnBuffSelected?.Invoke(selectedBuff);
        }

        // Wait focus duration
        if (focusDurationSeconds > 0f)
            yield return new WaitForSeconds(focusDurationSeconds);

        // Close screen
        if (screenRoot != null)
            screenRoot.SetActive(false);

        // Reset states
        ClearCardFocus();
        _isScreenActive = false;
        _activeRoutine = null;

        OnRoguelikeScreenClosed?.Invoke();
    }

    // --- Randomization / UI ---

    private void RollNewCards()
    {
        if (allBuffs == null || allBuffs.Count == 0)
            return;

        for (int i = 0; i < cardViews.Length; i++)
        {
            BuffData randomBuff = allBuffs[Random.Range(0, allBuffs.Count)];
            _currentBuffChoices[i] = randomBuff;

            if (cardViews[i] != null)
            {
                cardViews[i].Bind(randomBuff);
                cardViews[i].SetFocused(false);
                cardViews[i].gameObject.SetActive(true);
            }
        }
    }

    private void FocusOnSelectedCard(RoguelikeCardView selectedView)
    {
        for (int i = 0; i < cardViews.Length; i++)
        {
            var view = cardViews[i];
            if (view == null) continue;

            bool isSelected = (view == selectedView);
            view.SetFocused(isSelected);
            view.gameObject.SetActive(isSelected);
        }
    }

    private void ClearCardFocus()
    {
        foreach (var view in cardViews)
        {
            if (view == null) continue;
            view.SetFocused(false);
            view.gameObject.SetActive(true);
        }
    }

    // --- UI Callbacks ---

    private void HandleCardClicked(RoguelikeCardView view)
    {
        if (!_isScreenActive) return;
        if (view == null || view.boundBuff == null) return;

        // Immediately lock in selection and end selection phase
        // by jumping the timer in the coroutine via direct selection.
        // Implementation: store it and end selection loop by
        // "short-circuiting" via a helper.
        StartCoroutine(SelectAndEndFlow(view.boundBuff, view));
    }

    private IEnumerator SelectAndEndFlow(BuffData buff, RoguelikeCardView view)
    {
        // If we're already mid-flow, just set state, the main
        // coroutine will read it on next frame.
        // Simpler: stop current flow and restart focus+close manually.
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
        }

        _activeRoutine = null;

        _isScreenActive = true; // still active until we close

        // Focus, event, wait, close
        FocusOnSelectedCard(view);
        OnBuffSelected?.Invoke(buff);

        if (focusDurationSeconds > 0f)
            yield return new WaitForSeconds(focusDurationSeconds);

        if (screenRoot != null)
            screenRoot.SetActive(false);

        ClearCardFocus();
        _isScreenActive = false;

        OnRoguelikeScreenClosed?.Invoke();
    }

    private void HandleRerollClicked()
    {
        if (!_isScreenActive) return;
        RollNewCards();
    }

    private void HandleSkipClicked()
    {
        if (!_isScreenActive) return;

        // Treat skip as "no buff selected":
        // Close immediately without a focused card.
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        _activeRoutine = null;

        if (screenRoot != null)
            screenRoot.SetActive(false);

        ClearCardFocus();
        _isScreenActive = false;

        // No OnBuffSelected call for skip
        OnRoguelikeScreenClosed?.Invoke();
    }
}