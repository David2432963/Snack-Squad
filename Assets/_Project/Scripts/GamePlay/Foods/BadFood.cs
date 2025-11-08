using OSK;
using UnityEngine;

public class BadFood : Food
{
    [SerializeField] private EBadEffect badEffect;
    [SerializeField] private float effectDuration;

    protected override void OnTriggerEnter(Collider other)
    {
        bool isPlayer = false;

        if (other.CompareTag("Player"))
        {
            Main.Observer.Notify(EEvent.OnBadFoodCollected, this);
            if (badEffect == EBadEffect.Slow)
            {
                other.GetComponent<CharacterEffect>()?.Slow(effectDuration);
            }
            isPlayer = true;
        }
        else if (other.CompareTag("Bot"))
        {
            Main.Observer.Notify(EEvent.OnBadFoodCollected, this);
            if (badEffect == EBadEffect.Slow)
            {
                other.GetComponent<CharacterEffect>()?.Slow(effectDuration);
            }
            isPlayer = true;
        }

        if (isPlayer)
        {
            base.OnTriggerEnter(other);
        }
    }
}
