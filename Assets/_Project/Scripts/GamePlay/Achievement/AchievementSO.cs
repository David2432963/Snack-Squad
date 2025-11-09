using UnityEngine;
using Sirenix.OdinInspector;
using System;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement System/Achievement")]
public class AchievementSO : ScriptableObject
{
    [Title("Achievement Information")]
    [SerializeField] private string achievementId;
    [SerializeField] private string achievementName;
    [SerializeField, TextArea(2, 4)] private string description;
    [SerializeField] private Sprite icon;

    [Title("Achievement Requirements")]
    [SerializeField] private EAchievementType achievementType;
    [SerializeField] private int targetValue;
    [SerializeField, ShowIf("@achievementType == EAchievementType.CollectSpecificFood")] 
    private EFoodType requiredFoodType = EFoodType.Fruit;
    [SerializeField, ShowIf("@achievementType == EAchievementType.CollectSpecificFood && requiredFoodType == EFoodType.Fruit")] 
    private EFruitType requiredFruitType = EFruitType.Apple;
    [SerializeField, ShowIf("@achievementType == EAchievementType.CollectSpecificFood && requiredFoodType == EFoodType.FastFood")] 
    private EFastFoodType requiredFastFoodType = EFastFoodType.Baccon;
    [SerializeField, ShowIf("@achievementType == EAchievementType.CollectSpecificFood && requiredFoodType == EFoodType.Cake")] 
    private ECakeType requiredCakeType = ECakeType.Berry;

    [Title("Achievement Rewards")]
    [SerializeField] private int goldReward;
    [SerializeField] private bool isHidden; // Hidden until unlocked

    // Properties
    public string AchievementId => achievementId;
    public string AchievementName => achievementName;
    public string Description => description;
    public Sprite Icon => icon;
    public EAchievementType AchievementType => achievementType;
    public int TargetValue => targetValue;
    public EFoodType RequiredFoodType => requiredFoodType;
    public EFruitType RequiredFruitType => requiredFruitType;
    public EFastFoodType RequiredFastFoodType => requiredFastFoodType;
    public ECakeType RequiredCakeType => requiredCakeType;
    public int GoldReward => goldReward;
    public bool IsHidden => isHidden;

    private void OnValidate()
    {
        // Ensure ID is set
        if (string.IsNullOrEmpty(achievementId))
        {
            achievementId = name.Replace(" ", "").Replace("(", "").Replace(")", "");
        }

        // Ensure values are reasonable
        if (targetValue < 1)
            targetValue = 1;

        if (goldReward < 0)
            goldReward = 0;
    }

    [Button("Generate Description")]
    private void GenerateDescription()
    {
        switch (achievementType)
        {
            case EAchievementType.CollectSpecificFood:
                string foodName = GetSpecificFoodName();
                description = $"Collect {targetValue} {foodName} items";
                achievementName = $"{foodName} Master";
                break;
            case EAchievementType.CompleteQuests:
                description = $"Complete {targetValue} quests";
                achievementName = $"Quest Hero {targetValue}";
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
public enum EAchievementType
{
    CollectSpecificFood,   // Collect X specific food items
    CompleteQuests,        // Complete X quests
}