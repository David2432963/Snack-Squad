using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using OSK;

public class QuestUI : MonoBehaviour
{
    [Title("UI Components")]
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject questItemPrefab;

    [Title("Settings")]
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private bool skipFirstTime;
    [SerializeField] private float refreshInterval = 1f;

    private bool isSkipFirstTime;

    private float lastRefreshTime;

    public void Initialize()
    {
        Main.Observer.Add(EEvent.OnQuestAdded, OnQuestAdded);
        Main.Observer.Add(EEvent.OnQuestProgressUpdated, OnQuestProgressUpdated);
        Main.Observer.Add(EEvent.OnQuestCompleted, OnQuestCompleted);
        Main.Observer.Add(EEvent.OnQuestRewardClaimed, OnQuestRewardClaimed);
    }
    private void Awake()
    {
        // Subscribe to quest events

    }
    private void Start()
    {
        // Initial refresh
        // RefreshQuestDisplay();
    }
    private void OnDestroy()
    {
        // Unsubscribe from quest events
        Main.Observer.Remove(EEvent.OnQuestAdded, OnQuestAdded);
        Main.Observer.Remove(EEvent.OnQuestProgressUpdated, OnQuestProgressUpdated);
        Main.Observer.Remove(EEvent.OnQuestCompleted, OnQuestCompleted);
        Main.Observer.Remove(EEvent.OnQuestRewardClaimed, OnQuestRewardClaimed);
    }

    private void Update()
    {
        // Only auto-refresh if enabled and enough time has passed
        if (autoRefresh && Time.time - lastRefreshTime >= refreshInterval)
        {
            // Only refresh if we haven't done the first time skip logic yet
            if (!isSkipFirstTime && skipFirstTime)
            {
                RefreshQuestDisplay();
                isSkipFirstTime = true;
            }
            else if (!skipFirstTime)
            {
                RefreshQuestDisplay();
            }

            lastRefreshTime = Time.time;
        }
    }



    [Button("Refresh Quest Display")]
    public void RefreshQuestDisplay(object data = null)
    {
        if (Quest_Manager.Instance == null || questContainer == null) return;

        // Clear existing quest items
        foreach (Transform child in questContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        // Create quest items for active quests
        foreach (var questProgress in Quest_Manager.Instance.ActiveQuests)
        {
            HPDebug.Log("[QuestUI] Creating quest item for: " + questProgress.Quest.QuestName);
            CreateQuestItem(questProgress);
        }
    }

    private void CreateQuestItem(QuestProgress questProgress)
    {
        if (questItemPrefab == null || questContainer == null) return;

        GameObject questItem = Instantiate(questItemPrefab, questContainer);
        QuestItem questItemComponent = questItem.GetComponent<QuestItem>();

        if (questItemComponent != null)
        {
            questItemComponent.Setup(questProgress);
        }
        else
        {
            // Fallback: Setup basic UI components if QuestItem component doesn't exist
            SetupBasicQuestItem(questItem, questProgress);
        }
    }

    private void SetupBasicQuestItem(GameObject questItem, QuestProgress questProgress)
    {
        // Try to find basic UI components
        Text[] texts = questItem.GetComponentsInChildren<Text>();
        Slider slider = questItem.GetComponentInChildren<Slider>();

        if (texts.Length > 0)
        {
            texts[0].text = questProgress.Quest.QuestName;
        }

        if (texts.Length > 1)
        {
            texts[1].text = $"{questProgress.CurrentAmount}/{questProgress.TargetAmount} items collected";
        }

        // Show detailed progress if there's a third text component
        if (texts.Length > 2)
        {
            texts[2].text = questProgress.GetDetailedProgressText();
        }

        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = questProgress.TargetAmount;
            slider.value = questProgress.CurrentAmount;
        }
    }

    #region Event Handlers

    private void OnQuestAdded(object data)
    {
        if (data is QuestProgress questProgress)
        {
            Debug.Log($"[QuestUI] Quest added: {questProgress.Quest.QuestName}");
            RefreshQuestDisplay();
        }
    }

    private void OnQuestProgressUpdated(object data)
    {
        if (data is QuestProgress questProgress)
        {
            // Update individual quest items instead of refreshing everything
            // This is more efficient than full refresh
            if (!autoRefresh)
            {
                // Only refresh if auto-refresh is disabled
                // If auto-refresh is enabled, let the Update loop handle it
                RefreshQuestDisplay();
            }
        }
    }

    private void OnQuestCompleted(object data)
    {
        if (data is QuestProgress questProgress)
        {
            Debug.Log($"[QuestUI] Quest completed: {questProgress.Quest.QuestName}");
            RefreshQuestDisplay();
        }
    }

    private void OnQuestRewardClaimed(object data)
    {
        if (data is QuestProgress questProgress)
        {
            Debug.Log($"[QuestUI] Quest reward claimed: {questProgress.Quest.QuestName}");
            RefreshQuestDisplay();
        }
    }

    #endregion
}