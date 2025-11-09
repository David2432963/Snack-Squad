using OSK;
using UnityEngine;
using UnityEngine.UI;

public class GoldDisplay : MonoBehaviour
{
    [SerializeField] private Text textGold;

    private void Start()
    {
        UpdateGoldDisplay();

        Main.Observer.Add(EEvent.OnGoldChanged, UpdateGoldDisplay);
    }
    private void UpdateGoldDisplay(object data = null)
    {
        if (textGold != null)
        {
            textGold.text = GameData.Gold.ToString();
        }
    }

    private void OnDestroy()
    {
        Main.Observer.Remove(EEvent.OnGoldChanged, UpdateGoldDisplay);
    }
}
