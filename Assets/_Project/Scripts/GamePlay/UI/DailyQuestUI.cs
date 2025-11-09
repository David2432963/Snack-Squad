using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OSK;
using System.Linq;

public class DailyQuestUI : View
{
    [Header("UI References")]
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject dailyQuestItemPrefab;
    [SerializeField] private Transform questItemParent;
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Slider overallProgressSlider;
    [SerializeField] private TextMeshProUGUI overallProgressText;

    private List<DailyQuestItemUI> questItems = new List<DailyQuestItemUI>();
    private DailyQuest_Manager dailyQuestManager;

    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        dailyQuestManager = DailyQuest_Manager.Instance;
    }

    public override void Open(object[] data = null)
    {
        base.Open(data);

        // Get fresh reference to dailyQuestManager in case it wasn't available during Initialize
        if (dailyQuestManager == null)
        {
            dailyQuestManager = DailyQuest_Manager.Instance;
        }

        // Always refresh UI when opening to get latest data
        if (dailyQuestManager != null)
        {
            RefreshUI();
        }
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void OnDestroy()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveListener(OnRefreshButtonClicked);
        }
    }

    private void RefreshUI()
    {
        ClearQuestItems();
        CreateQuestItems();
        UpdateOverallProgress();
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

    private void CreateQuestItems()
    {
        if (dailyQuestManager == null || dailyQuestItemPrefab == null || questContainer == null)
            return;

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

    private void UpdateOverallProgress()
    {
        if (dailyQuestManager == null) return;

        float progress = dailyQuestManager.GetOverallDailyProgress();

        if (overallProgressSlider != null)
        {
            overallProgressSlider.value = progress;
        }

        if (overallProgressText != null)
        {
            overallProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
    }

    private void OnRefreshButtonClicked()
    {
        // Manually refresh UI to get latest quest data from manager
        RefreshUI();
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