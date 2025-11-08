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
    }
    private void Start()
    {
        // Subscribe to session food type selection
        Main.Observer.Add("OnSessionFoodTypeSelected", OnSessionFoodTypeSelected);
        
        // If GameData_Manager is already initialized, get the food type immediately
        if (GameData_Manager.Instance != null)
        {
            InitializeWithFoodType(GameData_Manager.Instance.CurrentSessionFoodType);
        }
        
        StartCoroutine(nameof(IECheckSpawnFood));
        Main.Observer.Add(EEvent.OnGoodFoodCollected, OnRemoveFood);
        Main.Observer.Add(EEvent.OnBadFoodCollected, OnRemoveBadFood);

        CheckEmptyTiles();
        
        // Only spawn if food type is already initialized
        if (foodTypeInitialized)
        {
            SpawnFood();
        }
    }

    private void OnSessionFoodTypeSelected(object data)
    {
        if (data is EFoodType foodType)
        {
            InitializeWithFoodType(foodType);
        }
    }

    private void InitializeWithFoodType(EFoodType foodType)
    {
        sessionFoodType = foodType;
        foodTypeInitialized = true;
        
        Debug.Log($"[FoodSpawner] Session food type set to: {sessionFoodType}");
        
        // Clear and setup food prefabs for the selected type
        foodPrefabs.Clear();
        SetupFoodPrefabsForSession();
        
        // Spawn initial food if we haven't already
        if (currentGoodFoodCount == 0)
        {
            SpawnFood();
        }
    }

    private void SetupFoodPrefabsForSession()
    {
        switch (sessionFoodType)
        {
            case EFoodType.Fruit:
                foodPrefabs.AddRange(fruitsPrefabs);
                Debug.Log($"[FoodSpawner] Added {fruitsPrefabs.Length} fruit prefabs for session");
                break;
            case EFoodType.FastFood:
                foodPrefabs.AddRange(fastFoodPrefabs);
                Debug.Log($"[FoodSpawner] Added {fastFoodPrefabs.Length} fast food prefabs for session");
                break;
            case EFoodType.Cake:
                Debug.LogWarning("[FoodSpawner] Cake food type not yet implemented!");
                break;
            default:
                Debug.LogError($"[FoodSpawner] Unknown food type: {sessionFoodType}");
                break;
        }
    }
    [Button]
    private void SpawnFood()
    {
        if (!foodTypeInitialized)
        {
            Debug.LogWarning("[FoodSpawner] Cannot spawn food - session food type not initialized yet!");
            return;
        }

        if (foodPrefabs.Count == 0)
        {
            Debug.LogWarning($"[FoodSpawner] No food prefabs available for {sessionFoodType}!");
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
        
        Debug.Log($"[FoodSpawner] Spawned {sessionFoodType} foods. Current count: {currentGoodFoodCount}/{maxGoodFoodToSpawn}");
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
                Debug.Log($"[FoodSpawner] Auto-spawned {sessionFoodType} food. Count: {currentGoodFoodCount}/{maxGoodFoodToSpawn}");
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
        if (data is Food food)
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
        Main.Observer.Remove("OnSessionFoodTypeSelected", OnSessionFoodTypeSelected);
    }
}
