using OSK;
using UnityEngine;

public class GoodFood : Food
{
    [SerializeField] protected EFoodType foodType;
    [SerializeField] private int point;
    [SerializeField] private ParticleSystem[] blowFx;

    public EFoodType FoodType => foodType;
    public int Point => point;

    public virtual bool MathchType(object type)
    {
        if (type is EFoodType foodType)
        {
            return foodType == this.foodType;
        }
        return false;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        EPlayerType playerType = EPlayerType.MainPlayer; // Default to main player
        bool isPlayer = false;

        if (other.CompareTag("Player"))
        {
            playerType = EPlayerType.MainPlayer;
            isPlayer = true;
        }
        else if (other.CompareTag("Bot"))
        {
            // Get the bot's player type from the BotController component
            var botController = other.GetComponent<BotController>();
            if (botController != null)
            {
                playerType = botController.PlayerType;
                isPlayer = true;
            }
        }

        if (isPlayer)
        {
            GameData_Manager.Instance.AddScore(playerType, point);
            Main.Observer.Notify(EEvent.OnGoodFoodCollected, this);
        }
        
        base.OnTriggerEnter(other);
    }
}
