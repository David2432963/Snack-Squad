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
        SingletonManager.Instance.RegisterScene(this);
        Main.Observer.Add(EEvent.OnPlayerCollectFood, OnGoodFoodCollected);
        Main.Observer.Add(EEvent.OnQuestCompleted, OnNormalQuestCompleted);
        
        // Initialize daily progress tracking
        GameData.CheckAndResetDailyProgress();
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
        Main.Observer.Remove(EEvent.OnPlayerCollectFood, OnGoodFoodCollected);
        Main.Observer.Remove(EEvent.OnQuestCompleted, OnNormalQuestCompleted);

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
        // Check if we have valid save data from today
        bool hasTodaysData = DailyQuestSaveSystem.HasTodaysSaveData();
        
        if (!hasTodaysData)
        {
            // No today's data - generate new daily quests
            ClearAllQuests();
            if (autoAssignDailyQuests)
            {
                AssignRandomDailyQuests();
            }
        }
        else if (activeDailyQuests.Count == 0)
        {
            // We have today's data but no active quests loaded - this shouldn't happen
            // But if it does, generate new quests
            if (autoAssignDailyQuests)
            {
                AssignRandomDailyQuests();
            }
        }

        // Update last login date
        lastLoginDate = DateTime.Now.Date;
    }

    private void ClearAllQuests()
    {
        // Clear active quests
        foreach (var quest in activeDailyQuests.ToList())
        {
            UnsubscribeFromDailyQuestEvents(quest);
        }
        activeDailyQuests.Clear();

        // Clear completed quests
        foreach (var quest in completedDailyQuests.ToList())
        {
            UnsubscribeFromDailyQuestEvents(quest);
        }
        completedDailyQuests.Clear();
    }

    private void CompleteDailyQuest(DailyQuestProgress questProgress)
    {
        if (!activeDailyQuests.Contains(questProgress)) return;

        UnsubscribeFromDailyQuestEvents(questProgress);
        activeDailyQuests.Remove(questProgress);
        completedDailyQuests.Add(questProgress);

        Main.Observer.Notify(EEvent.OnDailyQuestCompleted, questProgress);
    }

    private void AssignRandomDailyQuests()
    {
        // Clear any existing active quests - use ClearAllQuests to properly clean up
        ClearAllQuests();

        // Create the 2 types of daily quests
        CreateCompleteQuestsDaily();
        
        CreateCollectSpecificFoodDaily();
    }

    private void CreateCompleteQuestsDaily()
    {
        var quest = CreateCompleteQuestsQuestSO();
        AddDailyQuest(quest);
    }

    private void CreateCollectSpecificFoodDaily()
    {
        // Create a "Collect 10 specific foods" daily quest
        var collectFoodDaily = ScriptableObject.CreateInstance<DailyQuestSO>();
        collectFoodDaily.name = "Collect Specific Food Daily";

        // Randomly choose a food type and then a specific item within that type
        var foodTypes = System.Enum.GetValues(typeof(EFoodType));
        EFoodType randomFoodType = (EFoodType)foodTypes.GetValue(UnityEngine.Random.Range(0, foodTypes.Length));

        string foodName = "";

        // Set specific food type based on category
        switch (randomFoodType)
        {
            case EFoodType.Fruit:
                var fruitTypes = System.Enum.GetValues(typeof(EFruitType));
                EFruitType randomFruit = (EFruitType)fruitTypes.GetValue(UnityEngine.Random.Range(0, fruitTypes.Length));
                foodName = randomFruit.ToString();
                SetCollectFoodQuestFields(collectFoodDaily, foodName, randomFoodType, randomFruit, default, default);
                break;
            case EFoodType.FastFood:
                var fastFoodTypes = System.Enum.GetValues(typeof(EFastFoodType));
                EFastFoodType randomFastFood = (EFastFoodType)fastFoodTypes.GetValue(UnityEngine.Random.Range(0, fastFoodTypes.Length));
                foodName = randomFastFood.ToString();
                SetCollectFoodQuestFields(collectFoodDaily, foodName, randomFoodType, default, randomFastFood, default);
                break;
            case EFoodType.Cake:
                var cakeTypes = System.Enum.GetValues(typeof(ECakeType));
                ECakeType randomCake = (ECakeType)cakeTypes.GetValue(UnityEngine.Random.Range(0, cakeTypes.Length));
                foodName = randomCake.ToString();
                SetCollectFoodQuestFields(collectFoodDaily, foodName, randomFoodType, default, default, randomCake);
                break;
        }

        AddDailyQuest(collectFoodDaily);
    }

    private bool AddDailyQuest(DailyQuestSO quest)
    {
        if (quest == null)
        {
            return false;
        }

        if (activeDailyQuests.Count >= maxDailyQuests)
        {
            return false;
        }

        if (IsDailyQuestActive(quest))
        {
            return false;
        }

        var questProgress = new DailyQuestProgress(quest);
        SubscribeToDailyQuestEvents(questProgress);
        activeDailyQuests.Add(questProgress);

        Main.Observer.Notify(EEvent.OnDailyQuestAdded, questProgress);
        
        // Auto-save when quest is added
        SaveDailyQuestData();

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

    private void OnNormalQuestCompleted(object data)
    {
        // This gets called when a normal quest (not daily quest) is completed
        // Refresh daily quest progress for "Complete Normal Quests" type from GameData
        foreach (var questProgress in activeDailyQuests.ToList())
        {
            if (questProgress.Quest.QuestType == EDailyQuestType.CompleteNormalQuests)
            {
                questProgress.RefreshProgressFromGameData();
            }
        }
    }

    private void ProcessSpecificFoodCollection(GoodFood food)
    {
        // Refresh progress from GameData for all CollectSpecificFood quests
        foreach (var questProgress in activeDailyQuests.ToList())
        {
            if (questProgress.Quest.QuestType == EDailyQuestType.CollectSpecificFood)
            {
                questProgress.RefreshProgressFromGameData();
            }
        }
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
        
        // Auto-save when quest progress changes
        SaveDailyQuestData();
    }

    private void HandleDailyQuestCompleted(DailyQuestProgress questProgress)
    {
        CompleteDailyQuest(questProgress);
        
        // Auto-save when quest is completed
        SaveDailyQuestData();
    }

    private void HandleDailyQuestRewardClaimed(DailyQuestProgress questProgress)
    {
        Main.Observer.Notify(EEvent.OnDailyQuestRewardClaimed, questProgress);
        
        // Auto-save when reward is claimed
        SaveDailyQuestData();
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

        // Load today's quests only if data is from today
        if (saveData.IsFromToday())
        {
            LoadTodaysQuests(saveData.todaysQuests);
        }
    }

    private void LoadTodaysQuests(List<DailyQuestSaveEntry> questEntries)
    {
        foreach (var entry in questEntries)
        {
            // Only process quests from today
            if (!entry.IsFromToday())
            {
                continue;
            }
            
            // Recreate the quest from save data
            var quest = RecreateQuestFromSaveEntry(entry);
            
            if (quest != null)
            {
                var questProgress = new DailyQuestProgress(quest, entry.currentProgress,
                    entry.isCompleted, entry.isRewardClaimed, entry.GetQuestDate());

                SubscribeToDailyQuestEvents(questProgress);

                if (questProgress.IsCompleted)
                {
                    completedDailyQuests.Add(questProgress);
                }
                else
                {
                    activeDailyQuests.Add(questProgress);
                }
            }
        }
    }

    private DailyQuestSO RecreateQuestFromSaveEntry(DailyQuestSaveEntry entry)
    {
        if (entry.questType == EDailyQuestType.CompleteNormalQuests)
        {
            return CreateCompleteQuestsQuestSO();
        }
        else if (entry.questType == EDailyQuestType.CollectSpecificFood)
        {
            return CreateCollectFoodQuestSO(entry.requiredFoodType, entry.requiredFruitType, 
                entry.requiredFastFoodType, entry.requiredCakeType, entry.targetValue, entry.goldReward);
        }
        
        return null;
    }

    private DailyQuestSO CreateCollectFoodQuestSO(EFoodType foodType, EFruitType fruitType, 
        EFastFoodType fastFoodType, ECakeType cakeType, int targetValue, int goldReward)
    {
        var collectFoodQuest = ScriptableObject.CreateInstance<DailyQuestSO>();
        collectFoodQuest.name = "Collect Specific Food Daily";

        string foodName = "";
        switch (foodType)
        {
            case EFoodType.Fruit:
                foodName = fruitType.ToString();
                SetCollectFoodQuestFields(collectFoodQuest, foodName, foodType, fruitType, default, default);
                break;
            case EFoodType.FastFood:
                foodName = fastFoodType.ToString();
                SetCollectFoodQuestFields(collectFoodQuest, foodName, foodType, default, fastFoodType, default);
                break;
            case EFoodType.Cake:
                foodName = cakeType.ToString();
                SetCollectFoodQuestFields(collectFoodQuest, foodName, foodType, default, default, cakeType);
                break;
        }

        // Override target value and gold reward with saved values
        var targetValueField = typeof(DailyQuestSO).GetField("targetValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var goldRewardField = typeof(DailyQuestSO).GetField("goldReward", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        targetValueField?.SetValue(collectFoodQuest, targetValue);
        goldRewardField?.SetValue(collectFoodQuest, goldReward);

        return collectFoodQuest;
    }

    private DailyQuestSO RecreateQuestFromSaveData(DailyQuestSaveEntry entry)
    {
        // Recreate dynamic quests based on their names/types
        if (entry.questName.Contains("Quest Completionist") || entry.questName.Contains("Complete Normal Quests"))
        {
            return CreateCompleteQuestsQuestSO();
        }
        else if (entry.questName.Contains("Collector"))
        {
            // Try to extract the food type from the quest name
            string foodName = entry.questName.Replace(" Collector", "");
            return CreateCollectSpecificFoodQuestSO(foodName);
        }
        
        return null;
    }

    private DailyQuestSO CreateCompleteQuestsQuestSO()
    {
        var completeQuestsDaily = ScriptableObject.CreateInstance<DailyQuestSO>();
        completeQuestsDaily.name = "Complete Normal Quests Daily";

        // Use reflection to set private fields
        SetQuestFields(completeQuestsDaily, "Quest Completionist", 
            "Complete 2 normal quests (collect 3 required foods each)", 
            EDailyQuestType.CompleteNormalQuests, 2, 100);

        return completeQuestsDaily;
    }

    private DailyQuestSO CreateCollectSpecificFoodQuestSO(string foodName)
    {
        var collectFoodDaily = ScriptableObject.CreateInstance<DailyQuestSO>();
        collectFoodDaily.name = "Collect Specific Food Daily";

        // Try to determine food type from name
        EFoodType foodType;
        if (System.Enum.TryParse(foodName, out EFruitType fruitType))
        {
            foodType = EFoodType.Fruit;
            SetCollectFoodQuestFields(collectFoodDaily, foodName, foodType, fruitType, default, default);
        }
        else if (System.Enum.TryParse(foodName, out EFastFoodType fastFoodType))
        {
            foodType = EFoodType.FastFood;
            SetCollectFoodQuestFields(collectFoodDaily, foodName, foodType, default, fastFoodType, default);
        }
        else if (System.Enum.TryParse(foodName, out ECakeType cakeType))
        {
            foodType = EFoodType.Cake;
            SetCollectFoodQuestFields(collectFoodDaily, foodName, foodType, default, default, cakeType);
        }
        else
        {
            // Default to random if can't parse
            return null;
        }

        return collectFoodDaily;
    }

    private void SetQuestFields(DailyQuestSO quest, string name, string description, EDailyQuestType questType, int targetValue, int goldReward)
    {
        var questNameField = typeof(DailyQuestSO).GetField("questName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var questDescField = typeof(DailyQuestSO).GetField("questDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var questTypeField = typeof(DailyQuestSO).GetField("questType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var targetValueField = typeof(DailyQuestSO).GetField("targetValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var goldRewardField = typeof(DailyQuestSO).GetField("goldReward", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        questNameField?.SetValue(quest, name);
        questDescField?.SetValue(quest, description);
        questTypeField?.SetValue(quest, questType);
        targetValueField?.SetValue(quest, targetValue);
        goldRewardField?.SetValue(quest, goldReward);
    }

    private void SetCollectFoodQuestFields(DailyQuestSO quest, string foodName, EFoodType foodType, EFruitType fruitType, EFastFoodType fastFoodType, ECakeType cakeType)
    {
        SetQuestFields(quest, $"{foodName} Collector", $"Collect 10 {foodName} items", EDailyQuestType.CollectSpecificFood, 10, 50);
        
        var foodTypeField = typeof(DailyQuestSO).GetField("requiredFoodType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foodTypeField?.SetValue(quest, foodType);

        switch (foodType)
        {
            case EFoodType.Fruit:
                var fruitField = typeof(DailyQuestSO).GetField("requiredFruitType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fruitField?.SetValue(quest, fruitType);
                break;
            case EFoodType.FastFood:
                var fastFoodField = typeof(DailyQuestSO).GetField("requiredFastFoodType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fastFoodField?.SetValue(quest, fastFoodType);
                break;
            case EFoodType.Cake:
                var cakeField = typeof(DailyQuestSO).GetField("requiredCakeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cakeField?.SetValue(quest, cakeType);
                break;
        }
    }

    private void SaveDailyQuestData()
    {
        if (saveData == null)
            saveData = new DailyQuestSaveData();

        // Clear previous data
        saveData.todaysQuests.Clear();

        // Save all active quests
        foreach (var quest in activeDailyQuests)
        {
            var entry = new DailyQuestSaveEntry(quest);
            saveData.todaysQuests.Add(entry);
        }

        // Save completed quests from today
        foreach (var quest in completedDailyQuests)
        {
            var entry = new DailyQuestSaveEntry(quest);
            saveData.todaysQuests.Add(entry);
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
}