using System.Collections.Generic;
using UnityEngine;
using OSK;
using System.Linq;
using UnityEngine.UI;

public class DailyQuestUI : View
{
    [Header("UI References")]
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject dailyQuestItemPrefab;
    [SerializeField] private Transform achievementContainer;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private Button btnClose;

    private List<DailyQuestItemUI> questItems = new List<DailyQuestItemUI>();
    private List<AchievementItemUI> achievementItems = new List<AchievementItemUI>();
    private DailyQuest_Manager dailyQuestManager;
    private Achievement_Manager achievementManager;

    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);

        btnClose.onClick.AddListener(Close);
        dailyQuestManager = DailyQuest_Manager.Instance;
        achievementManager = Achievement_Manager.Instance;
    }

    public override void Open(object[] data = null)
    {
        base.Open(data);

        // Get fresh reference to managers in case they weren't available during Initialize
        if (dailyQuestManager == null)
        {
            dailyQuestManager = DailyQuest_Manager.Instance;
        }

        if (achievementManager == null)
        {
            achievementManager = Achievement_Manager.Instance;
        }

        // Always refresh UI when opening to get latest data
        if (dailyQuestManager != null)
        {
            RefreshUI();
        }

        if (achievementManager != null)
        {
            RefreshAchievements();
        }
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void Close()
    {
        Main.UI.Hide(this);
    }

    private void RefreshUI()
    {
        ClearQuestItems();
        CreateQuestItems();
    }

    private void RefreshAchievements()
    {
        ClearAchievementItems();
        CreateAchievementItems();
    }

    private void ClearQuestItems()
    {
        foreach (var item in questItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        questItems.Clear();
    }

    private void ClearAchievementItems()
    {
        foreach (var item in achievementItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        achievementItems.Clear();
    }

    private void CreateQuestItems()
    {
        if (dailyQuestManager == null || dailyQuestItemPrefab == null || questContainer == null)
            return;

        // Refresh progress from GameData before creating UI
        RefreshQuestProgressFromGameData();

        var activeQuests = dailyQuestManager.ActiveDailyQuests.ToList();
        var completedQuests = dailyQuestManager.CompletedDailyQuests.ToList();

        // Create items for active quests
        foreach (var quest in activeQuests)
        {
            CreateQuestItem(quest, false);
        }

        // Create items for completed quests
        foreach (var quest in completedQuests)
        {
            CreateQuestItem(quest, true);
        }
    }

    private void RefreshQuestProgressFromGameData()
    {
        if (dailyQuestManager == null) return;

        // Refresh progress for all quests that depend on GameData
        var allQuests = dailyQuestManager.ActiveDailyQuests.Concat(dailyQuestManager.CompletedDailyQuests).ToList();

        foreach (var questProgress in allQuests)
        {
            if (questProgress.Quest.QuestType == EDailyQuestType.CompleteNormalQuests ||
                questProgress.Quest.QuestType == EDailyQuestType.CollectSpecificFood)
            {
                questProgress.RefreshProgressFromGameData();
            }
        }
    }

    private void CreateAchievementItems()
    {
        if (achievementManager == null || achievementItemPrefab == null || achievementContainer == null)
            return;

        var allAchievements = achievementManager.AllAchievements.ToList();

        // Create items for all achievements
        foreach (var achievement in allAchievements)
        {
            CreateAchievementItem(achievement, achievement.IsUnlocked);
        }
    }

    private void CreateQuestItem(DailyQuestProgress questProgress, bool isCompleted)
    {
        GameObject itemObj = Instantiate(dailyQuestItemPrefab, questContainer);
        DailyQuestItemUI questItem = itemObj.GetComponent<DailyQuestItemUI>();

        if (questItem != null)
        {
            questItem.Initialize(questProgress, isCompleted);
            questItems.Add(questItem);
        }
    }

    private void CreateAchievementItem(AchievementProgress achievementProgress, bool isUnlocked)
    {
        GameObject itemObj = Instantiate(achievementItemPrefab, achievementContainer);
        AchievementItemUI achievementItem = itemObj.GetComponent<AchievementItemUI>();

        if (achievementItem != null)
        {
            achievementItem.Initialize(achievementProgress, isUnlocked);
            achievementItems.Add(achievementItem);
        }
    }

    private void OnRefreshButtonClicked()
    {
        // Manually refresh UI to get latest quest data from manager
        RefreshUI();
        RefreshAchievements();
    }

    #region Public Methods

    public void ShowDailyQuests()
    {
        Open();
    }

    public void HideDailyQuests()
    {
        Hide();
    }

    #endregion
}