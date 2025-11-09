using System;
using UnityEngine;

[System.Serializable]
public class AchievementProgress
{
    [SerializeField] private string achievementId;
    [SerializeField] private int currentProgress;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private bool isRewardClaimed;
    [SerializeField] private DateTime unlockedDate;

    // Events
    public event Action<AchievementProgress> OnProgressUpdated;
    public event Action<AchievementProgress> OnAchievementUnlocked;
    public event Action<AchievementProgress> OnRewardClaimed;

    // Properties
    public string AchievementId => achievementId;
    public int CurrentProgress => currentProgress;
    public bool IsUnlocked => isUnlocked;
    public bool IsRewardClaimed => isRewardClaimed;
    public DateTime UnlockedDate => unlockedDate;

    // Reference to the achievement SO (not serialized, loaded at runtime)
    public AchievementSO Achievement { get; private set; }
    
    // Progress calculation
    public float ProgressPercentage => Achievement != null ? 
        Mathf.Clamp01((float)currentProgress / Achievement.TargetValue) : 0f;

    // Constructor for new achievement
    public AchievementProgress(string achievementId)
    {
        this.achievementId = achievementId;
        this.currentProgress = 0;
        this.isUnlocked = false;
        this.isRewardClaimed = false;
        this.unlockedDate = DateTime.MinValue;
    }

    // Constructor for loading from save data
    public AchievementProgress(string achievementId, int progress, bool unlocked, bool rewardClaimed, DateTime unlockedDate)
    {
        this.achievementId = achievementId;
        this.currentProgress = progress;
        this.isUnlocked = unlocked;
        this.isRewardClaimed = rewardClaimed;
        this.unlockedDate = unlockedDate;
    }

    public void SetAchievementSO(AchievementSO achievementSO)
    {
        Achievement = achievementSO;
    }

    public void AddProgress(int amount = 1)
    {
        if (isUnlocked || Achievement == null) return;

        currentProgress += amount;
        currentProgress = Mathf.Clamp(currentProgress, 0, Achievement.TargetValue);

        OnProgressUpdated?.Invoke(this);

        // Check if achievement is now unlocked
        if (currentProgress >= Achievement.TargetValue && !isUnlocked)
        {
            UnlockAchievement();
        }
    }

    public void SetProgress(int amount)
    {
        if (isUnlocked || Achievement == null) return;

        currentProgress = Mathf.Clamp(amount, 0, Achievement.TargetValue);
        OnProgressUpdated?.Invoke(this);

        // Check if achievement is now unlocked
        if (currentProgress >= Achievement.TargetValue && !isUnlocked)
        {
            UnlockAchievement();
        }
    }

    private void UnlockAchievement()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        unlockedDate = DateTime.Now;
        
        OnAchievementUnlocked?.Invoke(this);
    }

    public void ClaimReward()
    {
        if (!isUnlocked || isRewardClaimed || Achievement == null) return;

        isRewardClaimed = true;

        // Award gold
        if (Achievement.GoldReward > 0)
        {
            GameData.Gold += Achievement.GoldReward;
        }

        OnRewardClaimed?.Invoke(this);
    }

    public string GetProgressText()
    {
        if (Achievement == null) return "0/0";
        return $"{currentProgress}/{Achievement.TargetValue}";
    }

    public string GetStatusText()
    {
        if (isUnlocked)
        {
            return isRewardClaimed ? "Claimed" : "Completed";
        }
        return "In Progress";
    }

    // For debugging
    public override string ToString()
    {
        return $"{achievementId}: {currentProgress}/{(Achievement?.TargetValue ?? 0)} - {GetStatusText()}";
    }
}

// Save data structure for achievements
[System.Serializable]
public class AchievementSaveEntry
{
    public string achievementId;
    public int currentProgress;
    public bool isUnlocked;
    public bool isRewardClaimed;
    public string unlockedDate; // DateTime as string for serialization

    public AchievementSaveEntry(string id, int progress, bool unlocked, bool claimed, DateTime unlockDate)
    {
        achievementId = id;
        currentProgress = progress;
        isUnlocked = unlocked;
        isRewardClaimed = claimed;
        unlockedDate = unlockDate.ToBinary().ToString();
    }

    public DateTime GetUnlockedDate()
    {
        if (long.TryParse(unlockedDate, out long binary))
        {
            return DateTime.FromBinary(binary);
        }
        return DateTime.MinValue;
    }
}