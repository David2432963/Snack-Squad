using OSK;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameOverUI : View
{
    [Header("Score Display")]
    [SerializeField] private Text[] textScore;
    [SerializeField] private Text[] textName;


    [Header("Reward Display")]
    [SerializeField] private Text txtGoldReward;

    [Header("Controls")]
    [SerializeField] private Button btnReplay;
    [SerializeField] private Button btnClaim;
    [SerializeField] private Button btnHome;

    [Header("Reward Settings")]
    [SerializeField] private int baseGoldReward = 10;
    [SerializeField] private int firstPlaceBonus = 50;
    [SerializeField] private int secondPlaceBonus = 20;
    [SerializeField] private int thirdPlaceBonus = 5;

    private int totalGoldEarned = 0;

    private void Start()
    {
        btnReplay.onClick.AddListener(Replay);
        btnClaim.onClick.AddListener(ClaimRewards);
        btnHome.onClick.AddListener(BackToMenu);
    }
    public override void Open(object[] data = null)
    {
        base.Open(data);
        DisplayScoresAndRanking();
        CalculateAndShowRewards();

        btnHome.gameObject.SetActive(false);
        btnClaim.gameObject.SetActive(true);
    }

    private void DisplayScoresAndRanking()
    {
        var gameDataManager = GameData_Manager.Instance;

        // Get all scores
        int playerScore = gameDataManager.GetPlayerScore(EPlayerType.Player);
        int bot1Score = gameDataManager.GetPlayerScore(EPlayerType.Bot1);
        int bot2Score = gameDataManager.GetPlayerScore(EPlayerType.Bot2);

        // Create player data list for ranking
        var playerData = new List<(string name, int score, EPlayerType type)>
        {
            ("Player", playerScore, EPlayerType.Player),
            ("Bot 1", bot1Score, EPlayerType.Bot1),
            ("Bot 2", bot2Score, EPlayerType.Bot2)
        };

        // Sort by score (descending)
        var rankedPlayers = playerData.OrderByDescending(p => p.score).ToList();

        // Display ranked results in the arrays
        for (int i = 0; i < rankedPlayers.Count && i < textScore.Length && i < textName.Length; i++)
        {
            if (textName[i] != null)
                textName[i].text = rankedPlayers[i].name;

            if (textScore[i] != null)
                textScore[i].text = rankedPlayers[i].score.ToString();
        }

        // Find player's rank for reward calculation
        int playerRank = 1;
        for (int i = 0; i < rankedPlayers.Count; i++)
        {
            if (rankedPlayers[i].type == EPlayerType.Player)
            {
                playerRank = i + 1;
                break;
            }
        }
    }

    private int GetPlayerRanking(int playerScore, int bot1Score, int bot2Score)
    {
        var scores = new List<int> { playerScore, bot1Score, bot2Score };
        var sortedScores = scores.OrderByDescending(x => x).ToList();

        int rank = 1;
        for (int i = 0; i < sortedScores.Count; i++)
        {
            if (sortedScores[i] == playerScore)
            {
                // Handle ties - if multiple players have same score, give best rank
                rank = i + 1;
                break;
            }
        }

        return rank;
    }

    private void CalculateAndShowRewards()
    {
        var gameDataManager = GameData_Manager.Instance;
        int playerScore = gameDataManager.GetPlayerScore(EPlayerType.Player);
        int bot1Score = gameDataManager.GetPlayerScore(EPlayerType.Bot1);
        int bot2Score = gameDataManager.GetPlayerScore(EPlayerType.Bot2);

        int playerRank = GetPlayerRanking(playerScore, bot1Score, bot2Score);

        // Base reward
        totalGoldEarned = baseGoldReward;

        // Rank bonus
        int rankBonus = GetRankBonus(playerRank);
        totalGoldEarned += rankBonus;

        // Score bonus (1 gold per 10 points)
        int scoreBonus = playerScore / 10;
        totalGoldEarned += scoreBonus;

        // Display rewards
        if (txtGoldReward != null)
            txtGoldReward.text = $"+{totalGoldEarned}";
    }
    private int GetRankBonus(int rank)
    {
        return rank switch
        {
            1 => firstPlaceBonus,
            2 => secondPlaceBonus,
            3 => thirdPlaceBonus,
            _ => 0
        };
    }
    private void ClaimRewards()
    {
        GameData.Gold += totalGoldEarned;
        btnClaim.gameObject.SetActive(false);
        btnHome.gameObject.SetActive(true);
    }
    private void BackToMenu()
    {
        SingletonManager.Instance.Get<GamePlay_Manager>().ReturnToMainMenu();
    }
    private void Replay()
    {
        SingletonManager.Instance.Get<GamePlay_Manager>().RestartGame();
    }
}
