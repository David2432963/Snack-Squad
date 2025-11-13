using UnityEngine;
using UnityEngine.UI;

public class ScrollBG : MonoBehaviour
{
    public float speed = 0.15f; //Speed Of Scroll
    public Vector2 direction = new Vector2(1, -1); //Normalized Direction Of Scroll

    Image img;

    private void Start()
    {
        img = GetComponent<Image>();
    }

    private void Update()
    {
        img.material.mainTextureOffset += -direction.normalized * Time.deltaTime * speed;
    }
}
