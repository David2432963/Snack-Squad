using DG.Tweening;
using OSK;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUI : View
{
    [SerializeField] private Text textScore;
    [SerializeField] private Text textTimer;
    [SerializeField] private Joystick joystick;
    [SerializeField] private QuestUI questUI;
    [SerializeField] private Text textTimeStart;
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
        Main.Observer.Add(EEvent.OnGameStateChange, UpdateStartText);
        Main.Observer.Add(EEvent.OnGameOver, OnEndGame);


        // Initialize timer display
        if (GamePlay_Manager.Instance != null)
        {
            UpdateTimerText(GamePlay_Manager.Instance.RemainingTime);
        }
        textScore.text = "0";
        joystick.gameObject.SetActive(false);
        textTimeStart.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        base.Hide();
        Main.Observer.Remove(EEvent.OnScoreChange, UpdateScoreText);
        Main.Observer.Remove(EEvent.OnTimeUpdated, UpdateTimerText);
        Main.Observer.Remove(EEvent.OnGameStateChange, UpdateStartText);
        Main.Observer.Remove(EEvent.OnGameOver, OnEndGame);
    }

    public void UpdateScoreText(object score)
    {
        textScore.text = score.ToString();
        textScore.transform.DOScale(1.2f, 0.05f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            textScore.transform.DOScale(1f, 0.05f).SetEase(Ease.InBack);
        });
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
    public void UpdateTimeStart(int time)
    {
        if (textTimeStart != null)
        {
            textTimeStart.text = time > 0 ? time.ToString() : "Go!";
        }
    }

    private void UpdateStartText(object data)
    {
        if (data is EGameState state)
        {
            if (state == EGameState.Ready)
            {
                textTimeStart.gameObject.SetActive(true);
                joystick.gameObject.SetActive(false);
            }
            else
            {
                textTimeStart.gameObject.SetActive(false);
                joystick.gameObject.SetActive(true);
            }
        }
    }
    private void OnEndGame(object data)
    {
        textTimer.text = "00:00";
    }
}
