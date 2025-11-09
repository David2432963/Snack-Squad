using UnityEngine;

public class Cake : GoodFood
{
    [Header("Cake Specific")]
    [SerializeField] protected ECakeType cakeType;

    public ECakeType CakeType => cakeType;
    public override bool MatchType(object type)
    {
        if (type is ECakeType cakeType)
        {
            return base.MatchType(type) && cakeType == this.cakeType;
        }
        return false;
    }
}
