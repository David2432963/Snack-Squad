using OSK;
using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

[AutoRegisterUpdate]
public class GamePlay_Manager : MonoBehaviour, IUpdate
{
    [Header("Game Timer Settings")]
    [SerializeField] private float gameTimeLimit = 60f; // 60 seconds
    [SerializeField] private int countdownTime = 3;

    private EGameState currentGameState;
    private float currentGameTime;
    private float remainingTime;

    // Events
    public event Action<float> OnTimeUpdated;

    // Properties
    public static GamePlay_Manager Instance => SingletonManager.Instance.Get<GamePlay_Manager>();
    public float RemainingTime => remainingTime;
    public float GameTimeLimit => gameTimeLimit;
    private void Awake()
    {
        Main.Mono.Register(this);
        SingletonManager.Instance.RegisterScene(this);
    }
    private void Start()
    {
        StartGame();
    }
    void OnDestroy()
    {
        Main.Mono.UnRegister(this);
    }

    public void Tick(float deltaTime)
    {
        if (currentGameState == EGameState.Playing)
        {
            UpdateGameTimer(deltaTime);
        }
    }

    private void UpdateGameTimer(float deltaTime)
    {
        currentGameTime += deltaTime;
        remainingTime = gameTimeLimit - currentGameTime;

        // Notify UI about time update
        OnTimeUpdated?.Invoke(remainingTime);
        Main.Observer.Notify(EEvent.OnTimeUpdated, remainingTime);

        // Check if time is up
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            GameOver();
        }
    }

    public void StartGame()
    {
        SwitchGameState(EGameState.Ready);
        // Reset timer
        currentGameTime = 0f;
        remainingTime = gameTimeLimit;

        // Unpause the game
        Main.Mono.SetPause(false);

        // Open gameplay UI
        Main.UI.Open<GamePlayUI>();

        // Notify that game started
        Main.Observer.Notify(EEvent.OnTimeUpdated, remainingTime);
        StartCoroutine(CountDownStart());
    }
    private IEnumerator CountDownStart()
    {
        int currentTime = countdownTime;
        var gameplayUI = Main.UI.Get<GamePlayUI>();
        gameplayUI.UpdateTimeStart(currentTime);

        while (currentTime >= 1)
        {
            Main.Sound.Play(SoundID.Count.ToString());
            yield return new WaitForSeconds(1f);
            currentTime -= 1;
            gameplayUI.UpdateTimeStart(currentTime);
        }
        Main.Sound.Play(SoundID.Go.ToString());
        yield return new WaitForSeconds(1f);
        SwitchGameState(EGameState.Playing);
    }
    private void SwitchGameState(EGameState newState)
    {
        currentGameState = newState;
        switch (newState)
        {
            case EGameState.Ready:
                Main.Sound.Stop(SoundType.MUSIC);
                break;
            case EGameState.Playing:
                Main.Sound.Play(SoundID.IngameMusic.ToString(), loop: true);
                break;
            case EGameState.Pause:
                Main.Sound.Pause(SoundType.MUSIC);
                break;
            case EGameState.End:
                Main.Sound.Stop(SoundType.MUSIC);
                break;
        }
        Main.Observer.Notify(EEvent.OnGameStateChange, newState);
    }
    private void GameOver()
    {
        SwitchGameState(EGameState.End);
        Main.Sound.Play(SoundID.Win.ToString());

        // Notify observers
        CheckWinner();
        Main.Observer.Notify(EEvent.OnGameOver);

        // Show game over UI
        DOVirtual.DelayedCall(4f, () =>
        {
            Main.UI.Open<GameOverUI>();
        });
    }

    public void RestartGame()
    {
        Main.Director.LoadScene(new DataScene() { sceneName = "Gameplay", loadMode = ELoadMode.Single })
        .OnStart(() =>
        {
            Main.UI.HideAll();
            Main.Mono.SetPause(false);
            Main.Sound.DestroyAll();
            Main.Pool.DestroyAllGroups();
        })
        .Build();
    }
    private EPlayerType winner;
    public EPlayerType Winner => winner;
    private void CheckWinner()
    {
        var gameDataManager = GameData_Manager.Instance;
        if (gameDataManager == null) return;

        int playerScore = gameDataManager.GetPlayerScore(EPlayerType.Player);
        int bot1Score = gameDataManager.GetPlayerScore(EPlayerType.Edward);
        int bot2Score = gameDataManager.GetPlayerScore(EPlayerType.Bruce);

        int highestScore = playerScore;

        // Check if Bot1 (Edward) has higher score
        if (bot1Score > highestScore)
        {
            winner = EPlayerType.Edward;
            highestScore = bot1Score;
        }

        // Check if Bot2 (Bruce) has higher score
        if (bot2Score > highestScore)
        {
            winner = EPlayerType.Bruce;
            highestScore = bot2Score;
        }
    }

    #region Public Methods

    public void PauseGame()
    {
        SwitchGameState(EGameState.Pause);
        Main.Mono.SetPause(true);
    }

    public void ResumeGame()
    {
        SwitchGameState(EGameState.Playing);
        Main.Mono.SetPause(false);
    }

    public void AddTime(float additionalTime)
    {
        remainingTime += additionalTime;
        if (remainingTime > gameTimeLimit)
        {
            remainingTime = gameTimeLimit;
            currentGameTime = 0f;
        }
        else
        {
            currentGameTime = gameTimeLimit - remainingTime;
        }
        OnTimeUpdated?.Invoke(remainingTime);
        Main.Observer.Notify(EEvent.OnTimeUpdated, remainingTime);
    }

    public string GetFormattedTimeRemaining()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void ReturnToMainMenu()
    {
        Main.Director.LoadScene(new DataScene() { sceneName = "MainMenu", loadMode = ELoadMode.Single })
        .OnStart(() =>
        {
            Main.UI.HideAll();
            Main.Mono.SetPause(false);
            Main.Sound.DestroyAll();
            Main.Pool.DestroyAllGroups();
        })
        .Build();
    }

    #endregion
}
