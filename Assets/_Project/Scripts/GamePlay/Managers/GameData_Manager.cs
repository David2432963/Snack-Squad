using OSK;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class GameData_Manager : MonoBehaviour
{
    [Title("Session Configuration")]
    [SerializeField, ReadOnly] private EFoodType currentSessionFoodType;
    
    [Title("Player Scores")]
    [SerializeField, ReadOnly] private Dictionary<EPlayerType, int> playerScores = new Dictionary<EPlayerType, int>();
    
    // Session food type access
    public EFoodType CurrentSessionFoodType => currentSessionFoodType;
    
    // Main player score for backwards compatibility
    public int CurrentScore => playerScores.ContainsKey(EPlayerType.MainPlayer) ? playerScores[EPlayerType.MainPlayer] : 0;
    
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
        // Initialize session food type randomly
        InitializeSessionFoodType();
        
        // Initialize all player scores to 0
        InitializePlayerScores();
    }

    private void InitializeSessionFoodType()
    {
        // Randomly select a food type for this gameplay session
        System.Array foodTypes = System.Enum.GetValues(typeof(EFoodType));
        currentSessionFoodType = (EFoodType)foodTypes.GetValue(Random.Range(0, foodTypes.Length));
        
        HPDebug.Log($"[GameData_Manager] Session food type selected: {currentSessionFoodType}");
        
        // Notify other systems about the selected food type
        Main.Observer.Notify("OnSessionFoodTypeSelected", currentSessionFoodType);
    }

    private void InitializePlayerScores()
    {
        playerScores.Clear();
        playerScores[EPlayerType.MainPlayer] = 0;
        playerScores[EPlayerType.Bot1] = 0;
        playerScores[EPlayerType.Bot2] = 0;
    }

    // Backwards compatibility method for main player
    public void AddScore(int amount)
    {
        AddScore(EPlayerType.MainPlayer, amount);
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
            case EPlayerType.MainPlayer:
                Main.Observer.Notify(EEvent.OnScoreChange, playerScores[playerType]);
                break;
            case EPlayerType.Bot1:
                Main.Observer.Notify("OnBot1ScoreChange", playerScores[playerType]);
                break;
            case EPlayerType.Bot2:
                Main.Observer.Notify("OnBot2ScoreChange", playerScores[playerType]);
                break;
        }
        
        // Notify total score change
        Main.Observer.Notify("OnTotalScoreChange", TotalScore);
        
        HPDebug.Log($"[GameData_Manager] {playerType} scored {amount} points. Total: {playerScores[playerType]}");
    }

    [Button("Reset All Scores")]
    public void ResetAllScores()
    {
        InitializePlayerScores();
        
        // Notify all score changes
        Main.Observer.Notify(EEvent.OnScoreChange, 0);
        Main.Observer.Notify("OnBot1ScoreChange", 0);
        Main.Observer.Notify("OnBot2ScoreChange", 0);
        Main.Observer.Notify("OnTotalScoreChange", 0);
    }

    [Button("Change Session Food Type")]
    private void DebugChangeSessionFoodType()
    {
        InitializeSessionFoodType();
    }

    [Button("Print All Scores")]
    private void DebugPrintScores()
    {
        HPDebug.Log("=== PLAYER SCORES ===");
        foreach (var kvp in playerScores)
        {
            HPDebug.Log($"{kvp.Key}: {kvp.Value} points");
        }
        HPDebug.Log($"Total Score: {TotalScore}");
    }
}
