using UnityEngine;

public class Fruit : GoodFood
{
    [Header("Fruit Specific")]
    [SerializeField] protected EFruitType fruitType;
    public EFruitType FruitType => fruitType;
    protected override string spawnedText => $"<color=orange>[Fruit]</color> {fruitType}";

    protected override object GetSpecificFoodType()
    {
        return fruitType;
    }

    public override bool MatchType(object type)
    {
        if (type is EFruitType fruitType)
        {
            return base.MatchType(type) && fruitType == this.fruitType;
        }
        return false;
    }
}
