using OSK;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : View
{
    [SerializeField] private Text txtScore;
    [SerializeField] private Button btnReplay;

    private void Start()
    {
        btnReplay.onClick.AddListener(Replay);
    }
    public override void Open(object[] data = null)
    {
        base.Open(data);
        txtScore.text = SingletonManager.Instance.Get<GameData_Manager>().CurrentScore.ToString();
    }
    private void Replay()
    {
        SingletonManager.Instance.Get<GamePlay_Manager>().RestartGame();
    }
}
