using System.Collections.Generic;
using System.Linq;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class Quest_Manager : MonoBehaviour
{
    [Title("Quest Configuration")]
    [SerializeField] private FoodCollectionQuest[] availableQuests;
    [SerializeField] private int maxActiveQuests = 3;
    [SerializeField] private bool autoAssignQuests = true;

    [Title("Debug Info")]
    [SerializeField, ReadOnly] private List<QuestProgress> activeQuests = new List<QuestProgress>();
    [SerializeField, ReadOnly] private List<QuestProgress> completedQuests = new List<QuestProgress>();

    // Events
    public event Action<QuestProgress> OnQuestAdded;
    public event Action<QuestProgress> OnQuestProgressUpdated;
    public event Action<QuestProgress> OnQuestCompleted;
    public event Action<QuestProgress> OnQuestRewardClaimed;

    // Properties
    public static Quest_Manager Instance => SingletonManager.Instance.Get<Quest_Manager>();
    public IReadOnlyList<QuestProgress> ActiveQuests => activeQuests.AsReadOnly();
    public IReadOnlyList<QuestProgress> CompletedQuests => completedQuests.AsReadOnly();
    public int ActiveQuestCount => activeQuests.Count;
    public int CompletedQuestCount => completedQuests.Count;


    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }

    private void Start()
    {
        // Subscribe to food collection events
        Main.Observer.Add(EEvent.OnGoodFoodCollected, OnGoodFoodCollected);

        // Auto-assign initial quests if enabled
        if (autoAssignQuests)
        {
            AssignRandomQuests();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        Main.Observer.Remove(EEvent.OnGoodFoodCollected, OnGoodFoodCollected);

        // Unsubscribe from quest events
        foreach (var quest in activeQuests)
        {
            UnsubscribeFromQuestEvents(quest);
        }

        foreach (var quest in completedQuests)
        {
            UnsubscribeFromQuestEvents(quest);
        }
    }

    #region Quest Management

    [Button("Load Quests from Assets")]
    private void LoadQuestsFromAssets()
    {
#if UNITY_EDITOR
        string searchPath = "Assets/_Project/Datas/NormalQuestData";
        Debug.Log($"[Quest_Manager] Searching for FoodCollectionQuest assets in: {searchPath}");

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FoodCollectionQuest", new[] { searchPath });
        List<FoodCollectionQuest> foundQuests = new List<FoodCollectionQuest>();

        Debug.Log($"[Quest_Manager] Found {guids.Length} quest asset GUIDs");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"[Quest_Manager] Loading quest from path: {path}");

            FoodCollectionQuest quest = UnityEditor.AssetDatabase.LoadAssetAtPath<FoodCollectionQuest>(path);
            if (quest != null)
            {
                foundQuests.Add(quest);
                Debug.Log($"[Quest_Manager] Successfully loaded quest: {quest.QuestName} (Type: {quest.RequiredFoodType})");
            }
            else
            {
                Debug.LogWarning($"[Quest_Manager] Failed to load quest from path: {path}");
            }
        }

        availableQuests = foundQuests.ToArray();
        Debug.Log($"[Quest_Manager] ✓ Successfully loaded {availableQuests.Length} quests from {searchPath}");

        // Group by food type for better organization in logs
        var questsByType = foundQuests.GroupBy(q => q.RequiredFoodType);
        foreach (var group in questsByType)
        {
            Debug.Log($"[Quest_Manager] {group.Key} quests: {group.Count()}");
            foreach (var quest in group)
            {
                Debug.Log($"[Quest_Manager]   - {quest.QuestName}");
            }
        }
#else
        Debug.LogWarning("LoadQuestsFromAssets only works in Unity Editor");
