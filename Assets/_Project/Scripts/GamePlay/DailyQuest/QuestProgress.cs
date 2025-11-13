using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class QuestProgress
{
    [SerializeField] private FoodCollectionQuestSO quest;
    [SerializeField] private int currentAmount;
    [SerializeField] private bool isCompleted;
    [SerializeField] private bool isRewardClaimed;
    [SerializeField] private DateTime startTime;
    [SerializeField] private DateTime completionTime;

    // For multi-item collection tracking
    [SerializeField] private List<int> collectedSpecificItems = new List<int>();

    // Events
    public event Action<QuestProgress> OnProgressUpdated;
    public event Action<QuestProgress> OnQuestCompleted;
    public event Action<QuestProgress> OnRewardClaimed;

    // Properties
    public FoodCollectionQuestSO Quest => quest;
    public int CurrentAmount => currentAmount;
    public int TargetAmount => quest.TargetAmount;
    public bool IsCompleted => isCompleted;
    public bool IsRewardClaimed => isRewardClaimed;
    public float Progress => (float)currentAmount / TargetAmount;
    public DateTime StartTime => startTime;
    public DateTime CompletionTime => completionTime;
    public IReadOnlyList<int> CollectedSpecificItems => collectedSpecificItems.AsReadOnly();

    // Constructor
    public QuestProgress(FoodCollectionQuestSO quest)
    {
        this.quest = quest;
        this.currentAmount = 0;
        this.isCompleted = false;
        this.isRewardClaimed = false;
        this.startTime = DateTime.Now;
        this.collectedSpecificItems = new List<int>();
    }

    // Methods
    public void AddProgress(int amount = 1)
    {
        if (isCompleted) return;

        // Since all quests are now specific item collection, this method is mainly for debugging
        currentAmount += amount;
        currentAmount = Mathf.Clamp(currentAmount, 0, TargetAmount);

        OnProgressUpdated?.Invoke(this);

        if (currentAmount >= TargetAmount && !isCompleted)
        {
            CompleteQuest();
        }
    }

    // Main method for specific item collection
    public bool AddSpecificItem(int itemValue)
    {
        if (isCompleted) return false;

        // Check if this item is required for the quest
        if (!quest.IsSpecificItemRequired(itemValue))
        {
            ResetProgress();
            return false; // Item not needed for this quest
        }

        // Check if we already collected this specific item
        if (collectedSpecificItems.Contains(itemValue))
        {
            return false; // Already collected this item
        }

        // Add the specific item to our collection
        collectedSpecificItems.Add(itemValue);
        currentAmount = collectedSpecificItems.Count;

        OnProgressUpdated?.Invoke(this);

        Debug.Log($"Collected specific item for quest '{quest.QuestName}': {GetItemName(itemValue)} ({currentAmount}/{TargetAmount})");

        if (currentAmount >= TargetAmount && !isCompleted)
        {
            CompleteQuest();
        }

        return true;
    }

    private string GetItemName(int itemValue)
    {
        switch (quest.RequiredFoodType)
        {
            case EFoodType.Fruit:
                return ((EFruitType)itemValue).ToString();
            case EFoodType.FastFood:
                return ((EFastFoodType)itemValue).ToString();
            case EFoodType.Cake:
                return ((ECakeType)itemValue).ToString();
            default:
                return "Unknown Item";
        }
    }

    public void SetProgress(int amount)
    {
        if (isCompleted) return;

        currentAmount = Mathf.Clamp(amount, 0, TargetAmount);
        OnProgressUpdated?.Invoke(this);

        if (currentAmount >= TargetAmount && !isCompleted)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        isCompleted = true;
        completionTime = DateTime.Now;
        OnQuestCompleted?.Invoke(this);

        if (quest.UseMultiItemCollection)
        {
            var collectedNames = collectedSpecificItems.Select(GetItemName).ToArray();
            Debug.Log($"Quest '{quest.QuestName}' completed! Collected specific items: {string.Join(", ", collectedNames)}");
        }
        else
        {
            Debug.Log($"Quest '{quest.QuestName}' completed! Collected {currentAmount}/{TargetAmount} {quest.RequiredFoodType}");
        }
    }

    public void ClaimReward()
    {
        if (!isCompleted || isRewardClaimed) return;

        isRewardClaimed = true;

        // Give rewards to player
        if (quest.ScoreReward > 0)
        {
            GameData_Manager.Instance.AddScore(quest.ScoreReward);
        }

        if (quest.BonusScore > 0)
        {
            GameData_Manager.Instance.AddScore(quest.BonusScore);
        }

        OnRewardClaimed?.Invoke(this);

        Debug.Log($"Quest reward claimed! Received {quest.ScoreReward + quest.BonusScore} points total.");
    }

    public void ResetProgress()
    {
        currentAmount = 0;
        isCompleted = false;
        isRewardClaimed = false;
        startTime = DateTime.Now;
        completionTime = default;
        collectedSpecificItems.Clear();

        OnProgressUpdated?.Invoke(this);
    }

    // Get completion percentage as string
    public string GetProgressText()
    {
        if (quest.UseMultiItemCollection)
        {
            return $"{collectedSpecificItems.Count}/{quest.SelectedSpecificItems.Count} specific items";
        }
        return $"{currentAmount}/{TargetAmount}";
    }

    // Get detailed progress text for multi-item quests
    public string GetDetailedProgressText()
    {
        if (!quest.UseMultiItemCollection)
        {
            return GetProgressText();
        }

        var requiredItems = quest.GetRequiredItemNames();
        var collectedNames = collectedSpecificItems.Select(GetItemName).ToList();

        string result = "Required: ";
        for (int i = 0; i < requiredItems.Count; i++)
        {
            bool isCollected = collectedNames.Contains(requiredItems[i]);
            result += isCollected ? $"✓{requiredItems[i]}" : requiredItems[i];

            if (i < requiredItems.Count - 1)
                result += ", ";
        }

        return result;
    }

    // Get progress percentage
    public float GetProgressPercentage()
    {
        return Progress * 100f;
    }

    // Check if specific item is still needed
    public bool IsSpecificItemNeeded(int itemValue)
    {
        if (!quest.UseMultiItemCollection) return true;
        return quest.IsSpecificItemRequired(itemValue) && !collectedSpecificItems.Contains(itemValue);
    }
}