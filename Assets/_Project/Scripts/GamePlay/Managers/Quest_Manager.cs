using System.Collections.Generic;
using System.Linq;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class Quest_Manager : MonoBehaviour
{
    [Title("Quest Configuration")]
    [SerializeField] private FoodCollectionQuestSO[] questDatas;
    [SerializeField] private int maxActiveQuests = 3;
    [SerializeField] private bool autoAssignQuests = true;

    [Title("Session Configuration")]
    [SerializeField, ReadOnly] private EFoodType sessionFoodType;
    [SerializeField, ReadOnly] private FoodCollectionQuestSO[] sessionQuests;
    [SerializeField, ReadOnly] private bool sessionInitialized = false;

    [Title("Debug Info")]
    [SerializeField, ReadOnly] private List<QuestProgress> activeQuests = new List<QuestProgress>();
    [SerializeField, ReadOnly] private List<QuestProgress> completedQuests = new List<QuestProgress>();

    // Properties
    public static Quest_Manager Instance => SingletonManager.Instance.Get<Quest_Manager>();
    public IReadOnlyList<QuestProgress> ActiveQuests => activeQuests.AsReadOnly();
    public IReadOnlyList<QuestProgress> CompletedQuests => completedQuests.AsReadOnly();
    public int ActiveQuestCount => activeQuests.Count;
    public int CompletedQuestCount => completedQuests.Count;


    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
        Main.Observer.Add(EEvent.OnGoodFoodCollected, OnGoodFoodCollected);
        Main.Observer.Add(EEvent.OnSessionFoodTypeSelected, OnSessionFoodTypeSelected);

        if (questDatas == null || questDatas.Length == 0)
        {
            LoadQuestsFromAssets();
        }
    }

    private void OnSessionFoodTypeSelected(object data)
    {
        if (data is EFoodType foodType)
        {
            InitializeWithSessionFoodType(foodType);
        }
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
            sessionQuests = new FoodCollectionQuestSO[0];
            return;
        }

        var filteredQuests = new List<FoodCollectionQuestSO>();
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
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FoodCollectionQuestSO", new[] { searchPath });
        List<FoodCollectionQuestSO> foundQuests = new List<FoodCollectionQuestSO>();

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            FoodCollectionQuestSO quest = UnityEditor.AssetDatabase.LoadAssetAtPath<FoodCollectionQuestSO>(path);
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

    private bool AddQuest(FoodCollectionQuestSO quest)
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

        Main.Observer.Notify(EEvent.OnQuestAdded, questProgress);

        return true;
    }

    private bool RemoveQuest(FoodCollectionQuestSO quest)
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

        Main.Observer.Notify(EEvent.OnQuestCompleted, questProgress);

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

    private void ClaimQuestReward(FoodCollectionQuestSO quest)
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
            if (collectionData.playerType == EPlayerType.Player)
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
                if (food is Cake cakeFood)
                {
                    return (int)cakeFood.CakeType;
                }
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
        Main.Observer.Notify(EEvent.OnQuestProgressUpdated, questProgress);
    }

    private void HandleQuestCompleted(QuestProgress questProgress)
    {
        CompleteQuest(questProgress);
    }

    private void HandleQuestRewardClaimed(QuestProgress questProgress)
    {
        Main.Observer.Notify(EEvent.OnQuestRewardClaimed, questProgress);
    }

    #endregion

    #region Query Methods

    public QuestProgress GetActiveQuestProgress(FoodCollectionQuestSO quest)
    {
        return activeQuests.FirstOrDefault(q => q.Quest == quest);
    }

    public QuestProgress GetCompletedQuestProgress(FoodCollectionQuestSO quest)
    {
        return completedQuests.FirstOrDefault(q => q.Quest == quest);
    }

    public bool IsQuestActive(FoodCollectionQuestSO quest)
    {
        return activeQuests.Any(q => q.Quest == quest);
    }

    public bool IsQuestCompleted(FoodCollectionQuestSO quest)
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