#endif
    }

    public void AssignRandomQuests()
    {
        if (availableQuests == null || availableQuests.Length == 0)
        {
            Debug.LogWarning("No available quests to assign!");
            return;
        }

        Debug.Log($"[Quest_Manager] Assigning random quests. Current active: {activeQuests.Count}, Max: {maxActiveQuests}");

        // Fill up to max active quests
        while (activeQuests.Count < maxActiveQuests && availableQuests.Length > 0)
        {
            var randomQuest = availableQuests[UnityEngine.Random.Range(0, availableQuests.Length)];
            Debug.Log($"[Quest_Manager] Attempting to add quest: {randomQuest.QuestName}");
            AddQuest(randomQuest);
        }

        Debug.Log($"[Quest_Manager] Quest assignment complete. Active quests: {activeQuests.Count}");
    }

    public bool AddQuest(FoodCollectionQuest quest)
    {
        HPDebug.Log("Adding quest...");
        if (quest == null)
        {
            Debug.LogWarning("Cannot add null quest!");
            return false;
        }

        if (activeQuests.Count >= maxActiveQuests)
        {
            Debug.LogWarning($"Cannot add quest '{quest.QuestName}' - maximum active quests reached ({maxActiveQuests})!");
            return false;
        }

        if (IsQuestActive(quest))
        {
            Debug.LogWarning($"Quest '{quest.QuestName}' is already active!");
            return false;
        }

        // Generate random specific items since all quests are now multi-item collection
        if (quest.SelectedSpecificItems.Count == 0)
        {
            quest.GenerateRandomSpecificItems();
        }

        var questProgress = new QuestProgress(quest);
        SubscribeToQuestEvents(questProgress);
        activeQuests.Add(questProgress);
        HPDebug.Log("Adding quest done");

        OnQuestAdded?.Invoke(questProgress);

        Debug.Log($"Quest '{quest.QuestName}' added! Target: {string.Join(", ", quest.GetRequiredItemNames())}");
        return true;
    }

    public bool RemoveQuest(FoodCollectionQuest quest)
    {
        var questProgress = GetActiveQuestProgress(quest);
        if (questProgress == null)
        {
            Debug.LogWarning($"Quest '{quest.QuestName}' is not active!");
            return false;
        }

        UnsubscribeFromQuestEvents(questProgress);
        activeQuests.Remove(questProgress);

        Debug.Log($"Quest '{quest.QuestName}' removed!");
        return true;
    }

    public void CompleteQuest(QuestProgress questProgress)
    {
        if (!activeQuests.Contains(questProgress)) return;

        UnsubscribeFromQuestEvents(questProgress);
        activeQuests.Remove(questProgress);
        completedQuests.Add(questProgress);

        OnQuestCompleted?.Invoke(questProgress);

        // Auto-assign new quest if enabled - but only assign ONE quest to prevent cascading
        if (autoAssignQuests && activeQuests.Count < maxActiveQuests)
        {
            var randomQuest = availableQuests[UnityEngine.Random.Range(0, availableQuests.Length)];
            AddQuest(randomQuest);
        }
    }

    public void ClaimQuestReward(FoodCollectionQuest quest)
    {
        var questProgress = GetCompletedQuestProgress(quest);
        if (questProgress != null)
        {
            questProgress.ClaimReward();
        }
    }

    #endregion

    #region Event Handling

    private void OnGoodFoodCollected(object data)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[Quest_Manager] OnGoodFoodCollected called with data: {data}");
#endif

        if (data is GoodFood food)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Quest_Manager] Food type: {food.GetType().Name}, FoodType: {food.FoodType}");
#endif
            ProcessFoodCollection(food);
        }
        else
        {
            Debug.LogWarning($"[Quest_Manager] Unexpected data type in OnGoodFoodCollected: {data?.GetType().Name}");
        }
    }

    private void ProcessFoodCollection(GoodFood food)
    {
        EFoodType foodType = food.FoodType;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[Quest_Manager] Processing food collection: {foodType}");
#endif

        // Update progress for all active quests that match this food type
        foreach (var questProgress in activeQuests.ToList()) // ToList to avoid modification during iteration
        {
            if (questProgress.Quest.RequiredFoodType == foodType)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Quest_Manager] Found matching quest: {questProgress.Quest.QuestName}");
#endif

                // All quests are now multi-item collection quests
                int specificItemValue = GetSpecificItemValue(food);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Quest_Manager] Specific item value: {specificItemValue}");
