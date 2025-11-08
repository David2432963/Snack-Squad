using OSK;
using UnityEngine;

public class GamePlayUI_Manager : MonoBehaviour
{
    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }

    private void Start()
    {
        
    }
}
