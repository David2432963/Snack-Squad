using System.Collections;
using UnityEngine;

public class CharacterEffect : MonoBehaviour, ISlow, IStun
{
    [SerializeField] protected bool canSlow;
    [SerializeField] protected bool canStun;

    protected void Start()
    {
        RefreshEffects();
    }

    private bool isSlowed;
    public bool IsSlowed => isSlowed;
    public void Slow(float duration)
    {
        if (!canSlow) return;

        StartCoroutine(IESlow(duration));
    }
    private IEnumerator IESlow(float duration)
    {
        isSlowed = true;
        yield return new WaitForSeconds(duration);
        isSlowed = false;
    }

    private bool isStunned;
    public bool IsStunned => isStunned;
    public void Stun(float duration)
    {
        if (!canStun) return;

        StartCoroutine(IEStun(duration));
    }
    private IEnumerator IEStun(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    public void RefreshEffects()
    {
        StopAllCoroutines();

        isSlowed = false;
        isStunned = false;
    }
}
