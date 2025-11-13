using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DailyQuestSaveData
{
    [SerializeField] public string lastLoginDate;
    [SerializeField] public List<DailyQuestSaveEntry> todaysQuests = new List<DailyQuestSaveEntry>();

    public DailyQuestSaveData()
    {
        lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd");
        todaysQuests = new List<DailyQuestSaveEntry>();
    }

    // Check if the saved data is from today
    public bool IsFromToday()
    {
        if (DateTime.TryParse(lastLoginDate, out DateTime savedDate))
        {
            return savedDate.Date == DateTime.Now.Date;
        }
        return false;
    }

    // Clear expired data (from previous days)
    public void ClearIfExpired()
    {
        if (!IsFromToday())
        {
            todaysQuests.Clear();
            lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}

[System.Serializable]
public class DailyQuestSaveEntry
{
    [SerializeField] public string questName;
    [SerializeField] public int currentProgress;
    [SerializeField] public bool isCompleted;
    [SerializeField] public bool isRewardClaimed;
    [SerializeField] public string questDate; // Always today for daily quests
    [SerializeField] public EDailyQuestType questType;
    [SerializeField] public int targetValue;
    [SerializeField] public int goldReward;

    // For CollectSpecificFood quests
    [SerializeField] public EFoodType requiredFoodType;
    [SerializeField] public EFruitType requiredFruitType;
    [SerializeField] public EFastFoodType requiredFastFoodType;
    [SerializeField] public ECakeType requiredCakeType;

    public DailyQuestSaveEntry(DailyQuestProgress questProgress)
    {
        var quest = questProgress.Quest;

        this.questName = quest.QuestName;
        this.currentProgress = questProgress.CurrentProgress;
        this.isCompleted = questProgress.IsCompleted;
        this.isRewardClaimed = questProgress.IsRewardClaimed;
        this.questDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Save quest properties for recreation
        this.questType = quest.QuestType;
        this.targetValue = quest.TargetValue;
        this.goldReward = quest.GoldReward;

        if (quest.QuestType == EDailyQuestType.CollectSpecificFood)
        {
            this.requiredFoodType = quest.RequiredFoodType;
            this.requiredFruitType = quest.RequiredFruitType;
            this.requiredFastFoodType = quest.RequiredFastFoodType;
            this.requiredCakeType = quest.RequiredCakeType;
        }
    }

    public DateTime GetQuestDate()
    {
        if (DateTime.TryParse(questDate, out DateTime date))
        {
            return date;
        }
        return DateTime.Now;
    }

    public bool IsFromToday()
    {
        return GetQuestDate().Date == DateTime.Now.Date;
    }
}

public static class DailyQuestSaveSystem
{
    private const string SAVE_KEY = "DailyQuestData";

    public static void SaveDailyQuestData(DailyQuestSaveData data)
    {
        try
        {
            // Always update the date when saving
            data.lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd");

            string jsonData = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception)
        {
        }
    }

    public static DailyQuestSaveData LoadDailyQuestData()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string jsonData = PlayerPrefs.GetString(SAVE_KEY);
                DailyQuestSaveData data = JsonUtility.FromJson<DailyQuestSaveData>(jsonData);

                if (data != null)
                {
                    // Automatically clear data if it's from a previous day
                    data.ClearIfExpired();
                    return data;
                }
            }
        }
        catch (Exception)
        {
        }

        return new DailyQuestSaveData();
    }

    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }

    public static bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public static bool HasTodaysSaveData()
    {
        var data = LoadDailyQuestData();
        return data.IsFromToday() && data.todaysQuests.Count > 0;
    }
}