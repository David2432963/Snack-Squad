using OSK;
using UnityEngine;

public class BadFood : Food
{
    [SerializeField] private EBadEffect badEffect;
    [SerializeField] private float effectDuration;

    protected override string spawnedText => "<color=red>[BadFood]</color> Chili";

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Main.Observer.Notify(EEvent.OnBadFoodCollected, this);
            Main.Sound.Play(SoundID.BadFood.ToString());
            if (badEffect == EBadEffect.Slow)
            {
                other.GetComponent<CharacterEffect>()?.Slow(effectDuration);
            }
        }
        else if (other.CompareTag("Bot"))
        {
            Main.Observer.Notify(EEvent.OnBadFoodCollected, this);
            if (badEffect == EBadEffect.Slow)
            {
                other.GetComponent<CharacterEffect>()?.Slow(effectDuration);
            }
        }


        base.OnTriggerEnter(other);
    }
}
