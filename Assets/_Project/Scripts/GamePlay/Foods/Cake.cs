using UnityEngine;

public class Cake : GoodFood
{
    [Header("Cake Specific")]
    [SerializeField] protected ECakeType cakeType;

    public ECakeType CakeType => cakeType;

    protected override string spawnedText => $"<color=orange>[Cake]</color> {cakeType}";

    protected override object GetSpecificFoodType()
    {
        return cakeType;
    }
    
    public override bool MatchType(object type)
    {
        if (type is ECakeType cakeType)
        {
            return base.MatchType(type) && cakeType == this.cakeType;
        }
        return false;
    }
}
