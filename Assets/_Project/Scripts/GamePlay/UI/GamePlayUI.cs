using OSK;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUI : View
{
    [SerializeField] private Text textScore;
    [SerializeField] private Text textGold;
    [SerializeField] private Joystick joystick;
    [SerializeField] private QuestUI questUI;

    public Joystick Joystick => joystick;

    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);
        questUI.Initialize();
    }

    public override void Open(object[] data = null)
    {
        base.Open(data);
        Main.Observer.Add(EEvent.OnScoreChange, UpdateScoreText);
    }

    public override void Hide()
    {
        base.Hide();
    }

    public void UpdateScoreText(object score)
    {
        textScore.text = score.ToString();
    }

    public void UpdateGoldText(object gold)
    {
        textGold.text = gold.ToString();
    }
}
