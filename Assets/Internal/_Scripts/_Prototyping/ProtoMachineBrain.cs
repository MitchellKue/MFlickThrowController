using UnityEngine;

public class ProtoMachineBrain : MonoBehaviour
{
    public ProtoScoreSystem scoreSystem;
    public ProtoTicketGenerator ticketGenerator;
    public ProtoTurnSystem turnSystem;

    private bool dispensedTickets = true;

    private void Awake()
    {
        if (scoreSystem == null) scoreSystem = GetComponent<ProtoScoreSystem>();
        if (ticketGenerator == null) ticketGenerator = GetComponent<ProtoTicketGenerator>();
        if (turnSystem == null) turnSystem = GetComponent<ProtoTurnSystem>();

        // Subscribe to turn events
        if (turnSystem != null)
        {
            turnSystem.OnTurnsDepleted.AddListener(HandleSessionOver);
        }
    }

    [ContextMenu("Start New Session")]
    public void StartNewSession()
    {
        scoreSystem?.ResetScore();
        turnSystem?.ResetTurnsToMax();

        dispensedTickets = false;
        // add start up sfx
    }

    [ContextMenu("Use a turn")]
    public void OnPlayerUsedTurn()
    {
        turnSystem?.ConsumeTurn();
    }

    [ContextMenu("Force Session Over")]
    private void HandleSessionOver()
    {
        int finalScore = scoreSystem != null ? scoreSystem.CurrentScoreValue : 0;

        if (!dispensedTickets)
        {
            int ticketCount = 0;

            // Use the same calculator that the ticket generator uses
            if (ProtoTicketCalculator.Instance != null)
            {
                ticketCount = ProtoTicketCalculator.Instance.CalculateConversion(finalScore);
            }
            else
            {
                Debug.LogWarning("No ProtoTicketCalculator instance found. Cannot compute ticketCount from score.");
            }

            // Physically spawn tickets
            if (ticketGenerator != null && ticketCount > 0)
            {
                ticketGenerator.GenerateTickets(ticketCount);
            }

            // Add tickets to player wallet
            if (PlayerWallet.Instance != null && ticketCount > 0)
            {
                PlayerWallet.Instance.AddTickets(ticketCount);
            }
            else if (PlayerWallet.Instance == null)
            {
                Debug.LogWarning("No PlayerWallet instance found. Tickets will not be stored.");
            }

            dispensedTickets = true;
        }

        // add session over sfx
    }
}