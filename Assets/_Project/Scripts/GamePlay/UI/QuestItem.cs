using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class QuestItem : MonoBehaviour
{
    [Title("UI Components")]
    [SerializeField] private Text questDescriptionText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text detailedProgressText; // New field for detailed multi-item progress
    [SerializeField] private Image questIcon;

    [Title("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color claimedColor = Color.gray;

    private QuestProgress currentQuestProgress;

    public void Setup(QuestProgress questProgress)
    {
        currentQuestProgress = questProgress;

        // Subscribe to quest progress events
        if (currentQuestProgress != null)
        {
            currentQuestProgress.OnProgressUpdated += OnProgressUpdated;
            currentQuestProgress.OnQuestCompleted += OnQuestCompleted;
            currentQuestProgress.OnRewardClaimed += OnRewardClaimed;
        }

        // Setup claim reward button
        // if (claimRewardButton != null)
        // {
        //     claimRewardButton.onClick.AddListener(ClaimReward);
        // }

        UpdateDisplay();
    }

    private void OnDestroy()
    {
        // Unsubscribe from quest progress events
        if (currentQuestProgress != null)
        {
            currentQuestProgress.OnProgressUpdated -= OnProgressUpdated;
            currentQuestProgress.OnQuestCompleted -= OnQuestCompleted;
            currentQuestProgress.OnRewardClaimed -= OnRewardClaimed;
        }

        // if (claimRewardButton != null)
        // {
        //     claimRewardButton.onClick.RemoveListener(ClaimReward);
        // }
    }

    [Button("Update Display")]
    public void UpdateDisplay()
    {
        if (currentQuestProgress == null) return;

        var quest = currentQuestProgress.Quest;

        // Update quest description - always show the auto-generated description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = quest.QuestDescription;
        }

        // Update progress text
        if (progressText != null)
        {
            progressText.text = $"{currentQuestProgress.CurrentAmount}/{quest.TargetAmount} items";
        }

        // Update detailed progress text
        if (detailedProgressText != null)
        {
            detailedProgressText.text = currentQuestProgress.GetDetailedProgressText();
            detailedProgressText.gameObject.SetActive(true);
        }

        // Update quest icon
        if (questIcon != null && quest.QuestIcon != null)
        {
            questIcon.sprite = quest.QuestIcon;
        }

        // Update visual state based on quest status
        UpdateVisualState();
    }

    [Button("Print Quest Details"), Title("Debug Actions")]
    private void DebugPrintQuestDetails()
    {
        if (currentQuestProgress == null)
        {
            Debug.Log("[QuestItem] No quest progress assigned");
            return;
        }

        var quest = currentQuestProgress.Quest;
        Debug.Log($"=== QUEST DETAILS ===");
        Debug.Log($"Name: {quest.QuestName}");
        Debug.Log($"Description: {quest.QuestDescription}");
        Debug.Log($"Food Type: {quest.RequiredFoodType}");
        Debug.Log($"Target Amount: {quest.TargetAmount}");
        Debug.Log($"Required Items: {string.Join(", ", quest.GetRequiredItemNames())}");
        Debug.Log($"Detailed Progress: {currentQuestProgress.GetDetailedProgressText()}");
        Debug.Log($"Current Progress: {currentQuestProgress.GetProgressText()}");
        Debug.Log($"Completion: {currentQuestProgress.GetProgressPercentage():F1}%");
        Debug.Log($"Status: {(currentQuestProgress.IsCompleted ? "Completed" : "In Progress")}");
        Debug.Log($"Reward: {GetTotalRewardValue()} points");
    }

    private void UpdateVisualState()
    {
        Color targetColor = normalColor;

        if (currentQuestProgress.IsRewardClaimed)
        {
            targetColor = claimedColor;
        }
        else if (currentQuestProgress.IsCompleted)
        {
            targetColor = completedColor;
        }

        // Apply color to detailed progress text
        if (detailedProgressText != null)
        {
            detailedProgressText.color = targetColor;
        }
    }

    private void ClaimReward()
    {
        if (currentQuestProgress != null && currentQuestProgress.IsCompleted && !currentQuestProgress.IsRewardClaimed)
        {
            currentQuestProgress.ClaimReward();
        }
    }

    #region Event Handlers

    private void OnProgressUpdated(QuestProgress questProgress)
    {
        UpdateDisplay();
    }

    private void OnQuestCompleted(QuestProgress questProgress)
    {
        ClaimReward();
        UpdateDisplay();
        // Add completion effect here if desired
        Debug.Log($"Quest '{questProgress.Quest.QuestName}' completed in UI!");
    }

    private void OnRewardClaimed(QuestProgress questProgress)
    {
        UpdateDisplay();

        // Add reward claim effect here if desired
        Debug.Log($"Reward claimed for quest '{questProgress.Quest.QuestName}' in UI!");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get a formatted string showing the quest type and requirements
    /// </summary>
    public string GetQuestTypeDescription()
    {
        if (currentQuestProgress == null) return "";

        var quest = currentQuestProgress.Quest;
        var requiredItems = quest.GetRequiredItemNames();
        return $"Collect specific items: {string.Join(", ", requiredItems)}";
    }

    /// <summary>
    /// Get the quest completion percentage as a formatted string
    /// </summary>
    public string GetProgressPercentageString()
    {
        if (currentQuestProgress == null) return "0%";
        return $"{currentQuestProgress.GetProgressPercentage():F0}%";
    }

    /// <summary>
    /// Check if this quest is eligible for reward claiming
    /// </summary>
    public bool CanClaimReward()
    {
        return currentQuestProgress != null &&
               currentQuestProgress.IsCompleted &&
               !currentQuestProgress.IsRewardClaimed;
    }

    /// <summary>
    /// Get the total reward value for this quest
    /// </summary>
    public int GetTotalRewardValue()
    {
        if (currentQuestProgress == null) return 0;
        var quest = currentQuestProgress.Quest;
        return quest.ScoreReward + quest.BonusScore;
    }

    #endregion
}