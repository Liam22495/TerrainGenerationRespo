using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
    {
    public Terrain terrain;
    public int terrainWidth;
    public int terrainLength;
    public int terrainHeight;
    public float scale = 20f;
    public Texture2D grassTexture;
    public TerrainLayer grassLayer;
    private List<Vector3> pathPositions = new List<Vector3>();
    public GameObject playerPrefab;

    public int pathLength; // Length of the path in terms of waypoints or segments
    public float pathWidth; // Width of the path
    public GameObject waterPrefab; // Blue-colored sphere prefab
    public GameObject cloudPrefab; // White-colored sphere prefab
    public GameObject treePrefab;
    public GameObject bushPrefab;
    public GameObject rockPrefab;
    public GameObject FlowerPrefab;
    public GameObject FlowerPrefabBlue;
    public int numberOfItems;
    public int numberOfWaterBodies;
    public int numberOfClouds;

    void Start()
        {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        ApplyTexture();
        GenerateContinuousPathOnTerrain();
        PlantItems();
        CreateWaterBodies();
        CreateClouds();
        CreateRain();
        SpawnPlayerOnPath();
        }

    void GenerateContinuousPathOnTerrain()
        {
        Vector3 startPosition = new Vector3(0, 0, 0); // Start of the path
        Vector3 endPosition = new Vector3(terrainWidth, 0, terrainLength); // End of the path

        // Example of a straight line path from start to end
        for (float t = 0; t <= 1; t += 0.01f)
            {
            Vector3 pathPosition = Vector3.Lerp(startPosition, endPosition, t);
            pathPosition.y = terrain.SampleHeight(pathPosition); // Adjust height to match terrain
            FlattenTerrainForPath(pathPosition);
            pathPositions.Add(pathPosition); // Add to path positions list
            }
        }

    void SpawnPlayerOnPath()
        {
        if (playerPrefab != null && pathPositions.Count > 0)
            {
            int randomIndex = Random.Range(0, pathPositions.Count);
            Vector3 spawnPosition = pathPositions[randomIndex];
            Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            }
        }

    void FlattenTerrainForPath(Vector3 pathPosition)
        {
        int heightmapResolution = terrain.terrainData.heightmapResolution;

        // Calculate the position and width in terrain's heightmap resolution
        int terrainPosX = Mathf.RoundToInt((pathPosition.x / terrainWidth) * heightmapResolution);
        int terrainPosZ = Mathf.RoundToInt((pathPosition.z / terrainLength) * heightmapResolution);
        int pathWidthInTerrain = Mathf.RoundToInt((pathWidth / Mathf.Max(terrainWidth, terrainLength)) * heightmapResolution);

        // Ensure we don't go out of bounds
        int xBase = Mathf.Clamp(terrainPosX - pathWidthInTerrain / 2, 0, heightmapResolution - pathWidthInTerrain);
        int yBase = Mathf.Clamp(terrainPosZ - pathWidthInTerrain / 2, 0, heightmapResolution - pathWidthInTerrain);

        // Get the heights in the desired area
        float[,] heights = terrain.terrainData.GetHeights(xBase, yBase, pathWidthInTerrain, pathWidthInTerrain);

        // Flatten the area
        for (int x = 0; x < pathWidthInTerrain; x++)
            {
            for (int y = 0; y < pathWidthInTerrain; y++)
                {
                heights[x, y] = 0; // Set to desired height level (0 for flat)
                }
            }

        // Apply the modified heights back to the terrain
        terrain.terrainData.SetHeights(xBase, yBase, heights);
        }

    void PlantItems()
        {
        List<GameObject> prefabs = new List<GameObject>
        {
            treePrefab,
            bushPrefab,
            rockPrefab,
            FlowerPrefabBlue,
            FlowerPrefab,
        };

        for (int i = 0; i < numberOfItems; i++)
            {
            GameObject itemToPlant = prefabs[Random.Range(0, prefabs.Count)];

            float x = Random.Range(0, terrainWidth);
            float z = Random.Range(0, terrainLength);
            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            Vector3 position = new Vector3(x, y, z);

            GameObject item = Instantiate(itemToPlant, position, Quaternion.identity);
            item.transform.SetParent(transform);
            }
        }

    void CreateWaterBodies()
        {
        for (int i = 0; i < numberOfWaterBodies; i++)
            {
            Vector3 waterPosition = GetRandomPositionOnTerrain();
            waterPosition.y = terrain.SampleHeight(waterPosition) - 0.5f; // Adjust water level

            GameObject waterBody = Instantiate(waterPrefab, waterPosition, Quaternion.identity);
            waterBody.transform.localScale = new Vector3(10, 1, 10); // Adjust the size of the water body
            }
        }

    void CreateClouds()
        {
        float highestPoint = GetHighestPoint() + 30; // 30 units above the highest terrain point

        for (int i = 0; i < numberOfClouds; i++)
            {
            Vector3 cloudPosition = GetRandomPositionOnTerrain();
            cloudPosition.y = highestPoint + Random.Range(10f, 20f); // Randomize height a bit

            float randomScale = Random.Range(3f, 10f); // Randomize cloud size
            GameObject cloud = Instantiate(cloudPrefab, cloudPosition, Quaternion.identity);
            cloud.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            }
        }

    void CreateRain()
        {
        GameObject rainSystem = new GameObject("Rain");
        rainSystem.transform.position = new Vector3(terrainWidth / 2, 30, terrainLength / 2); // Centered over terrain

        ParticleSystem rain = rainSystem.AddComponent<ParticleSystem>();
        rainSystem.transform.rotation = Quaternion.Euler(0, 0, 0); // Ensure the system is not rotated

        var main = rain.main;
        main.loop = true;
        main.startSpeed = -10f;
        main.startSize = 0.2f;
        main.startLifetime = 2f;
        main.maxParticles = 5000;
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Use world space
        main.gravityModifier = 2f;

        var emission = rain.emission;
        emission.rateOverTime = 1000;

        var shape = rain.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(terrainWidth, 1, terrainLength);

        var velocityOverLifetime = rain.velocityOverLifetime;
        velocityOverLifetime.enabled = false; // Disable any velocity over lifetime

        var limitVelocityOverLifetime = rain.limitVelocityOverLifetime;
        limitVelocityOverLifetime.enabled = false; // Disable any limiting velocity

        ParticleSystemRenderer renderer = rain.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        }

    TerrainData GenerateTerrain(TerrainData terrainData)
        {
        // Ensure that heightmap resolution is a power of two plus one for correct detail mapping
        int heightmapResolution = Mathf.NextPowerOfTwo(Mathf.Max(terrainWidth, terrainLength)) + 1;
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

        // Set detail resolution based on the heightmap resolution
        // The second parameter '16' is the resolution per patch, which you can adjust as needed
        terrainData.SetDetailResolution(heightmapResolution, 16);

        // Generate the heights using your existing method
        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
        }

    float GetHighestPoint()
        {
        float highestPoint = 0f;
        for (int x = 0; x < terrain.terrainData.heightmapResolution; x++)
            {
            for (int y = 0; y < terrain.terrainData.heightmapResolution; y++)
                {
                highestPoint = Mathf.Max(highestPoint, terrain.terrainData.GetHeight(x, y));
                }
            }
        return highestPoint;
        }

    Vector3 GetRandomPositionOnTerrain()
    {
        float x = Random.Range(0, terrainWidth);
        float z = Random.Range(0, terrainLength);
        float y = terrain.SampleHeight(new Vector3(x, 0, z));
        return new Vector3(x, y, z);
    }


    void ApplyTexture()
        {
        // Check if grassLayer is not null
        if (grassLayer == null)
            {
            Debug.LogError("Terrain Layer is not assigned.");
            return;
            }

        grassLayer.diffuseTexture = grassTexture; // Assign grass texture
        grassLayer.tileSize = new Vector2(15, 15); // Adjust the tile size as needed

        // Assign the Terrain Layer
        terrain.terrainData.terrainLayers = new TerrainLayer[] { grassLayer };
        }

    



    float[,] GenerateHeights()
        {
        float[,] heights = new float[terrainWidth, terrainLength];
        for (int x = 0; x < terrainWidth; x++)
            {
            for (int y = 0; y < terrainLength; y++)
                {
                heights[x, y] = CalculateHeight(x, y);
                }
            }
        return heights;
        }

    float CalculateHeight(int x, int y)
        {
        float xCoord = (float)x / terrainWidth * scale;
        float yCoord = (float)y / terrainLength * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
        }
    }
