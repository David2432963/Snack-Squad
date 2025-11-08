using UnityEngine;

public class Fruit : GoodFood
{
    [Header("Fruit Specific")]
    [SerializeField] protected EFruitType fruitType;

    public EFruitType FruitType => fruitType;
    public override bool MathchType(object type)
    {
        if (type is EFruitType fruitType)
        {
            return base.MathchType(type) && fruitType == this.fruitType;
        }
        return false;
    }
}
