using OSK;
using UnityEngine;

public class UIMenu_Manager : MonoBehaviour
{
    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }
    private void Start()
    {
        Main.UI.Open<MainMenuUI>();
    }
}
