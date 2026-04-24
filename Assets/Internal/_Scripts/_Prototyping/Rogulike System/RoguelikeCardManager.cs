using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoguelikeCardManager : MonoBehaviour
{
    public static RoguelikeCardManager Instance { get; private set; }

    [Header("Buff Database")]
    public List<BuffData> allBuffs = new List<BuffData>();

    [Header("Screen Root")]
    [Tooltip("Root object for the roguelike UI. This whole object will be enabled/disabled.")]
    public GameObject screenRoot;

    [Header("Card Setup")]
    [Tooltip("Parent transform where card prefabs will be instantiated (has HorizontalLayoutGroup, etc).")]
    public Transform cardRoot;

    [Tooltip("Card prefab that has RoguelikeCardView on it.")]
    public RoguelikeCardView cardPrefab;

    [Tooltip("How many cards to show (MVP = 3).")]
    public int cardCount = 3;

    [Header("Buttons")]
    public Button rerollButton;
    public Button skipButton;

    [Header("Timing")]
    public float selectionTimeoutSeconds = 10f;
    public float focusDurationSeconds = 2f;

    [Header("Events (hooks for machines)")]
    public UnityEvent OnRoguelikeScreenOpened;
    public UnityEvent OnRoguelikeScreenClosed;

    [System.Serializable]
    public class BuffSelectedEvent : UnityEvent<BuffData> { }
    public BuffSelectedEvent OnBuffSelected;

    private bool _isActive;
    private Coroutine _flowRoutine;

    private readonly List<RoguelikeCardView> _spawnedCards = new List<RoguelikeCardView>();
    private readonly List<BuffData> _currentBuffs = new List<BuffData>();

    private BuffData _selectedBuff;
    private RoguelikeCardView _selectedCardView;
    private bool _skipRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (screenRoot != null)
            screenRoot.SetActive(false);

        if (rerollButton != null)
            rerollButton.onClick.AddListener(HandleRerollClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(HandleSkipClicked);
    }

    // --- PUBLIC API ---
    [ContextMenu("Request Roguelike Selection Screen")]
    public void RequestRoguelikeScreen()
    {
        if (_isActive)
            return;

        if (allBuffs == null || allBuffs.Count == 0)
        {
            Debug.LogWarning("[RoguelikeCardManager] No buffs configured.");
            return;
        }

        _flowRoutine = StartCoroutine(RunFlow());
    }

    // --- MAIN FLOW ---

    private IEnumerator RunFlow()
    {
        _isActive = true;
        _selectedBuff = null;
        _selectedCardView = null;
        _skipRequested = false;

        if (screenRoot != null)
            screenRoot.SetActive(true);

        OnRoguelikeScreenOpened?.Invoke();

        SpawnCards();
        RollNewBuffsAndBind();

        float elapsed = 0f;

        // Wait for selection, skip, or timeout
        while (!_skipRequested && _selectedBuff == null && elapsed < selectionTimeoutSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // If skip pressed: close immediately with no buff
        if (_skipRequested)
        {
            CloseScreen();
            yield break;
        }

        // If still nothing selected at timeout, auto-pick a random one
        if (_selectedBuff == null && _currentBuffs.Count > 0)
        {
            int index = Random.Range(0, _currentBuffs.Count);
            _selectedBuff = _currentBuffs[index];
            _selectedCardView = _spawnedCards[index];
        }

        // Focus and event (if we have a selected buff)
        if (_selectedBuff != null && _selectedCardView != null)
        {
            FocusOnCard(_selectedCardView);
            OnBuffSelected?.Invoke(_selectedBuff);
        }

        if (focusDurationSeconds > 0f)
            yield return new WaitForSeconds(focusDurationSeconds);

        CloseScreen();
    }

    private void CloseScreen()
    {
        ClearCardFocus();

        if (screenRoot != null)
            screenRoot.SetActive(false);

        _isActive = false;
        _flowRoutine = null;

        OnRoguelikeScreenClosed?.Invoke();
    }

    // --- CARD SPAWNING / BINDING ---

    private void SpawnCards()
    {
        // Only spawn once for MVP; reuse if already present
        if (_spawnedCards.Count > 0)
            return;

        if (cardRoot == null || cardPrefab == null)
        {
            Debug.LogWarning("[RoguelikeCardManager] cardRoot or cardPrefab not set.");
            return;
        }

        _spawnedCards.Clear();

        for (int i = 0; i < cardCount; i++)
        {
            var cardInstance = Instantiate(cardPrefab, cardRoot);
            cardInstance.onClicked = HandleCardClicked;
            cardInstance.SetFocused(false);
            _spawnedCards.Add(cardInstance);
        }
    }

    private void RollNewBuffsAndBind()
    {
        _currentBuffs.Clear();

        if (allBuffs == null || allBuffs.Count == 0)
            return;

        for (int i = 0; i < _spawnedCards.Count; i++)
        {
            BuffData randomBuff = allBuffs[Random.Range(0, allBuffs.Count)];
            _currentBuffs.Add(randomBuff);

            _spawnedCards[i].Bind(randomBuff);
            _spawnedCards[i].SetFocused(false);
            _spawnedCards[i].gameObject.SetActive(true);
        }
    }

    private void FocusOnCard(RoguelikeCardView selected)
    {
        foreach (var card in _spawnedCards)
        {
            if (card == null) continue;

            bool isSelected = (card == selected);
            card.SetFocused(isSelected);
            card.gameObject.SetActive(isSelected);
        }
    }

    private void ClearCardFocus()
    {
        foreach (var card in _spawnedCards)
        {
            if (card == null) continue;
            card.SetFocused(false);
            card.gameObject.SetActive(true);
        }
    }

    // --- UI CALLBACKS ---

    private void HandleCardClicked(RoguelikeCardView view)
    {
        if (!_isActive) return;
        if (view == null || view.boundBuff == null) return;

        // Lock in selection
        _selectedBuff = view.boundBuff;
        _selectedCardView = view;

        // Let the coroutine handle the rest (focus + close)
    }

    private void HandleRerollClicked()
    {
        if (!_isActive) return;
        RollNewBuffsAndBind();

        // Reset any previous selection
        _selectedBuff = null;
        _selectedCardView = null;
        ClearCardFocus();
    }

    private void HandleSkipClicked()
    {
        if (!_isActive) return;
        _skipRequested = true;
    }
}