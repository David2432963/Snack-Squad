using System.Collections;
using System.Collections.Generic;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] private Tile[] tiles;
    [SerializeField] private Fruit[] fruitsPrefabs;
    [SerializeField] private FastFood[] fastFoodPrefabs;
    [SerializeField] private Cake[] cakePrefabs;
    [SerializeField] private BadFood[] badFoodPrefabs;
    [SerializeField] private int maxGoodFoodToSpawn;
    [SerializeField] private int maxBadFoodToSpawn;

    private int currentGoodFoodCount;
    private int currentBadFoodCount;
    private List<Tile> emptyTiles = new();
    private List<Food> foodPrefabs = new();
    private List<Food> spawnedFoods = new();

    // Session food type tracking
    private EFoodType sessionFoodType;
    private bool foodTypeInitialized = false;

    // Public access to spawned foods
    public static FoodSpawner Instance => SingletonManager.Instance.Get<FoodSpawner>();
    public IReadOnlyList<Food> SpawnedFoods => spawnedFoods.AsReadOnly();
    public EFoodType SessionFoodType => sessionFoodType;


    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
        Main.Observer.Add(EEvent.OnGoodFoodCollected, OnRemoveFood);
        Main.Observer.Add(EEvent.OnBadFoodCollected, OnRemoveBadFood);
    }
    private void Start()
    {
        // Randomly select food type for this session

        InitializeRandomFoodType();

        StartCoroutine(nameof(IECheckSpawnFood));

        CheckEmptyTiles();
        SpawnFood();
    }

    private void InitializeRandomFoodType()
    {
        // Get all available food types (excluding Cake if not implemented)
        List<EFoodType> availableFoodTypes = new List<EFoodType>
        {
            EFoodType.Fruit,
            EFoodType.FastFood,
            EFoodType.Cake
        };

        // Randomly select one food type for this session
        sessionFoodType = availableFoodTypes[Random.Range(0, availableFoodTypes.Count)];
        foodTypeInitialized = true;

        // Setup food prefabs for the selected type
        SetupFoodPrefabsForSession();

        // Notify other systems about the selected food type
        Main.Observer.Notify(EEvent.OnSessionFoodTypeSelected, sessionFoodType);
    }

    private void SetupFoodPrefabsForSession()
    {
        foodPrefabs.Clear();

        switch (sessionFoodType)
        {
            case EFoodType.Fruit:
                foodPrefabs.AddRange(fruitsPrefabs);
                break;
            case EFoodType.FastFood:
                foodPrefabs.AddRange(fastFoodPrefabs);
                break;
            case EFoodType.Cake:
                foodPrefabs.AddRange(cakePrefabs);
                break;
        }
    }

    [Button]
    private void SpawnFood()
    {
        if (!foodTypeInitialized)
        {
            return;
        }

        if (foodPrefabs.Count == 0)
        {
            return;
        }

        while (currentGoodFoodCount < maxGoodFoodToSpawn && emptyTiles.Count > 0)
        {
            int random = Random.Range(0, emptyTiles.Count);
            var foodPrefab = foodPrefabs[Random.Range(0, foodPrefabs.Count)];
            var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab, transform);
            emptyTiles[random].AddFood(food);
            emptyTiles.RemoveAt(random);
            spawnedFoods.Add(food);

            currentGoodFoodCount++;
        }
    }

    private void SpawnBadFood()
    {
        foreach (var tile in tiles)
        {
            var foodPrefab = badFoodPrefabs[Random.Range(0, badFoodPrefabs.Length)];
            var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab, transform);
            tile.AddFood(food);
            spawnedFoods.Add(food);
        }
    }

    private void CheckEmptyTiles()
    {
        foreach (var tile in tiles)
        {
            if (tile.IsEmpty && !emptyTiles.Contains(tile))
            {
                emptyTiles.Add(tile);
            }
        }
    }

    private IEnumerator IECheckSpawnFood()
    {
        yield return new WaitForSeconds(5f);

        while (true)
        {
            // Only spawn if food type is initialized
            if (!foodTypeInitialized)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            CheckEmptyTiles();
            if (currentGoodFoodCount < maxGoodFoodToSpawn && emptyTiles.Count > 0)
            {
                int random = Random.Range(0, emptyTiles.Count);
                var foodPrefab = foodPrefabs[Random.Range(0, foodPrefabs.Count)];
                var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab, transform);
                emptyTiles[random].AddFood(food);
                emptyTiles.RemoveAt(random);
                spawnedFoods.Add(food);

                currentGoodFoodCount++;
            }
            if (currentBadFoodCount < maxBadFoodToSpawn && emptyTiles.Count > 0)
            {
                int random = Random.Range(0, emptyTiles.Count);
                var foodPrefab = badFoodPrefabs[Random.Range(0, badFoodPrefabs.Length)];
                var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab, transform);
                emptyTiles[random].AddFood(food);
                emptyTiles.RemoveAt(random);
                spawnedFoods.Add(food);

                currentBadFoodCount++;
            }
            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
    }

    private void SpawnFood(EFoodType foodType)
    {
        Food[] foods = null;
        switch (foodType)
        {
            case EFoodType.Fruit:
                foods = fruitsPrefabs;
                break;
            case EFoodType.FastFood:
                foods = fastFoodPrefabs;
                break;
        }

        foreach (var tile in tiles)
        {
            var foodPrefab = foods[Random.Range(0, foods.Length)];
            var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab, transform);
            tile.AddFood(food);
            spawnedFoods.Add(food);
        }
    }

    private void OnRemoveFood(object data)
    {
        Food food = null;

        // Handle new FoodCollectionData format
        if (data is FoodCollectionData collectionData)
        {
            food = collectionData.food;
        }
        // Handle legacy Food format for backward compatibility
        else if (data is Food legacyFood)
        {
            food = legacyFood;
        }

        if (food != null)
        {
            spawnedFoods.Remove(food);
        }
        currentGoodFoodCount--;
    }
    private void OnRemoveBadFood(object data)
    {
        if (data is Food food)
        {
            spawnedFoods.Remove(food);
        }
        currentBadFoodCount--;
    }

    private void OnDestroy()
    {
        Main.Observer.Remove(EEvent.OnGoodFoodCollected, OnRemoveFood);
        Main.Observer.Remove(EEvent.OnBadFoodCollected, OnRemoveBadFood);
    }
}
