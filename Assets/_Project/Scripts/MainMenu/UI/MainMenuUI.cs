using OSK;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : View
{
    [SerializeField] private Button btnStart;

    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);
        btnStart.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        SingletonManager.Instance.Get<Menu_Manager>().StartGame();
    }
}
