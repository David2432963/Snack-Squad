using UnityEngine;
using System;

[Serializable]
public struct RangeFloat
{
    public float min;
    public float max;

    public RangeFloat(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float RandomInRange()
    {
        return UnityEngine.Random.Range(min, max);
    }
}

[Serializable]
public struct FoodCollectionData
{
    public GoodFood food;
    public EPlayerType playerType;

    public FoodCollectionData(GoodFood food, EPlayerType playerType)
    {
        this.food = food;
        this.playerType = playerType;
    }
}

public interface IPlayerInput
{
    Vector2 Movement { get; }
    bool Fire { get; }
    bool FireHold { get; }

    void Tick(float deltaTime);
}

public interface IStun
{
    void Stun(float duration);
}
public interface ISlow
{
    void Slow(float duration);
}