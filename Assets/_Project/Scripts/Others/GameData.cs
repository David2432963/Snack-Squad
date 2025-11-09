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

    // Food collection tracking for achievements
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
