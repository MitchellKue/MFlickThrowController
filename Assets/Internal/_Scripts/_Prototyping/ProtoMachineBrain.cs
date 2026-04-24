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

        // handlet tickets dispenssing
        if (dispensedTickets == false)
        {
            ticketGenerator?.GenerateTicketsFromScore(finalScore);

            // let system know tickets were dispensed already
            dispensedTickets = true;
        }

        
    }
}