using UnityEngine;

public class FastFood : GoodFood
{
    [Header("Fruit Specific")]
    [SerializeField] protected EFastFoodType fastFoodType;

    public EFastFoodType FastFoodType => fastFoodType;
    public override bool MatchType(object type)
    {
        if (type is EFastFoodType fastFoodType)
        {
            return base.MatchType(type) && fastFoodType == this.fastFoodType;
        }
        return false;
    }
}
