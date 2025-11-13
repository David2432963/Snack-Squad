using OSK;
using UnityEngine;

public class Menu_Manager : MonoBehaviour
{
    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }
    private void Start()
    {
        // GameData.Gold += 200;
        Main.Sound.Play(SoundID.MenuMusic.ToString(), loop: true);
    }
    public void StartGame()
    {
        Main.Director
            .LoadScene(
            new DataScene() { sceneName = "Gameplay", loadMode = ELoadMode.Single })
            .Async(true)
            .OnStart(() =>
            {
                Main.Sound.StopAll();
                Main.Sound.DestroyAll();
            }).OnComplete(() =>
            {
                Main.UI.Get<MainMenuUI>().Hide();
            }).Build();
    }
}
