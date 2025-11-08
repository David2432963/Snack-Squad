using DG.Tweening;
using OSK;
using UnityEngine;

public class Boostrap : GameInit
{
    public override void OnInit()
    {
        SingletonManager.Instance.RegisterScene(this);
        DOVirtual.DelayedCall(0.1f, StartLoading);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartLoading();
        }
    }

    private void StartLoading()
    {
        Main.Director
            .LoadScene(
            new DataScene() { sceneName = "MainMenu", loadMode = ELoadMode.Single })
            .Async(true)
            .FakeDuration(1f)
            .OnStart(() =>
            {
                Main.UI.Open<LoadingUI>();
            }).OnComplete(() =>
            {
                Main.UI.Get<LoadingUI>().Hide();
            }).Build();
    }
}
