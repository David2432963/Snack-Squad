using System;
using UnityEngine;

[System.Serializable]
public class DailyQuestProgress
{
    [SerializeField] private DailyQuestSO quest;
    [SerializeField] private int currentProgress;
    [SerializeField] private bool isCompleted;
    [SerializeField] private bool isRewardClaimed;
    [SerializeField] private DateTime assignedDate;
    [SerializeField] private DateTime completionTime;

    // Events
    public event Action<DailyQuestProgress> OnProgressUpdated;
    public event Action<DailyQuestProgress> OnQuestCompleted;
    public event Action<DailyQuestProgress> OnRewardClaimed;

    // Properties
    public DailyQuestSO Quest => quest;
    public int CurrentProgress => currentProgress;
    public int TargetProgress => quest.TargetValue;
    public bool IsCompleted => isCompleted;
    public bool IsRewardClaimed => isRewardClaimed;
    public float Progress => (float)currentProgress / TargetProgress;
    public DateTime AssignedDate => assignedDate;
    public DateTime CompletionTime => completionTime;
    public bool IsExpired => DateTime.Now.Date > assignedDate.Date;

    // Constructor
    public DailyQuestProgress(DailyQuestSO quest)
    {
        this.quest = quest;
        this.currentProgress = 0;
        this.isCompleted = false;
        this.isRewardClaimed = false;
        this.assignedDate = DateTime.Now;
    }

    // Constructor for loading from save data
    public DailyQuestProgress(DailyQuestSO quest, int progress, bool completed, bool rewardClaimed, DateTime assignedDate)
    {
        this.quest = quest;
        this.currentProgress = progress;
        this.isCompleted = completed;
        this.isRewardClaimed = rewardClaimed;
        this.assignedDate = assignedDate;
    }

    public void AddProgress(int amount = 1)
    {
        if (isCompleted || IsExpired) return;

        currentProgress += amount;
        currentProgress = Mathf.Clamp(currentProgress, 0, TargetProgress);

        OnProgressUpdated?.Invoke(this);

        if (currentProgress >= TargetProgress && !isCompleted)
        {
            CompleteQuest();
        }
    }

    public void SetProgress(int amount)
    {
        if (isCompleted || IsExpired) return;

        currentProgress = Mathf.Clamp(amount, 0, TargetProgress);
        OnProgressUpdated?.Invoke(this);

        if (currentProgress >= TargetProgress && !isCompleted)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        isCompleted = true;
        completionTime = DateTime.Now;
        OnQuestCompleted?.Invoke(this);
    }

    public void ClaimReward()
    {
        if (!isCompleted || isRewardClaimed) return;

        isRewardClaimed = true;

        // Give rewards based on reward type
        // Daily quests now only give gold rewards
        if (quest.GoldReward > 0)
        {
            GameData.Gold += quest.GoldReward;
        }

        OnRewardClaimed?.Invoke(this);
    }

    public string GetProgressText()
    {
        return $"{currentProgress}/{TargetProgress}";
    }

    public float GetProgressPercentage()
    {
        return Progress * 100f;
    }

    public string GetTimeRemainingText()
    {
        if (IsExpired)
            return "Expired";

        DateTime nextDay = assignedDate.Date.AddDays(1);
        TimeSpan remaining = nextDay - DateTime.Now;

        if (remaining.TotalHours < 1)
        {
            return $"{remaining.Minutes}m left";
        }
        else
        {
            return $"{remaining.Hours}h {remaining.Minutes}m left";
        }
    }

    public void ResetProgress()
    {
        currentProgress = 0;
        isCompleted = false;
        isRewardClaimed = false;
        assignedDate = DateTime.Now;

        OnProgressUpdated?.Invoke(this);
    }
}