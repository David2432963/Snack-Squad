using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<GoodFood> spawnedGoodFoods = new();
    private List<BadFood> spawnedBadFoods = new();

    // Session food type tracking
    private EFoodType sessionFoodType;
    private bool foodTypeInitialized = false;

    // Public access to spawned foods
    public static FoodSpawner Instance => SingletonManager.Instance.Get<FoodSpawner>();
    public IReadOnlyList<Food> SpawnedFoods => spawnedGoodFoods.AsReadOnly();
    public EFoodType SessionFoodType => sessionFoodType;


    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
        Main.Observer.Add(EEvent.OnGoodFoodCollected, OnRemoveGoodFood);
        Main.Observer.Add(EEvent.OnBadFoodCollected, OnRemoveBadFood);
        Main.Observer.Add(EEvent.OnGameOver, OnEndGame);
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
            var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab.gameObject, transform).GetComponent<GoodFood>();
            emptyTiles[random].AddFood(food);
            emptyTiles.RemoveAt(random);
            spawnedGoodFoods.Add(food);

            currentGoodFoodCount++;
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
            SpawnQuestFood();
            if (currentGoodFoodCount < maxGoodFoodToSpawn && emptyTiles.Count > 0)
            {
                int random = Random.Range(0, emptyTiles.Count);
                var foodPrefab = foodPrefabs[Random.Range(0, foodPrefabs.Count)];
                var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab.gameObject, transform).GetComponent<GoodFood>();
                emptyTiles[random].AddFood(food);
                emptyTiles.RemoveAt(random);
                spawnedGoodFoods.Add(food);

                currentGoodFoodCount++;
            }
            if (currentBadFoodCount < maxBadFoodToSpawn && emptyTiles.Count > 0)
            {
                int random = Random.Range(0, emptyTiles.Count);
                var foodPrefab = badFoodPrefabs[Random.Range(0, badFoodPrefabs.Length)];
                var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab.gameObject, transform).GetComponent<BadFood>();
                emptyTiles[random].AddFood(food);
                emptyTiles.RemoveAt(random);
                spawnedBadFoods.Add(food);

                currentBadFoodCount++;
            }
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    private List<int> questFoods = new();
    private void SpawnQuestFood()
    {
        if (!foodTypeInitialized)
        {
            return;
        }

        if (foodPrefabs.Count == 0)
        {
            return;
        }

        if (emptyTiles.Count == 0)
        {
            return;
        }

        questFoods = Quest_Manager.Instance.ActiveQuests[0].Quest.SelectedSpecificItems.ToList();
        EFoodType questFoodType = Quest_Manager.Instance.ActiveQuests[0].Quest.RequiredFoodType;

        switch (questFoodType)
        {
            case EFoodType.Fruit:
                foreach (var item in spawnedGoodFoods)
                {
                    int fruitId = (int)((Fruit)item).FruitType;
                    if (questFoodType == item.FoodType && questFoods.Contains(fruitId))
                    {
                        questFoods.Remove(fruitId);
                        break;
                    }
                }
                break;
            case EFoodType.FastFood:
                foreach (var item in spawnedGoodFoods)
                {
                    int fastFoodId = (int)((FastFood)item).FastFoodType;
                    if (questFoodType == item.FoodType && questFoods.Contains(fastFoodId))
                    {
                        questFoods.Remove(fastFoodId);
                        break;
                    }
                }
                break;
            case EFoodType.Cake:
                foreach (var item in spawnedGoodFoods)
                {
                    int cakeId = (int)((Cake)item).CakeType;
                    if (questFoodType == item.FoodType && questFoods.Contains(cakeId))
                    {
                        questFoods.Remove(cakeId);
                        break;
                    }
                }
                break;
        }

        for (int i = 0; i < questFoods.Count; i++)
        {
            int random = Random.Range(0, emptyTiles.Count);
            if (questFoodType == EFoodType.Fruit)
            {
                var foodPrefab = fruitsPrefabs.FirstOrDefault(f => (int)f.FruitType == questFoods[i]);
                var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab.gameObject, transform).GetComponent<GoodFood>();
                emptyTiles[random].AddFood(food);
                emptyTiles.RemoveAt(random);
                spawnedGoodFoods.Add(food);
                currentGoodFoodCount++;
            }
            else if (questFoodType == EFoodType.FastFood)
            {
                var foodPrefab = fastFoodPrefabs.FirstOrDefault(f => (int)f.FastFoodType == questFoods[i]);
                var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab.gameObject, transform).GetComponent<GoodFood>();
                emptyTiles[random].AddFood(food);
                emptyTiles.RemoveAt(random);
                spawnedGoodFoods.Add(food);
                currentGoodFoodCount++;
            }
            else if (questFoodType == EFoodType.Cake)
            {
                var foodPrefab = cakePrefabs.FirstOrDefault(f => (int)f.CakeType == questFoods[i]);
                var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab.gameObject, transform).GetComponent<GoodFood>();
                emptyTiles[random].AddFood(food);
                emptyTiles.RemoveAt(random);
                spawnedGoodFoods.Add(food);
                currentGoodFoodCount++;
            }
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
            case EFoodType.Cake:
                foods = cakePrefabs;
                break;
        }

        foreach (var tile in tiles)
        {
            var foodPrefab = foods[Random.Range(0, foods.Length)];
            var food = Main.Pool.Spawn(KEY_POOL.KEY_POOL_DEFAULT_CONTAINER, foodPrefab, transform);
            tile.AddFood(food);
            spawnedGoodFoods.Add((GoodFood)food);
        }
    }
    private void OnEndGame(object data)
    {
        StopCoroutine(nameof(IECheckSpawnFood));
    }

    private void OnRemoveGoodFood(object data)
    {
        if (data is FoodCollectionData collectionData)
        {
            spawnedGoodFoods.Remove(collectionData.food);
        }
        currentGoodFoodCount--;
    }
    private void OnRemoveBadFood(object data)
    {
        if (data is BadFood food)
        {
            spawnedBadFoods.Remove(food);
        }
        currentBadFoodCount--;
    }

    private void OnDestroy()
    {
        Main.Observer.Remove(EEvent.OnGoodFoodCollected, OnRemoveGoodFood);
        Main.Observer.Remove(EEvent.OnBadFoodCollected, OnRemoveBadFood);
        Main.Observer.Remove(EEvent.OnGameOver, OnEndGame);
    }
}
