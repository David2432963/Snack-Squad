using System.Collections.Generic;
using System.Linq;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class Quest_Manager : MonoBehaviour
{
    [Title("Quest Configuration")]
    [SerializeField] private FoodCollectionQuest[] questDatas;
    [SerializeField] private int maxActiveQuests = 3;
    [SerializeField] private bool autoAssignQuests = true;

    [Title("Session Configuration")]
    [SerializeField, ReadOnly] private EFoodType sessionFoodType;
    [SerializeField, ReadOnly] private FoodCollectionQuest[] sessionQuests;
    [SerializeField, ReadOnly] private bool sessionInitialized = false;

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
        // Auto-load quests if array is empty
        if (questDatas == null || questDatas.Length == 0)
        {
            LoadQuestsFromAssets();
        }
        SelectRandomSessionFoodType();
    }

    private void SelectRandomSessionFoodType()
    {
        var foodTypes = Enum.GetValues(typeof(EFoodType));
        var data = (EFoodType)foodTypes.GetValue(UnityEngine.Random.Range(0, foodTypes.Length));
        InitializeWithSessionFoodType(data);
    }

    private void InitializeWithSessionFoodType(EFoodType foodType)
    {
        sessionFoodType = foodType;
        sessionInitialized = true;

        FilterQuestsForSession();

        if (autoAssignQuests)
        {
            AssignRandomQuests();
        }
    }

    private void FilterQuestsForSession()
    {
        if (questDatas == null || questDatas.Length == 0)
        {
            sessionQuests = new FoodCollectionQuest[0];
            return;
        }

        var filteredQuests = new List<FoodCollectionQuest>();
        foreach (var quest in questDatas)
        {
            if (quest.RequiredFoodType == sessionFoodType)
            {
                filteredQuests.Add(quest);
            }
        }

        sessionQuests = filteredQuests.ToArray();
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
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FoodCollectionQuest", new[] { searchPath });
        List<FoodCollectionQuest> foundQuests = new List<FoodCollectionQuest>();

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            FoodCollectionQuest quest = UnityEditor.AssetDatabase.LoadAssetAtPath<FoodCollectionQuest>(path);
            if (quest != null)
            {
                foundQuests.Add(quest);
            }
        }

        questDatas = foundQuests.ToArray();

        // Re-filter quests if session is already initialized
        if (sessionInitialized)
        {
            FilterQuestsForSession();
        }
#endif
    }

    private void AssignRandomQuests()
    {
        if (!sessionInitialized)
        {
            return;
        }

        if (sessionQuests == null || sessionQuests.Length == 0)
        {
            return;
        }

        while (activeQuests.Count < maxActiveQuests && sessionQuests.Length > 0)
        {
            var randomQuest = sessionQuests[UnityEngine.Random.Range(0, sessionQuests.Length)];
            bool added = AddQuest(randomQuest);

            // Break if we can't add any more quests to prevent infinite loop
            if (!added)
            {
                break;
            }
        }
    }

    private bool AddQuest(FoodCollectionQuest quest)
    {
        if (quest == null)
        {
            return false;
        }

        if (activeQuests.Count >= maxActiveQuests)
        {
            return false;
        }

        if (IsQuestActive(quest))
        {
            return false;
        }

        if (quest.SelectedSpecificItems.Count == 0)
        {
            quest.GenerateRandomSpecificItems();
        }

        var questProgress = new QuestProgress(quest);
        SubscribeToQuestEvents(questProgress);
        activeQuests.Add(questProgress);

        OnQuestAdded?.Invoke(questProgress);

        return true;
    }

    private bool RemoveQuest(FoodCollectionQuest quest)
    {
        var questProgress = GetActiveQuestProgress(quest);
        if (questProgress == null)
        {
            return false;
        }

        UnsubscribeFromQuestEvents(questProgress);
        activeQuests.Remove(questProgress);

        return true;
    }

    private void CompleteQuest(QuestProgress questProgress)
    {
        if (!activeQuests.Contains(questProgress)) return;

        UnsubscribeFromQuestEvents(questProgress);
        activeQuests.Remove(questProgress);
        completedQuests.Add(questProgress);

        OnQuestCompleted?.Invoke(questProgress);

        // Notify daily quest system about completed normal quest
        if (DailyQuest_Manager.Instance != null)
        {
            DailyQuest_Manager.Instance.NotifyNormalQuestCompleted();
        }

        // Auto-assign new quest if enabled - but only assign ONE quest to prevent cascading
        if (autoAssignQuests && activeQuests.Count < maxActiveQuests && sessionQuests.Length > 0)
        {
            var randomQuest = sessionQuests[UnityEngine.Random.Range(0, sessionQuests.Length)];
            AddQuest(randomQuest);
        }
    }

    private void ClaimQuestReward(FoodCollectionQuest quest)
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
        if (data is FoodCollectionData collectionData)
        {
            if (collectionData.playerType == EPlayerType.MainPlayer)
            {
                ProcessFoodCollection(collectionData.food);
            }
        }
        else if (data is GoodFood food)
        {
            ProcessFoodCollection(food);
        }
    }

    private void ProcessFoodCollection(GoodFood food)
    {
        EFoodType foodType = food.FoodType;

        foreach (var questProgress in activeQuests.ToList())
        {
            if (questProgress.Quest.RequiredFoodType == foodType)
            {
                int specificItemValue = GetSpecificItemValue(food);

                if (specificItemValue != -1)
                {
                    questProgress.AddSpecificItem(specificItemValue);
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
                    return (int)fruitFood.FruitType;
                }
                break;
            case EFoodType.FastFood:
                if (food is FastFood fastFood)
                {
                    return (int)fastFood.FastFoodType;
                }
                break;
            case EFoodType.Cake:
                break;
        }
        return -1;
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