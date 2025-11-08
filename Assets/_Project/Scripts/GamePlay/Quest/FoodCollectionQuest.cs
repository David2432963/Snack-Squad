using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Food Collection Quest", menuName = "Quest System/Food Collection Quest")]
public class FoodCollectionQuest : ScriptableObject
{
    [Title("Quest Information")]
    [SerializeField] private string questName;
    [SerializeField, TextArea(2, 4)] private string questDescription;
    [SerializeField] private Sprite questIcon;
    
    [Title("Random Item Collection")]
    [SerializeField] private EFoodType foodType = EFoodType.Fruit;
    [SerializeField, Range(2, 6)] private int itemCount = 3;
    [SerializeField, ReadOnly] private List<int> selectedSpecificItems = new List<int>();

    [Title("Quest Rewards")]
    [SerializeField] private int scoreReward;
    [SerializeField] private int bonusScore;

    // Properties
    public string QuestName => questName;
    public string QuestDescription => questDescription;
    public Sprite QuestIcon => questIcon;
    public EFoodType RequiredFoodType => foodType;
    public int TargetAmount => selectedSpecificItems.Count > 0 ? selectedSpecificItems.Count : itemCount;
    public int ScoreReward => scoreReward;
    public int BonusScore => bonusScore;
    public bool UseMultiItemCollection => true; // Always true for simplified system
    public int RandomItemCount => itemCount;
    public IReadOnlyList<int> SelectedSpecificItems => selectedSpecificItems.AsReadOnly();

    // Methods for multi-item collection
    public void GenerateRandomSpecificItems()
    {
        selectedSpecificItems.Clear();
        
        // Get available items based on food type
        List<int> availableItems = GetAvailableItemsByFoodType(foodType);
        
        if (availableItems.Count < itemCount)
        {
            Debug.LogWarning($"Not enough available items for {foodType}. Available: {availableItems.Count}, Required: {itemCount}");
            itemCount = availableItems.Count;
        }

        // Randomly select items
        var shuffledItems = availableItems.OrderBy(x => UnityEngine.Random.value).Take(itemCount).ToList();
        selectedSpecificItems.AddRange(shuffledItems);
        
        // Update quest name and description to reflect the specific items
        UpdateQuestTextForSpecificItems();
    }

    private List<int> GetAvailableItemsByFoodType(EFoodType foodType)
    {
        List<int> items = new List<int>();
        
        switch (foodType)
        {
            case EFoodType.Fruit:
                items.AddRange(System.Enum.GetValues(typeof(EFruitType)).Cast<int>());
                break;
            case EFoodType.FastFood:
                items.AddRange(System.Enum.GetValues(typeof(EFastFoodType)).Cast<int>());
                break;
            case EFoodType.Cake:
                items.AddRange(System.Enum.GetValues(typeof(ECakeType)).Cast<int>());
                break;
        }
        
        return items;
    }

    private void UpdateQuestTextForSpecificItems()
    {
        if (selectedSpecificItems.Count == 0) return;

        string itemNames = "";
        for (int i = 0; i < selectedSpecificItems.Count; i++)
        {
            string itemName = GetItemName(foodType, selectedSpecificItems[i]);
            itemNames += itemName;
            
            if (i < selectedSpecificItems.Count - 1)
            {
                itemNames += (i == selectedSpecificItems.Count - 2) ? " and " : ", ";
            }
        }

        questDescription = $"Collect {itemNames}";
    }

    private string GetItemName(EFoodType foodType, int itemValue)
    {
        switch (foodType)
        {
            case EFoodType.Fruit:
                return ((EFruitType)itemValue).ToString();
            case EFoodType.FastFood:
                return ((EFastFoodType)itemValue).ToString();
            case EFoodType.Cake:
                return ((ECakeType)itemValue).ToString();
            default:
                return "Unknown Item";
        }
    }

    public bool IsSpecificItemRequired(int itemValue)
    {
        return selectedSpecificItems.Contains(itemValue);
    }

    public List<string> GetRequiredItemNames()
    {
        List<string> names = new List<string>();
        foreach (int itemValue in selectedSpecificItems)
        {
            names.Add(GetItemName(foodType, itemValue));
        }
        return names;
    }

    [Button("Generate Random Items")]
    private void DebugGenerateRandomItems()
    {
        GenerateRandomSpecificItems();
        Debug.Log($"Generated random items for {questName}: {string.Join(", ", GetRequiredItemNames())}");
    }

    [Button("Validate Quest")]
    private void ValidateQuest()
    {
        if (string.IsNullOrEmpty(questName))
        {
            Debug.LogWarning("Quest name is empty!");
        }

        if (itemCount <= 0)
        {
            Debug.LogWarning("Item count must be greater than 0!");
        }

        if (scoreReward < 0)
        {
            Debug.LogWarning("Score reward should not be negative!");
        }

        if (selectedSpecificItems.Count == 0)
        {
            Debug.LogWarning("No specific items selected. Use 'Generate Random Items' button.");
        }
    }

    private void OnValidate()
    {
        // Ensure item count is reasonable
        if (itemCount < 2)
            itemCount = 2;
        if (itemCount > 6)
            itemCount = 6;

        // Ensure rewards are non-negative
        if (scoreReward < 0)
            scoreReward = 0;

        if (bonusScore < 0)
            bonusScore = 0;
    }
}