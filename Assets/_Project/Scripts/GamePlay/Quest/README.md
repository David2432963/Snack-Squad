# Quest System Documentation

## Overview

The Quest System allows players to collect specific amounts of food types (EFoodType) to complete quests. Quests are configurable ScriptableObjects that can be easily created and modified in the Unity Editor.

## System Components

### 1. FoodCollectionQuest (ScriptableObject)
The main quest definition that stores:
- Quest name and description
- Required food type (EFoodType)
- Target amount to collect
- Score rewards
- Quest icon

**Creating a Quest:**
1. Right-click in Project window
2. Create > Quest System > Food Collection Quest
3. Configure the quest settings
4. Add to Quest_Manager's Available Quests array

### 2. QuestProgress (Class)
Tracks individual quest progress including:
- Current collection amount
- Completion status
- Reward claim status
- Start and completion times

### 3. Quest_Manager (MonoBehaviour)
The main manager that handles:
- Active quest tracking
- Progress monitoring
- Event listening for food collection
- Quest completion and rewards
- Auto-assignment of new quests

### 4. QuestUI & QuestItem (UI Components)
Provides visual feedback for quest progress:
- Displays active quests
- Shows progress bars
- Handles reward claiming
- Updates in real-time

## Setup Instructions

### 1. Quest Manager Setup
1. Add `Quest_Manager` component to a GameObject in your scene
2. Create quest ScriptableObjects (see step 2)
3. Assign quests to the `Available Quests` array
4. Configure `Max Active Quests` (default: 3)
5. Enable `Auto Assign Quests` if desired

### 2. Creating Quest Assets
```csharp
// Example quest configuration:
Quest Name: "Collect Apples"
Quest Description: "Collect 10 fruits to complete this quest"
Required Food Type: Fruit
Target Amount: 10
Score Reward: 100
Bonus Score: 50
```

### 3. UI Setup (Optional)
1. Create a Canvas for quest display
2. Add `QuestUI` component
3. Create a quest item prefab with `QuestItem` component
4. Assign references in QuestUI inspector

## Food Collection Integration

The system automatically tracks food collection through the existing `Food` class:
- When food is collected, `EEvent.OnFoodCollected` is triggered
- Quest_Manager listens for this event
- Progress is updated for matching quest food types

## API Usage

### Basic Quest Management
```csharp
// Get the quest manager instance
Quest_Manager questManager = Quest_Manager.Instance;

// Add a specific quest
questManager.AddQuest(myFoodCollectionQuest);

// Remove a quest
questManager.RemoveQuest(myFoodCollectionQuest);

// Check if quest is active
bool isActive = questManager.IsQuestActive(myFoodCollectionQuest);

// Get quest progress
QuestProgress progress = questManager.GetActiveQuestProgress(myFoodCollectionQuest);
```

### Quest Progress Monitoring
```csharp
// Subscribe to quest events
questManager.OnQuestAdded += OnQuestAdded;
questManager.OnQuestCompleted += OnQuestCompleted;
questManager.OnQuestProgressUpdated += OnProgressUpdated;

// Manual progress update (if needed)
questProgress.AddProgress(5); // Add 5 to current amount
questProgress.SetProgress(10); // Set to specific amount
```

### Reward Management
```csharp
// Claim quest reward
questManager.ClaimQuestReward(myFoodCollectionQuest);

// Check reward status
bool canClaim = questProgress.IsCompleted && !questProgress.IsRewardClaimed;
```

## Events

### Quest Manager Events
- `OnQuestAdded`: Fired when a new quest is added
- `OnQuestProgressUpdated`: Fired when quest progress changes
- `OnQuestCompleted`: Fired when a quest is completed
- `OnQuestRewardClaimed`: Fired when rewards are claimed

### Quest Progress Events
- `OnProgressUpdated`: Individual quest progress changed
- `OnQuestCompleted`: Individual quest completed
- `OnRewardClaimed`: Individual quest reward claimed

## Debug Features

The Quest_Manager includes several debug buttons in the inspector:
- `Assign Random Quests`: Assigns random quests up to max limit
- `Complete All Active Quests`: Instantly completes all active quests
- `Reset All Quests`: Clears all quest progress
- `Print Quest Status`: Logs current quest status to console

## Food Types Supported

Based on the existing `EFoodType` enum:
- `Fruit`: General fruit category
- `FastFood`: Fast food items
- `Cake`: Cake items

Specific sub-types (EFruitType, EFastFoodType) are also supported through the existing food hierarchy.

## Integration Notes

### Existing Systems Integration
- Uses existing `SingletonManager` pattern
- Integrates with `ObserverManager` for events
- Works with existing `GameData_Manager` for score rewards
- Compatible with current food collection system

### Performance Considerations
- Quest progress updates are event-driven
- UI updates can be throttled via refresh interval
- Memory usage scales linearly with number of active quests

## Troubleshooting

### Common Issues
1. **Quests not tracking progress**: Ensure food objects have the correct `EFoodType` set
2. **UI not updating**: Check that QuestUI is properly subscribed to events
3. **Events not firing**: Verify Observer system is properly initialized

### Debug Tips
- Use the "Print Quest Status" button to check current state
- Check console logs for quest completion messages
- Ensure Quest_Manager is initialized before food collection begins

## Example Quest Configurations

### Easy Fruit Collection
- Target: 5 Fruits
- Reward: 50 points
- Description: "Collect your first fruits!"

### Medium FastFood Challenge
- Target: 15 FastFood items
- Reward: 150 points + 25 bonus
- Description: "Gather fast food for a quick meal"

### Hard Mixed Collection
- Target: 25 of any food type
- Reward: 300 points + 100 bonus
- Description: "Master collector achievement"

## Future Enhancements

Possible extensions to the system:
- Time-limited quests
- Multi-objective quests
- Quest chains/dependencies
- Different reward types (power-ups, lives, etc.)
- Quest difficulty scaling
- Player preference-based quest assignment