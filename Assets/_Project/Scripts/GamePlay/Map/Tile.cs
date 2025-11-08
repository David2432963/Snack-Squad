using UnityEngine;

public class Tile : MonoBehaviour
{
    private Food foodOnTile;
    public bool IsEmpty => foodOnTile == null;

    public void AddFood(Food food)
    {
        foodOnTile = food;
        food.transform.position = transform.position;
        food.OnRemoved += () =>
        {
            foodOnTile = null;
        };
    }
    public void RemoveFood()
    {
        foodOnTile = null;
    }
}
