using OSK;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : View
{
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnQuest;
    [SerializeField] private Button btnDailyQuest;

    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);
        btnStart.onClick.AddListener(StartGame);
        btnQuest.onClick.AddListener(OpenQuestUI);
        btnDailyQuest.onClick.AddListener(OpenDailyQuestUI);
    }

    public void StartGame()
    {
        SingletonManager.Instance.Get<Menu_Manager>().StartGame();
    }

    public void OpenQuestUI()
    {
        Main.UI.Open<DailyQuestUI>();
    }

    public void OpenDailyQuestUI()
    {
        Main.UI.Open<DailyQuestUI>();
    }
}
