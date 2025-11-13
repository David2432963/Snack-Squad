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
        this.isCompleted = false;
        this.isRewardClaimed = false;
        this.assignedDate = DateTime.Now;
        
        // Initialize progress based on quest type
        if (quest.QuestType == EDailyQuestType.CompleteNormalQuests)
        {
            // For CompleteNormalQuests, load progress from GameData
            this.currentProgress = GameData.GetNormalQuestsCompletedToday();
        }
        else if (quest.QuestType == EDailyQuestType.CollectSpecificFood)
        {
            // For CollectSpecificFood, load progress from GameData
            this.currentProgress = GetSpecificFoodCollectedCount(quest);
        }
        else
        {
            this.currentProgress = 0;
        }
        
        // Check if already completed
        if (currentProgress >= TargetProgress)
        {
            CompleteQuest();
        }
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

    /// <summary>
    /// Refreshes progress for CompleteNormalQuests type from GameData
    /// </summary>
    public void RefreshProgressFromGameData()
    {
        if (quest.QuestType == EDailyQuestType.CompleteNormalQuests)
        {
            int currentFromGameData = GameData.GetNormalQuestsCompletedToday();
            if (currentFromGameData != currentProgress)
            {
                SetProgress(currentFromGameData);
            }
        }
        else if (quest.QuestType == EDailyQuestType.CollectSpecificFood)
        {
            int currentFromGameData = GetSpecificFoodCollectedCount(quest);
            if (currentFromGameData != currentProgress)
            {
                SetProgress(currentFromGameData);
            }
        }
    }
    
    /// <summary>
    /// Gets the current count of collected specific food for this quest from GameData
    /// </summary>
    private int GetSpecificFoodCollectedCount(DailyQuestSO quest)
    {
        if (quest.QuestType != EDailyQuestType.CollectSpecificFood)
            return 0;
            
        object specificType = null;
        
        switch (quest.RequiredFoodType)
        {
            case EFoodType.Fruit:
                specificType = quest.RequiredFruitType;
                break;
            case EFoodType.FastFood:
                specificType = quest.RequiredFastFoodType;
                break;
            case EFoodType.Cake:
                specificType = quest.RequiredCakeType;
                break;
        }
        
        return GameData.GetDailyFoodCollected(quest.RequiredFoodType, specificType);
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