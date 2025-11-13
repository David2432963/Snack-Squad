using OSK;
using UnityEngine;

public class ScoreRanking : MonoBehaviour
{
    [SerializeField] private PlayerScore[] playerScores;
    [SerializeField] private Color[] textColors;

    private void Awake()
    {
        Main.Observer.Add(EEvent.OnTotalScoreChange, OrderPlayersScore);
    }
    private void Start()
    {
        (EPlayerType playerType, int score)[] scores = new (EPlayerType, int)[]
        {
                (EPlayerType.Player, 0),
                (EPlayerType.Edward, 0),
                (EPlayerType.Bruce, 0)
        };
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i].SetPlayer(scores[i].playerType, i, scores[i].score, textColors[i]);
        }
    }

    private void OrderPlayersScore(object data)
    {
        int playerScore = GameData_Manager.Instance.GetPlayerScore(EPlayerType.Player);
        int bot1Score = GameData_Manager.Instance.GetPlayerScore(EPlayerType.Edward);
        int bot2Score = GameData_Manager.Instance.GetPlayerScore(EPlayerType.Bruce);

        // Create an array of player types and their scores
        (EPlayerType playerType, int score)[] scores = new (EPlayerType, int)[]
        {
            (EPlayerType.Player, playerScore),
            (EPlayerType.Edward, bot1Score),
            (EPlayerType.Bruce, bot2Score)
        };

        // Sort the array based on scores in descending order
        System.Array.Sort(scores, (a, b) => b.score.CompareTo(a.score));

        // Update the UI elements based on sorted scores
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i].SetPlayer(scores[i].playerType, i, scores[i].score, textColors[i]);
        }
    }

    private void OnDestroy()
    {
        Main.Observer.Remove(EEvent.OnTotalScoreChange, OrderPlayersScore);
    }
}
