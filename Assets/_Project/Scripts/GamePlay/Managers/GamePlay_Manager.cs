using OSK;
using UnityEngine;
using System;

[AutoRegisterUpdate]
public class GamePlay_Manager : MonoBehaviour, IUpdate, IFixedUpdate, ILateUpdate
{
    [Header("Game Timer Settings")]
    [SerializeField] private float gameTimeLimit = 60f; // 60 seconds
    [SerializeField] private bool isGameActive = false;

    private float currentGameTime;
    private float remainingTime;

    // Events
    public event Action<float> OnTimeUpdated;
    public event Action OnTimeExpired;

    // Properties
    public static GamePlay_Manager Instance => SingletonManager.Instance.Get<GamePlay_Manager>();
    public float RemainingTime => remainingTime;
    public float GameTimeLimit => gameTimeLimit;
    public bool IsGameActive => isGameActive;
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

    public void FixedTick(float fixedDeltaTime)
    {
    }

    public void LateTick(float deltaTime)
    {

    }

    public void Tick(float deltaTime)
    {
        if (isGameActive)
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
            TimeUp();
        }
    }

    private void TimeUp()
    {
        isGameActive = false;
        OnTimeExpired?.Invoke();
        Main.Observer.Notify(EEvent.OnTimeExpired);
        GameOver();
    }

    public void StartGame()
    {
        // Reset timer
        currentGameTime = 0f;
        remainingTime = gameTimeLimit;
        isGameActive = true;

        // Unpause the game
        Main.Mono.SetPause(false);

        // Open gameplay UI
        Main.UI.Open<GamePlayUI>();

        // Notify that game started
        Main.Observer.Notify(EEvent.OnTimeUpdated, remainingTime);
    }
    public void GameOver()
    {
        // Stop the game
        isGameActive = false;

        // Pause the game
        Main.Mono.SetPause(true);

        // Notify observers
        Main.Observer.Notify(EEvent.OnGameOver);

        // Show game over UI
        Main.UI.Open<GameOverUI>();
    }

    public void RestartGame()
    {
        Main.Director.LoadScene(new DataScene() { sceneName = "Gameplay", loadMode = ELoadMode.Single })
        .OnStart(() =>
        {
            Main.UI.HideAll();
            Main.Mono.SetPause(false);
        })
        .Build();
    }

    #region Public Methods

    public void PauseGame()
    {
        isGameActive = false;
        Main.Mono.SetPause(true);
    }

    public void ResumeGame()
    {
        isGameActive = true;
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
        })
        .Build();
    }

    #endregion
}
