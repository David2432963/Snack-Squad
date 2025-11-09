using System.Collections.Generic;
using System.Linq;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class DailyQuest_Manager : MonoBehaviour
{
    [Title("Daily Quest Configuration")]
    [SerializeField] private DailyQuestSO[] availableDailyQuests;
    [SerializeField] private int maxDailyQuests = 2;
    [SerializeField] private bool autoAssignDailyQuests = true;

    [Title("Debug Info")]
    [SerializeField, ReadOnly] private List<DailyQuestProgress> activeDailyQuests = new List<DailyQuestProgress>();
    [SerializeField, ReadOnly] private List<DailyQuestProgress> completedDailyQuests = new List<DailyQuestProgress>();
    [SerializeField, ReadOnly] private DateTime lastLoginDate;

    // Properties
    public static DailyQuest_Manager Instance => SingletonManager.Instance.Get<DailyQuest_Manager>();
    public IReadOnlyList<DailyQuestProgress> ActiveDailyQuests => activeDailyQuests.AsReadOnly();
    public IReadOnlyList<DailyQuestProgress> CompletedDailyQuests => completedDailyQuests.AsReadOnly();
    public int ActiveDailyQuestCount => activeDailyQuests.Count;

    private DailyQuestSaveData saveData;

    private void Awake()
    {
        SingletonManager.Instance.RegisterGlobal(this);
        Main.Observer.Add(EEvent.OnGoodFoodCollected, OnGoodFoodCollected);
    }

    private void Start()
    {
        // Subscribe to events for tracking daily quest progress

        // Load daily quest data
        LoadDailyQuestData();

        // Auto-load daily quests if array is empty
        if (availableDailyQuests == null || availableDailyQuests.Length == 0)
        {
            LoadDailyQuestsFromAssets();
        }

        // Check and assign daily quests
        CheckAndAssignDailyQuests();
    }

    private void OnDestroy()
    {
        // Save data before destroying
        SaveDailyQuestData();

        // Unsubscribe from events
        Main.Observer.Remove(EEvent.OnGoodFoodCollected, OnGoodFoodCollected);

        // Unsubscribe from daily quest events
        foreach (var quest in activeDailyQuests)
        {
            UnsubscribeFromDailyQuestEvents(quest);
        }

        foreach (var quest in completedDailyQuests)
        {
            UnsubscribeFromDailyQuestEvents(quest);
        }
    }

    #region Daily Quest Management

    [Button("Load Daily Quests from Assets")]
    private void LoadDailyQuestsFromAssets()
    {
#if UNITY_EDITOR
        string searchPath = "Assets/_Project/Datas/DailyQuestData";
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DailyQuestSO", new[] { searchPath });
        List<DailyQuestSO> foundQuests = new List<DailyQuestSO>();

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            DailyQuestSO quest = UnityEditor.AssetDatabase.LoadAssetAtPath<DailyQuestSO>(path);
            if (quest != null)
            {
                foundQuests.Add(quest);
            }
        }

        availableDailyQuests = foundQuests.ToArray();
#endif
    }

    private void CheckAndAssignDailyQuests()
    {
        // Check if it's a new day
        DateTime today = DateTime.Now.Date;

        if (lastLoginDate.Date != today)
        {
            // New day detected
            HandleNewDay();
        }

        // Assign daily quests if needed
        if (autoAssignDailyQuests && activeDailyQuests.Count == 0)
        {
            AssignRandomDailyQuests();
        }

        // Remove expired quests
        RemoveExpiredQuests();
    }

    private void HandleNewDay()
    {
        DateTime today = DateTime.Now.Date;

        lastLoginDate = today;

        // Clear completed quests from yesterday
        ClearExpiredCompletedQuests();

        // Assign new daily quests
        if (autoAssignDailyQuests)
        {
            AssignRandomDailyQuests();
        }
    }

    private void AssignRandomDailyQuests()
    {
        // Clear any existing active quests
        foreach (var quest in activeDailyQuests.ToList())
        {
            RemoveDailyQuest(quest.Quest);
        }

        // Create the 2 types of daily quests
        CreateCompleteQuestsDaily();
        CreateCollectSpecificFoodDaily();
    }

    private void CreateCompleteQuestsDaily()
    {
        // Create a "Complete 2 quests" daily quest
        var completeQuestsDaily = ScriptableObject.CreateInstance<DailyQuestSO>();
        completeQuestsDaily.name = "Complete Normal Quests Daily";

        // Use reflection to set private fields
        var questNameField = typeof(DailyQuestSO).GetField("questName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var questDescField = typeof(DailyQuestSO).GetField("questDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var questTypeField = typeof(DailyQuestSO).GetField("questType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var targetValueField = typeof(DailyQuestSO).GetField("targetValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var goldRewardField = typeof(DailyQuestSO).GetField("goldReward", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        questNameField?.SetValue(completeQuestsDaily, "Quest Completionist");
        questDescField?.SetValue(completeQuestsDaily, "Complete 2 normal quests (collect 3 required foods each)");
        questTypeField?.SetValue(completeQuestsDaily, EDailyQuestType.CompleteNormalQuests);
        targetValueField?.SetValue(completeQuestsDaily, 2);
        goldRewardField?.SetValue(completeQuestsDaily, 100);

        AddDailyQuest(completeQuestsDaily);
    }

    private void CreateCollectSpecificFoodDaily()
    {
        // Create a "Collect 10 specific foods" daily quest
        var collectFoodDaily = ScriptableObject.CreateInstance<DailyQuestSO>();
        collectFoodDaily.name = "Collect Specific Food Daily";

        // Randomly choose a food type and then a specific item within that type
        var foodTypes = System.Enum.GetValues(typeof(EFoodType));
        EFoodType randomFoodType = (EFoodType)foodTypes.GetValue(UnityEngine.Random.Range(0, foodTypes.Length));

        // Use reflection to set private fields
        var questNameField = typeof(DailyQuestSO).GetField("questName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var questDescField = typeof(DailyQuestSO).GetField("questDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var questTypeField = typeof(DailyQuestSO).GetField("questType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var targetValueField = typeof(DailyQuestSO).GetField("targetValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var foodTypeField = typeof(DailyQuestSO).GetField("requiredFoodType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var goldRewardField = typeof(DailyQuestSO).GetField("goldReward", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Set common fields
        questTypeField?.SetValue(collectFoodDaily, EDailyQuestType.CollectSpecificFood);
        targetValueField?.SetValue(collectFoodDaily, 10);
        foodTypeField?.SetValue(collectFoodDaily, randomFoodType);
        goldRewardField?.SetValue(collectFoodDaily, 50);

        string foodName = "";

        // Set specific food type based on category
        switch (randomFoodType)
        {
            case EFoodType.Fruit:
                var fruitTypes = System.Enum.GetValues(typeof(EFruitType));
                EFruitType randomFruit = (EFruitType)fruitTypes.GetValue(UnityEngine.Random.Range(0, fruitTypes.Length));
                var fruitField = typeof(DailyQuestSO).GetField("requiredFruitType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fruitField?.SetValue(collectFoodDaily, randomFruit);
                foodName = randomFruit.ToString();
                break;
            case EFoodType.FastFood:
                var fastFoodTypes = System.Enum.GetValues(typeof(EFastFoodType));
                EFastFoodType randomFastFood = (EFastFoodType)fastFoodTypes.GetValue(UnityEngine.Random.Range(0, fastFoodTypes.Length));
                var fastFoodField = typeof(DailyQuestSO).GetField("requiredFastFoodType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fastFoodField?.SetValue(collectFoodDaily, randomFastFood);
                foodName = randomFastFood.ToString();
                break;
            case EFoodType.Cake:
                var cakeTypes = System.Enum.GetValues(typeof(ECakeType));
                ECakeType randomCake = (ECakeType)cakeTypes.GetValue(UnityEngine.Random.Range(0, cakeTypes.Length));
                var cakeField = typeof(DailyQuestSO).GetField("requiredCakeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cakeField?.SetValue(collectFoodDaily, randomCake);
                foodName = randomCake.ToString();
                break;
        }

        questNameField?.SetValue(collectFoodDaily, $"{foodName} Collector");
        questDescField?.SetValue(collectFoodDaily, $"Collect 10 {foodName} items");

        AddDailyQuest(collectFoodDaily);
    }

    private bool AddDailyQuest(DailyQuestSO quest)
    {
        if (quest == null)
            return false;

        if (activeDailyQuests.Count >= maxDailyQuests)
            return false;

        if (IsDailyQuestActive(quest))
            return false;

        var questProgress = new DailyQuestProgress(quest);
        SubscribeToDailyQuestEvents(questProgress);
        activeDailyQuests.Add(questProgress);

        Main.Observer.Notify(EEvent.OnDailyQuestAdded, questProgress);

        return true;
    }

    private bool RemoveDailyQuest(DailyQuestSO quest)
    {
        var questProgress = GetActiveDailyQuestProgress(quest);
        if (questProgress == null)
            return false;

        UnsubscribeFromDailyQuestEvents(questProgress);
        activeDailyQuests.Remove(questProgress);

        return true;
    }

    private void CompleteDailyQuest(DailyQuestProgress questProgress)
    {
        if (!activeDailyQuests.Contains(questProgress)) return;

        UnsubscribeFromDailyQuestEvents(questProgress);
        activeDailyQuests.Remove(questProgress);
        completedDailyQuests.Add(questProgress);

        Main.Observer.Notify(EEvent.OnDailyQuestCompleted, questProgress);
    }

    private void RemoveExpiredQuests()
    {
        var expiredQuests = activeDailyQuests.Where(q => q.IsExpired).ToList();
        foreach (var expiredQuest in expiredQuests)
        {
            RemoveDailyQuest(expiredQuest.Quest);
        }
    }

    private void ClearExpiredCompletedQuests()
    {
        completedDailyQuests.RemoveAll(q => q.IsExpired);
    }

    #endregion

    #region Event Handling

    private void OnGoodFoodCollected(object data)
    {
        if (data is FoodCollectionData collectionData)
        {
            if (collectionData.playerType == EPlayerType.Player)
            {
                ProcessSpecificFoodCollection(collectionData.food);
            }
        }
        else if (data is GoodFood food)
        {
            ProcessSpecificFoodCollection(food);
        }
    }

    private void ProcessSpecificFoodCollection(GoodFood food)
    {
        foreach (var questProgress in activeDailyQuests.ToList())
        {
            if (questProgress.Quest.QuestType == EDailyQuestType.CollectSpecificFood)
            {
                if (DoesMatchRequiredFood(questProgress.Quest, food))
                {
                    questProgress.AddProgress(1);
                }
            }
        }
    }

    private bool DoesMatchRequiredFood(DailyQuestSO quest, GoodFood food)
    {
        // First check if the food type matches
        if (quest.RequiredFoodType != food.FoodType)
            return false;

        // Then check the specific type within that category
        switch (quest.RequiredFoodType)
        {
            case EFoodType.Fruit:
                if (food is Fruit fruitFood)
                {
                    return quest.RequiredFruitType == fruitFood.FruitType;
                }
                break;
            case EFoodType.FastFood:
                if (food is FastFood fastFood)
                {
                    return quest.RequiredFastFoodType == fastFood.FastFoodType;
                }
                break;
            case EFoodType.Cake:
                if (food is Cake cakeFood)
                {
                    return quest.RequiredCakeType == cakeFood.CakeType;
                }
                break;
        }
        return false;
    }

    private void SubscribeToDailyQuestEvents(DailyQuestProgress questProgress)
    {
        questProgress.OnProgressUpdated += HandleDailyQuestProgressUpdated;
        questProgress.OnQuestCompleted += HandleDailyQuestCompleted;
        questProgress.OnRewardClaimed += HandleDailyQuestRewardClaimed;
    }

    private void UnsubscribeFromDailyQuestEvents(DailyQuestProgress questProgress)
    {
        questProgress.OnProgressUpdated -= HandleDailyQuestProgressUpdated;
        questProgress.OnQuestCompleted -= HandleDailyQuestCompleted;
        questProgress.OnRewardClaimed -= HandleDailyQuestRewardClaimed;
    }

    private void HandleDailyQuestProgressUpdated(DailyQuestProgress questProgress)
    {
        Main.Observer.Notify(EEvent.OnDailyQuestProgressUpdated, questProgress);
    }

    private void HandleDailyQuestCompleted(DailyQuestProgress questProgress)
    {
        CompleteDailyQuest(questProgress);
    }

    private void HandleDailyQuestRewardClaimed(DailyQuestProgress questProgress)
    {
        Main.Observer.Notify(EEvent.OnDailyQuestRewardClaimed, questProgress);
    }

    #endregion

    #region Save/Load System

    private void LoadDailyQuestData()
    {
        saveData = DailyQuestSaveSystem.LoadDailyQuestData();

        // Parse last login date
        if (DateTime.TryParse(saveData.lastLoginDate, out DateTime loginDate))
        {
            lastLoginDate = loginDate;
        }
        else
        {
            lastLoginDate = DateTime.Now.Date;
        }

        // Load active quests
        LoadActiveDailyQuests(saveData.activeQuests);
    }

    private void LoadActiveDailyQuests(List<DailyQuestSaveEntry> questEntries)
    {
        if (availableDailyQuests == null || availableDailyQuests.Length == 0)
            return;

        foreach (var entry in questEntries)
        {
            var quest = availableDailyQuests.FirstOrDefault(q => q.QuestName == entry.questName);
            if (quest != null)
            {
                var questProgress = new DailyQuestProgress(quest, entry.currentProgress,
                    entry.isCompleted, entry.isRewardClaimed, entry.GetAssignedDate());

                SubscribeToDailyQuestEvents(questProgress);

                if (questProgress.IsCompleted)
                {
                    completedDailyQuests.Add(questProgress);
                }
                else if (!questProgress.IsExpired)
                {
                    activeDailyQuests.Add(questProgress);
                }
            }
        }
    }

    private void SaveDailyQuestData()
    {
        if (saveData == null)
            saveData = new DailyQuestSaveData();

        saveData.lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd");
        saveData.activeQuests.Clear();

        // Save active quests
        foreach (var quest in activeDailyQuests)
        {
            var entry = new DailyQuestSaveEntry(quest.Quest.QuestName, quest.CurrentProgress,
                quest.IsCompleted, quest.IsRewardClaimed, quest.AssignedDate);
            saveData.activeQuests.Add(entry);
        }

        // Save completed quests (for current day only)
        foreach (var quest in completedDailyQuests.Where(q => !q.IsExpired))
        {
            var entry = new DailyQuestSaveEntry(quest.Quest.QuestName, quest.CurrentProgress,
                quest.IsCompleted, quest.IsRewardClaimed, quest.AssignedDate);
            saveData.activeQuests.Add(entry);
        }

        DailyQuestSaveSystem.SaveDailyQuestData(saveData);
    }

    #endregion

    #region Query Methods

    public DailyQuestProgress GetActiveDailyQuestProgress(DailyQuestSO quest)
    {
        return activeDailyQuests.FirstOrDefault(q => q.Quest == quest);
    }

    public DailyQuestProgress GetCompletedDailyQuestProgress(DailyQuestSO quest)
    {
        return completedDailyQuests.FirstOrDefault(q => q.Quest == quest);
    }

    public bool IsDailyQuestActive(DailyQuestSO quest)
    {
        return activeDailyQuests.Any(q => q.Quest == quest);
    }

    public bool IsDailyQuestCompleted(DailyQuestSO quest)
    {
        return completedDailyQuests.Any(q => q.Quest == quest);
    }

    public List<DailyQuestProgress> GetDailyQuestsByType(EDailyQuestType questType)
    {
        return activeDailyQuests.Where(q => q.Quest.QuestType == questType).ToList();
    }

    public float GetOverallDailyProgress()
    {
        if (activeDailyQuests.Count == 0) return 0f;
        return activeDailyQuests.Average(q => q.Progress);
    }

    #endregion

    #region Public Methods
    public void NotifyNormalQuestCompleted()
    {
        foreach (var questProgress in activeDailyQuests.ToList())
        {
            if (questProgress.Quest.QuestType == EDailyQuestType.CompleteNormalQuests)
            {
                questProgress.AddProgress(1);
            }
        }
    }

    #endregion
}