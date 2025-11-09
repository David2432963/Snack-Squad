using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DailyQuestSaveData
{
    [SerializeField] public string lastLoginDate;
    [SerializeField] public List<DailyQuestSaveEntry> activeQuests = new List<DailyQuestSaveEntry>();

    public DailyQuestSaveData()
    {
        lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd");
        activeQuests = new List<DailyQuestSaveEntry>();
    }
}

[System.Serializable]
public class DailyQuestSaveEntry
{
    [SerializeField] public string questName;
    [SerializeField] public int currentProgress;
    [SerializeField] public bool isCompleted;
    [SerializeField] public bool isRewardClaimed;
    [SerializeField] public string assignedDate;

    public DailyQuestSaveEntry(string questName, int progress, bool completed, bool rewardClaimed, DateTime assignedDate)
    {
        this.questName = questName;
        this.currentProgress = progress;
        this.isCompleted = completed;
        this.isRewardClaimed = rewardClaimed;
        this.assignedDate = assignedDate.ToString("yyyy-MM-dd");
    }

    public DateTime GetAssignedDate()
    {
        if (DateTime.TryParse(assignedDate, out DateTime date))
        {
            return date;
        }
        return DateTime.Now;
    }
}

public static class DailyQuestSaveSystem
{
    private const string SAVE_KEY = "DailyQuestData";

    public static void SaveDailyQuestData(DailyQuestSaveData data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save daily quest data: {e.Message}");
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
                return data ?? new DailyQuestSaveData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load daily quest data: {e.Message}");
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
}