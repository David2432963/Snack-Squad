using OSK;
using UnityEngine;

public class GoodFood : Food
{
    [SerializeField] protected EFoodType foodType;

    public EFoodType FoodType => foodType;

    public virtual bool MatchType(object type)
    {
        if (type is EFoodType foodType)
        {
            return foodType == this.foodType;
        }
        return false;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        EPlayerType playerType = EPlayerType.Player; // Default to main player
        bool isPlayer = false;

        if (other.CompareTag("Player"))
        {
            Main.Sound.Play(SoundID.CollectItem.ToString());
            playerType = EPlayerType.Player;
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

        var collectionData = new FoodCollectionData(this, playerType);
        if (isPlayer)
        {
            SaveFoodCollectionToGameData();

            // Pass both food and player type information for quest system
            Main.Observer.Notify(EEvent.OnPlayerCollectFood, collectionData);
        }
        Main.Observer.Notify(EEvent.OnGoodFoodCollected, collectionData);

        base.OnTriggerEnter(other);
    }

    protected virtual void SaveFoodCollectionToGameData()
    {
        object specificType = GetSpecificFoodType();
        GameData.AddFoodCollected(foodType, specificType);
    }

    protected virtual object GetSpecificFoodType()
    {
        // Base implementation returns null
        // Override in derived classes (Fruit, FastFood, Cake) to return specific types
        return null;
    }
}
