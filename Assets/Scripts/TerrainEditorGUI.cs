using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainEditorGUI : Editor
{
    public override void OnInspectorGUI()
    {
        

        DrawDefaultInspector();
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        // Check if realtime editing is enabled
        if (terrainGenerator.realtimeEditingSettings)
        {
            // Pick a random offset
            if (GUILayout.Button("Randomize Terrain"))
            {
                terrainGenerator.offsetX = Random.Range(0f, 9999f);
                terrainGenerator.offsetY = Random.Range(0f, 9999f);

                terrainGenerator.GenerateMap();
            }

            // Build the map with the current settings
            if (GUILayout.Button("Generate Map"))
            {
                terrainGenerator.GenerateMap();
            }

            // Save the settings while in play mode
            if (GUILayout.Button("Save Map"))
            {
                terrainGenerator.SaveMap(terrainGenerator.gameObject);
            }

            // Load the settings while in edit mode
            if (GUILayout.Button("Load Map"))
            {
                terrainGenerator.LoadMap(terrainGenerator.gameObject);
            }
        }
    }
}