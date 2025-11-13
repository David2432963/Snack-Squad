using OSK;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private Animator animator;
    private TextMesh textMesh;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        textMesh = GetComponentInChildren<TextMesh>();
    }
    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }

    }
    // Animator event
    public void OnDoneAnimation()
    {
        Main.Pool.Despawn(gameObject);
    }
}
