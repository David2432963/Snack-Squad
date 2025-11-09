using OSK;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class GameData_Manager : MonoBehaviour
{
    [Title("Player Scores")]
    [SerializeField, ReadOnly] private Dictionary<EPlayerType, int> playerScores = new Dictionary<EPlayerType, int>();

    // Main player score for backwards compatibility
    public int CurrentScore => playerScores.ContainsKey(EPlayerType.Player) ? playerScores[EPlayerType.Player] : 0;

    // Individual player score access
    public int GetPlayerScore(EPlayerType playerType) => playerScores.ContainsKey(playerType) ? playerScores[playerType] : 0;
    public int Bot1Score => GetPlayerScore(EPlayerType.Bot1);
    public int Bot2Score => GetPlayerScore(EPlayerType.Bot2);

    // Total score for all players
    public int TotalScore
    {
        get
        {
            int total = 0;
            foreach (var score in playerScores.Values)
            {
                total += score;
            }
            return total;
        }
    }

    public static GameData_Manager Instance => SingletonManager.Instance.Get<GameData_Manager>();

    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }

    private void Start()
    {
        // Initialize all player scores to 0
        InitializePlayerScores();
    }

    private void InitializePlayerScores()
    {
        playerScores.Clear();
        playerScores[EPlayerType.Player] = 0;
        playerScores[EPlayerType.Bot1] = 0;
        playerScores[EPlayerType.Bot2] = 0;
    }

    // Backwards compatibility method for main player
    public void AddScore(int amount)
    {
        AddScore(EPlayerType.Player, amount);
    }

    // New method for specific player scoring
    public void AddScore(EPlayerType playerType, int amount)
    {
        if (!playerScores.ContainsKey(playerType))
        {
            playerScores[playerType] = 0;
        }

        playerScores[playerType] += amount;

        // Notify different events based on player type
        switch (playerType)
        {
            case EPlayerType.Player:
                Main.Observer.Notify(EEvent.OnScoreChange, playerScores[playerType]);
                break;
            case EPlayerType.Bot1:
                Main.Observer.Notify(EEvent.OnBot1ScoreChange, playerScores[playerType]);
                break;
            case EPlayerType.Bot2:
                Main.Observer.Notify(EEvent.OnBot2ScoreChange, playerScores[playerType]);
                break;
        }

        // Notify total score change
        Main.Observer.Notify(EEvent.OnTotalScoreChange, TotalScore);
    }
}
