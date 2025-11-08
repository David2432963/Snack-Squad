using OSK;
using UnityEngine;

public class Menu_Manager : MonoBehaviour
{
    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }
    public void StartGame()
    {
        Main.Director
            .LoadScene(
            new DataScene() { sceneName = "Gameplay", loadMode = ELoadMode.Single })
            .Async(true)
            .OnStart(() =>
            {
            }).OnComplete(() =>
            {
            }).Build();
    }
}