#endif

                if (specificItemValue != -1)
                {
                    bool wasAdded = questProgress.AddSpecificItem(specificItemValue);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[Quest_Manager] Specific item added to quest: {wasAdded}");
#endif
                }
            }
        }
    }

    private int GetSpecificItemValue(GoodFood food)
    {
        switch (food.FoodType)
        {
            case EFoodType.Fruit:
                if (food is Fruit fruitFood)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[Quest_Manager] Fruit detected: {fruitFood.FruitType} (value: {(int)fruitFood.FruitType})");
#endif
                    return (int)fruitFood.FruitType;
                }
                else
                {
                    Debug.LogWarning($"[Quest_Manager] Food marked as Fruit but is not Fruit class: {food.GetType().Name}");
                }
                break;
            case EFoodType.FastFood:
                if (food is FastFood fastFood)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[Quest_Manager] FastFood detected: {fastFood.FastFoodType} (value: {(int)fastFood.FastFoodType})");
#endif
                    return (int)fastFood.FastFoodType;
                }
                else
                {
                    Debug.LogWarning($"[Quest_Manager] Food marked as FastFood but is not FastFood class: {food.GetType().Name}");
                }
                break;
            case EFoodType.Cake:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Quest_Manager] Cake type not yet implemented");
#endif
                break;
            default:
                Debug.LogWarning($"[Quest_Manager] Unknown food type: {food.FoodType}");
                break;
        }
        return -1; // Invalid or unsupported food type
    }

    private void SubscribeToQuestEvents(QuestProgress questProgress)
    {
        questProgress.OnProgressUpdated += HandleQuestProgressUpdated;
        questProgress.OnQuestCompleted += HandleQuestCompleted;
        questProgress.OnRewardClaimed += HandleQuestRewardClaimed;
    }

    private void UnsubscribeFromQuestEvents(QuestProgress questProgress)
    {
        questProgress.OnProgressUpdated -= HandleQuestProgressUpdated;
        questProgress.OnQuestCompleted -= HandleQuestCompleted;
        questProgress.OnRewardClaimed -= HandleQuestRewardClaimed;
    }

    private void HandleQuestProgressUpdated(QuestProgress questProgress)
    {
        OnQuestProgressUpdated?.Invoke(questProgress);
    }

    private void HandleQuestCompleted(QuestProgress questProgress)
    {
        CompleteQuest(questProgress);
    }

    private void HandleQuestRewardClaimed(QuestProgress questProgress)
    {
        OnQuestRewardClaimed?.Invoke(questProgress);
    }

    #endregion

    #region Query Methods

    public QuestProgress GetActiveQuestProgress(FoodCollectionQuest quest)
    {
        return activeQuests.FirstOrDefault(q => q.Quest == quest);
    }

    public QuestProgress GetCompletedQuestProgress(FoodCollectionQuest quest)
    {
        return completedQuests.FirstOrDefault(q => q.Quest == quest);
    }

    public bool IsQuestActive(FoodCollectionQuest quest)
    {
        return activeQuests.Any(q => q.Quest == quest);
    }

    public bool IsQuestCompleted(FoodCollectionQuest quest)
    {
        return completedQuests.Any(q => q.Quest == quest);
    }

    public List<QuestProgress> GetQuestsByFoodType(EFoodType foodType)
    {
        return activeQuests.Where(q => q.Quest.RequiredFoodType == foodType).ToList();
    }

    public float GetOverallProgress()
    {
        if (activeQuests.Count == 0) return 0f;
        return activeQuests.Average(q => q.Progress);
    }

    #endregion
}