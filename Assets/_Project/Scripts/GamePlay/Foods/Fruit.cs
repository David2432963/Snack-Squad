using UnityEngine;

public class Fruit : GoodFood
{
    [Header("Fruit Specific")]
    [SerializeField] protected EFruitType fruitType;

    public EFruitType FruitType => fruitType;
    public override bool MatchType(object type)
    {
        if (type is EFruitType fruitType)
        {
            return base.MatchType(type) && fruitType == this.fruitType;
        }
        return false;
    }
}
