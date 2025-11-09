using UnityEngine;
using Sirenix.OdinInspector;
using System;

[CreateAssetMenu(fileName = "New Daily Quest", menuName = "Quest System/Daily Quest")]
public class DailyQuestSO : ScriptableObject
{
    [Title("Daily Quest Information")]
    [SerializeField] private string questName;
    [SerializeField, TextArea(2, 4)] private string questDescription;

    [Title("Quest Requirements")]
    [SerializeField] private EDailyQuestType questType;
    [SerializeField] private int targetValue;
    [SerializeField] private EFoodType requiredFoodType = EFoodType.Fruit; // For CollectSpecificFood quest
    [SerializeField, ShowIf("@requiredFoodType == EFoodType.Fruit")] private EFruitType requiredFruitType = EFruitType.Apple; // When requiredFoodType is Fruit
    [SerializeField, ShowIf("@requiredFoodType == EFoodType.FastFood")] private EFastFoodType requiredFastFoodType = EFastFoodType.Baccon; // When requiredFoodType is FastFood
    [SerializeField, ShowIf("@requiredFoodType == EFoodType.Cake")] private ECakeType requiredCakeType = ECakeType.Berry; // When requiredFoodType is Cake

    [Title("Quest Rewards")]
    [SerializeField] private int goldReward;

    // Properties
    public string QuestName => questName;
    public string QuestDescription => questDescription;
    public EDailyQuestType QuestType => questType;
    public int TargetValue => targetValue;
    public EFoodType RequiredFoodType => requiredFoodType;
    public EFruitType RequiredFruitType => requiredFruitType;
    public EFastFoodType RequiredFastFoodType => requiredFastFoodType;
    public ECakeType RequiredCakeType => requiredCakeType;
    public int GoldReward => goldReward;

    private void OnValidate()
    {
        // Ensure values are reasonable
        if (targetValue < 1)
            targetValue = 1;

        if (goldReward < 0)
            goldReward = 0;
    }

    [Button("Generate Quest Description")]
    private void GenerateQuestDescription()
    {
        switch (questType)
        {
            case EDailyQuestType.CompleteNormalQuests:
                questDescription = $"Complete {targetValue} normal quests (collect 3 required foods each)";
                questName = "Quest Completionist";
                break;
            case EDailyQuestType.CollectSpecificFood:
                string specificFoodName = GetSpecificFoodName();
                questDescription = $"Collect {targetValue} {specificFoodName} items";
                questName = $"{specificFoodName} Collector";
                break;
        }
    }

    private string GetSpecificFoodName()
    {
        switch (requiredFoodType)
        {
            case EFoodType.Fruit:
                return requiredFruitType.ToString();
            case EFoodType.FastFood:
                return requiredFastFoodType.ToString();
            case EFoodType.Cake:
                return requiredCakeType.ToString();
            default:
                return "Unknown Food";
        }
    }
}

[System.Serializable]
public enum EDailyQuestType
{
    CompleteNormalQuests,   // Complete 2 normal quests (collect 3 required foods)
    CollectSpecificFood     // Collect 10 foods of a random specific food in EFood
}