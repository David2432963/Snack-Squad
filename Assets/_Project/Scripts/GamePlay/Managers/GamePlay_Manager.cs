using OSK;
using UnityEngine;

[AutoRegisterUpdate]
public class GamePlay_Manager : MonoBehaviour, IUpdate, IFixedUpdate, ILateUpdate
{
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

    }

    public void StartGame()
    {
        Main.UI.Open<GamePlayUI>();
    }
    public void GameOver()
    {
        Main.Mono.SetPause(true);
        Main.Observer.Notify("OnGameOver");
        Main.UI.Open<GameOverUI>();
    }

    public void RestartGame()
    {
        Main.Director.LoadScene(new DataScene() { sceneName = "GamePlay", loadMode = ELoadMode.Single })
        .OnStart(() =>
        {
            Main.UI.HideAll();
            Main.Mono.SetPause(false);
        })
        .Build();
    }
}
