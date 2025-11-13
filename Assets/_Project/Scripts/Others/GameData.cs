using OSK;
using UnityEngine;
using System.Collections.Generic;

public static class GameData
{
    public static int Gold
    {
        set
        {
            PlayerPrefs.SetInt("GoldAmount", value);
            Main.Observer.Notify(EEvent.OnGoldChanged);
            PlayerPrefs.Save();
        }
        get
        {
            return PlayerPrefs.GetInt("GoldAmount", 0);
        }
    }

    public static int TotalFoodCollected
    {
        set
        {
            PlayerPrefs.SetInt("TotalFoodCollected", value);
            PlayerPrefs.Save();
        }
        get
        {
            return PlayerPrefs.GetInt("TotalFoodCollected", 0);
        }
    }

    public static int TotalQuestsCompleted
    {
        set
        {
            PlayerPrefs.SetInt("TotalQuestsCompleted", value);
            PlayerPrefs.Save();
        }
        get
        {
            return PlayerPrefs.GetInt("TotalQuestsCompleted", 0);
        }
    }

    // Achievement data
    public static string AchievementData
    {
        set
        {
            PlayerPrefs.SetString("AchievementData", value);
            PlayerPrefs.Save();
        }
        get
        {
            return PlayerPrefs.GetString("AchievementData", "");
        }
    }

    // Daily Quest Progress Tracking (for cross-scene persistence)
    public static void AddNormalQuestCompleted()
    {
        int current = PlayerPrefs.GetInt("NormalQuestsCompletedToday", 0);
        PlayerPrefs.SetInt("NormalQuestsCompletedToday", current + 1);
        PlayerPrefs.Save();

        // Notify observers about quest completion for daily quest tracking
        Main.Observer.Notify(EEvent.OnQuestCompleted);
        TotalQuestsCompleted++;
    }

    public static int GetNormalQuestsCompletedToday()
    {
        return PlayerPrefs.GetInt("NormalQuestsCompletedToday", 0);
    }

    public static void ResetDailyProgress()
    {
        PlayerPrefs.DeleteKey("NormalQuestsCompletedToday");
        
        // Clear daily food collection data
        ClearDailyFoodData();
        
        PlayerPrefs.Save();
    }
    
    private static void ClearDailyFoodData()
    {
        // Clear all daily food collection keys
        foreach (EFoodType foodType in System.Enum.GetValues(typeof(EFoodType)))
        {
            // Clear general food type
            string dailyKey = $"DailyFood_{foodType}";
            PlayerPrefs.DeleteKey(dailyKey);
            
            // Clear specific food types
            switch (foodType)
            {
                case EFoodType.Fruit:
                    foreach (EFruitType fruitType in System.Enum.GetValues(typeof(EFruitType)))
                    {
                        PlayerPrefs.DeleteKey($"DailyFood_{foodType}_{fruitType}");
                    }
                    break;
                case EFoodType.FastFood:
                    foreach (EFastFoodType fastFoodType in System.Enum.GetValues(typeof(EFastFoodType)))
                    {
                        PlayerPrefs.DeleteKey($"DailyFood_{foodType}_{fastFoodType}");
                    }
                    break;
                case EFoodType.Cake:
                    foreach (ECakeType cakeType in System.Enum.GetValues(typeof(ECakeType)))
                    {
                        PlayerPrefs.DeleteKey($"DailyFood_{foodType}_{cakeType}");
                    }
                    break;
            }
        }
    }

    // Check if daily progress should be reset (new day)
    public static void CheckAndResetDailyProgress()
    {
        string lastResetDate = PlayerPrefs.GetString("LastDailyReset", "");
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        
        if (lastResetDate != today)
        {
            ResetDailyProgress();
            PlayerPrefs.SetString("LastDailyReset", today);
            PlayerPrefs.Save();
        }
    }

    // Character Skin Data
    public static ESkin CurrentSkin
    {
        set
        {
            PlayerPrefs.SetInt("CurrentSkin", (int)value);
            PlayerPrefs.Save();
        }
        get
        {
            return (ESkin)PlayerPrefs.GetInt("CurrentSkin", (int)ESkin.Skin0);
        }
    }

    // Food collection tracking for achievements and daily quests
    public static void AddFoodCollected(EFoodType foodType, object specificType = null)
    {
        // Update total food collected
        TotalFoodCollected++;

        // Update specific food type counters
        string key = $"Food_{foodType}";
        if (specificType != null)
        {
            key += $"_{specificType}";
        }

        int current = PlayerPrefs.GetInt(key, 0);
        PlayerPrefs.SetInt(key, current + 1);
        PlayerPrefs.Save();
        
        // Also track daily collections (resets each day)
        string dailyKey = $"DailyFood_{foodType}";
        if (specificType != null)
        {
            dailyKey += $"_{specificType}";
        }
        
        int dailyCurrent = PlayerPrefs.GetInt(dailyKey, 0);
        PlayerPrefs.SetInt(dailyKey, dailyCurrent + 1);
        PlayerPrefs.Save();
    }

    public static int GetFoodCollected(EFoodType foodType, object specificType = null)
    {
        string key = $"Food_{foodType}";
        if (specificType != null)
        {
            key += $"_{specificType}";
        }
        return PlayerPrefs.GetInt(key, 0);
    }

    public static int GetDailyFoodCollected(EFoodType foodType, object specificType = null)
    {
        string dailyKey = $"DailyFood_{foodType}";
        if (specificType != null)
        {
            dailyKey += $"_{specificType}";
        }
        return PlayerPrefs.GetInt(dailyKey, 0);
    }

    public static bool IsSkinUnlocked(ESkin skin)
    {
        return PlayerPrefs.GetInt($"Skin_{skin}", 0) == 1;
    }

    public static void UnlockSkin(ESkin skin)
    {
        PlayerPrefs.SetInt($"Skin_{skin}", 1);
        PlayerPrefs.Save();
    }

    public static void LockSkin(ESkin skin)
    {
        PlayerPrefs.SetInt($"Skin_{skin}", 0);
        PlayerPrefs.Save();
    }

    // Clear all data (for debugging/reset)
    public static void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
