using UnityEngine;

public class Machine_Brain_Controller : MonoBehaviour
{
    public Machine_Score_Controller scoreSystem;
    public Machine_Ticket_Controller ticketGenerator;
    public Machine_Session_Controller turnSystem;

    private bool dispensedTickets = true;

    private void Start()
    {
        StartNewSession();
    }

    private void Awake()
    {
        if (scoreSystem == null) scoreSystem = GetComponent<Machine_Score_Controller>();
        if (ticketGenerator == null) ticketGenerator = GetComponent<Machine_Ticket_Controller>();
        if (turnSystem == null) turnSystem = GetComponent<Machine_Session_Controller>();

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

        // temporary test. show rogulike system after each successful turn
        RoguelikeCardManager.Instance.RequestRoguelikeScreen();

        scoreSystem.TestAddRandomScore();
    }

    [ContextMenu("Force Session Over")]
    private void HandleSessionOver()
    {
        int finalScore = scoreSystem != null ? scoreSystem.CurrentScoreValue : 0;

        if (!dispensedTickets)
        {
            Debug.Log("dispensing tickets");
            int ticketCount = 0;

            // Use the same calculator that the ticket generator uses
            if (Economy_TicketCalculator.Instance != null)
            {
                ticketCount = Economy_TicketCalculator.Instance.CalculateConversion(finalScore);
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