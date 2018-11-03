using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    public int depth = 20;
    public float groundLevel = 0.6f;
    public int width = 256;
    public int height = 256;
    public int scale = 20;

    public float offsetX = 100f;
    public float offsetY = 100f;

    public bool realtimeEditingSettings;

    // Use this for initialization
    void Start()
    {

        if (!realtimeEditingSettings)
        {
            Terrain terrain = GetComponent<Terrain>();
            terrain.terrainData = GenerateTerrain(terrain.terrainData);

            // Generate covers
            GetComponent<GeneratingObjects>().generateCover();

            // Apply the textures
            GetComponent<Grass>().paintTextures(terrain, terrain.terrainData);
        }
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);

        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float) x / width * scale + offsetX;
        float yCoord = (float) y / height * scale + offsetY;

        float newHeight = Mathf.PerlinNoise(xCoord, yCoord);
        if (newHeight < groundLevel)
        {
            newHeight = groundLevel;
        }
        return newHeight;
    }

    public void GenerateMap()
    {
        Terrain terrain = GetComponent<Terrain>();

        // Delete all objects
        foreach(GameObject gameObject in GameObject.FindGameObjectsWithTag("Cover")) {
            Destroy(gameObject);
        }
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Environment")) {
            Destroy(gameObject);
        }

        // Generate the terrain
        GenerateTerrain(terrain.terrainData);

        // Generate covers
        GetComponent<GeneratingObjects>().generateCover();

        // Apply the textures
        GetComponent<Grass>().paintTextures(terrain, terrain.terrainData);
    }

    public void SaveMap(GameObject saveObject)
    {
        // Save changes made in realtime editing
        List<string> saveData = new List<string>();

        saveData.Add(this.GetInstanceID().ToString());
        saveData.Add(depth.ToString());
        saveData.Add(groundLevel.ToString());
        saveData.Add(width.ToString());
        saveData.Add(height.ToString());
        saveData.Add(scale.ToString());
        saveData.Add(offsetX.ToString());
        saveData.Add(offsetY.ToString());

        System.IO.File.WriteAllLines(GetInstanceFileName(saveObject), saveData.ToArray());

    }

    public void LoadMap(GameObject loadObject)
    {
        string[] lines = System.IO.File.ReadAllLines(GetInstanceFileName(loadObject));
        if (lines.Length > 0)
        {
            depth = int.Parse(lines[1]);
            groundLevel = float.Parse(lines[2]);
            width = int.Parse(lines[3]);
            height = int.Parse(lines[4]);
            scale = int.Parse(lines[5]);
            offsetX = float.Parse(lines[6]);
            offsetY = float.Parse(lines[7]);
        }

        GenerateMap();
    }

    string GetInstanceFileName(GameObject baseObject)
    {
        return System.IO.Path.GetTempPath() + baseObject.name + "_" + baseObject.GetInstanceID() + ".keepTerrain.txt";
    }


}
