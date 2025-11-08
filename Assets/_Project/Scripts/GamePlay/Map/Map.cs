using System.Collections.Generic;
using HP.Utils;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform tileHolder;
    [SerializeField] private FoodSpawner foodSpawner;

    [Header("Map Settings")]
    [SerializeField] private GameObject[] tilePrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float tileSize;

    private List<GameObject> spawnedTiles = new List<GameObject>();
    public static Map Instance => SingletonManager.Instance.Get<Map>();

    private void Awake()
    {
        SingletonManager.Instance.RegisterScene(this);
    }
    private void Start()
    {
        // SpawnTiles();
    }

#if UNITY_EDITOR
    [Button]
    private void SpawnTiles()
    {
        Vector2 startPos = GeneralUtils.SetUpStartSpawnPosOfGrid(width, height, tileSize, tileSize);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 spawnPos = new Vector3(startPos.x + x * tileSize, 0, startPos.y - y * tileSize);
                var spawn = Instantiate(tilePrefab[Random.Range(0, tilePrefab.Length)], tileHolder);
                spawnedTiles.Add(spawn);
                spawn.transform.position = spawnPos;
                spawn.name = $"Tile_{x}_{y}";
            }
        }
    }
    [Button]
    private void ClearTiles()
    {
        foreach (var tile in spawnedTiles)
        {
            DestroyImmediate(tile);
        }
        for (int i = tileHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(tileHolder.GetChild(i).gameObject);
        }
        spawnedTiles.Clear();
    }
#endif
}
