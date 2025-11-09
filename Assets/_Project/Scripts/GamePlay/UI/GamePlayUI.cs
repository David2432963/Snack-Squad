using OSK;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUI : View
{
    [SerializeField] private Text textScore;
    [SerializeField] private Text textTimer;
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
        Main.Observer.Add(EEvent.OnTimeUpdated, UpdateTimerText);

        // Initialize timer display
        if (GamePlay_Manager.Instance != null)
        {
            UpdateTimerText(GamePlay_Manager.Instance.RemainingTime);
        }
        textScore.text = "0";
    }

    public override void Hide()
    {
        base.Hide();
        Main.Observer.Remove(EEvent.OnScoreChange, UpdateScoreText);
        Main.Observer.Remove(EEvent.OnTimeUpdated, UpdateTimerText);
    }

    public void UpdateScoreText(object score)
    {
        textScore.text = score.ToString();
    }

    public void UpdateTimerText(object remainingTime)
    {
        if (textTimer != null && remainingTime is float timeValue)
        {
            int minutes = Mathf.FloorToInt(timeValue / 60f);
            int seconds = Mathf.FloorToInt(timeValue % 60f);
            textTimer.text = $"{minutes:00}:{seconds:00}";

            // Change color to red when time is running low (last 10 seconds)
            if (timeValue <= 10f)
            {
                textTimer.color = Color.red;
            }
            else if (timeValue <= 20f)
            {
                textTimer.color = Color.yellow;
            }
            else
            {
                textTimer.color = Color.white;
            }
        }
    }
}
