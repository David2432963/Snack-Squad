using UnityEngine;

public static class GameData
{
    public static int Gold
    {
        set
        {
            Gold = value;
            PlayerPrefs.SetInt("GoldAmount", value);
            PlayerPrefs.Save();
        }
        get
        {
            return PlayerPrefs.GetInt("GoldAmount", 0);
        }
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
}
