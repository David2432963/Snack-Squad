using OSK;
using UnityEngine;
using UnityEngine.UI;

public class BGTileMove : MonoBehaviour, IFixedUpdate
{
    private Image bg;
    public Vector2 direction = new Vector2(1, -1);
    public float speed = .2f;

    private void Start()
    {
        Main.Mono.Register(this);
        bg = GetComponent<Image>();
    }

    public void FixedTick(float fixedDeltaTime)
    {
        HPDebug.Log("BG Move Fixed Tick");
        bg.material.mainTextureOffset += -direction.normalized * fixedDeltaTime * speed;
    }
    private void OnDestroy()
    {
        Main.Mono.UnRegister(this);
    }
}
