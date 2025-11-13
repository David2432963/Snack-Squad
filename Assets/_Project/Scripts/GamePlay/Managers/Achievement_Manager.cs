using System.Collections.Generic;
using System.Linq;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class Achievement_Manager : MonoBehaviour
{
    [Title("Achievement Configuration")]
    [SerializeField] private AchievementSO[] availableAchievements;

    [Title("Debug Info")]
    [SerializeField, ReadOnly] private List<AchievementProgress> allAchievements = new List<AchievementProgress>();

    // Properties
    public static Achievement_Manager Instance => SingletonManager.Instance.Get<Achievement_Manager>();
    public IReadOnlyList<AchievementProgress> AllAchievements => allAchievements.AsReadOnly();
    public IReadOnlyList<AchievementProgress> UnlockedAchievements => 
        allAchievements.Where(a => a.IsUnlocked).ToList().AsReadOnly();
    public IReadOnlyList<AchievementProgress> LockedAchievements => 
        allAchievements.Where(a => !a.IsUnlocked).ToList().AsReadOnly();

    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }

    private void Start()
    {
        // Load achievements from assets if needed
        if (availableAchievements == null || availableAchievements.Length == 0)
        {
            LoadAchievementsFromAssets();
        }

        // Initialize achievement system
        InitializeAchievements();
        LoadAchievementProgress();
        
        // Update achievement progress from GameData
        UpdateAchievementProgressFromGameData();
    }

    private void OnDestroy()
    {
        SaveAchievementProgress();
    }

    #region Achievement Management

    [Button("Load Achievements from Assets")]
    private void LoadAchievementsFromAssets()
    {
#if UNITY_EDITOR
        string searchPath = "Assets/_Project/Datas/AchievementData";
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AchievementSO", new[] { searchPath });
        List<AchievementSO> foundAchievements = new List<AchievementSO>();

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            AchievementSO achievement = UnityEditor.AssetDatabase.LoadAssetAtPath<AchievementSO>(path);
            if (achievement != null)
            {
                foundAchievements.Add(achievement);
            }
        }

        availableAchievements = foundAchievements.ToArray();
#endif
    }

    private void InitializeAchievements()
    {
        allAchievements.Clear();

        foreach (var achievementSO in availableAchievements)
        {
            var progress = new AchievementProgress(achievementSO.AchievementId);
            progress.SetAchievementSO(achievementSO);
            
            // Subscribe to achievement events
            progress.OnProgressUpdated += HandleAchievementProgressUpdated;
            progress.OnAchievementUnlocked += HandleAchievementUnlocked;
            progress.OnRewardClaimed += HandleAchievementRewardClaimed;

            allAchievements.Add(progress);
        }
    }

    private void UpdateAchievementProgressFromGameData()
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.IsUnlocked) continue;

            switch (achievement.Achievement.AchievementType)
            {
                case EAchievementType.CollectSpecificFood:
                    UpdateSpecificFoodAchievement(achievement);
                    break;
                    
                case EAchievementType.CompleteQuests:
                    achievement.SetProgress(GameData.TotalQuestsCompleted);
                    break;
            }
        }
    }

    private void UpdateSpecificFoodAchievement(AchievementProgress achievement)
    {
        var achievementSO = achievement.Achievement;
        int collected = 0;

        switch (achievementSO.RequiredFoodType)
        {
            case EFoodType.Fruit:
                collected = GameData.GetFoodCollected(EFoodType.Fruit, achievementSO.RequiredFruitType);
                break;
            case EFoodType.FastFood:
                collected = GameData.GetFoodCollected(EFoodType.FastFood, achievementSO.RequiredFastFoodType);
                break;
            case EFoodType.Cake:
                collected = GameData.GetFoodCollected(EFoodType.Cake, achievementSO.RequiredCakeType);
                break;
        }

        achievement.SetProgress(collected);
    }

    #endregion

    #region Event Handlers

    private void HandleAchievementProgressUpdated(AchievementProgress progress)
    {
        Main.Observer.Notify(EEvent.OnAchievementProgressUpdated, progress);
    }

    private void HandleAchievementUnlocked(AchievementProgress progress)
    {
        Main.Observer.Notify(EEvent.OnAchievementUnlocked, progress);
    }

    private void HandleAchievementRewardClaimed(AchievementProgress progress)
    {
        Main.Observer.Notify(EEvent.OnAchievementRewardClaimed, progress);
    }

    #endregion

    #region Save/Load System

    private void SaveAchievementProgress()
    {
        var saveList = new List<AchievementSaveEntry>();
        
        foreach (var achievement in allAchievements)
        {
            var entry = new AchievementSaveEntry(
                achievement.AchievementId,
                achievement.CurrentProgress,
                achievement.IsUnlocked,
                achievement.IsRewardClaimed,
                achievement.UnlockedDate
            );
            saveList.Add(entry);
        }

        string jsonData = JsonUtility.ToJson(new AchievementSaveData { achievements = saveList }, true);
        GameData.AchievementData = jsonData;
    }

    private void LoadAchievementProgress()
    {
        string jsonData = GameData.AchievementData;
        if (string.IsNullOrEmpty(jsonData)) return;

        try
        {
            var saveData = JsonUtility.FromJson<AchievementSaveData>(jsonData);
            
            foreach (var entry in saveData.achievements)
            {
                var achievement = allAchievements.FirstOrDefault(a => a.AchievementId == entry.achievementId);
                if (achievement != null)
                {
                    // Create new achievement progress with saved data
                    var newProgress = new AchievementProgress(
                        entry.achievementId,
                        entry.currentProgress,
                        entry.isUnlocked,
                        entry.isRewardClaimed,
                        entry.GetUnlockedDate()
                    );
                    
                    // Set the SO reference and events
                    newProgress.SetAchievementSO(achievement.Achievement);
                    newProgress.OnProgressUpdated += HandleAchievementProgressUpdated;
                    newProgress.OnAchievementUnlocked += HandleAchievementUnlocked;
                    newProgress.OnRewardClaimed += HandleAchievementRewardClaimed;

                    // Replace in list
                    int index = allAchievements.IndexOf(achievement);
                    allAchievements[index] = newProgress;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load achievement data: {e.Message}");
        }
    }

    #endregion

    #region Public Methods

    public AchievementProgress GetAchievement(string achievementId)
    {
        return allAchievements.FirstOrDefault(a => a.AchievementId == achievementId);
    }

    public void ClaimAchievementReward(string achievementId)
    {
        var achievement = GetAchievement(achievementId);
        achievement?.ClaimReward();
    }

    public int GetUnlockedCount()
    {
        return allAchievements.Count(a => a.IsUnlocked);
    }

    public int GetTotalCount()
    {
        return allAchievements.Count;
    }

    public float GetCompletionPercentage()
    {
        if (allAchievements.Count == 0) return 0f;
        return (float)GetUnlockedCount() / GetTotalCount() * 100f;
    }

    [Button("Refresh Achievement Progress")]
    public void RefreshAchievementProgress()
    {
        UpdateAchievementProgressFromGameData();
        Debug.Log("Achievement progress refreshed from GameData!");
    }

    [Button("Clear Achievement Data")]
    private void ClearAchievementData()
    {
        GameData.AchievementData = "";
        foreach (var achievement in allAchievements)
        {
            // Reset progress (would need to recreate achievement progress objects)
        }
        Debug.Log("Achievement data cleared!");
    }

    #endregion
}

[System.Serializable]
public class AchievementSaveData
{
    public List<AchievementSaveEntry> achievements = new List<AchievementSaveEntry>();
}