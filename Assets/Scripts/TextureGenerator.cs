using UnityEngine;
using System.Collections;
using System.Linq;

public class TextureGenerator : MonoBehaviour {

    public int[] textureHeight;
    public int textureMergeThreshold;

    public void paintTextures(Terrain terrain, TerrainData terrainData) {

        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++) {
            for (int x = 0; x < terrainData.alphamapWidth; x++) {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;

                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

                float[] splatWeights = new float[terrainData.alphamapLayers];

                for (int i=0; i < splatWeights.Length; i++) {
                    int previousIndex = i - 1;

                    if (i == splatWeights.Length - 1) {
                        if (height >= textureHeight[previousIndex]) {
                            splatWeights[i] = 1;
                        } else {
                            splatWeights[i] = 0;
                        }
                    }
                    else {
                        if (previousIndex > -1) {
                            if (height <= textureHeight[i] && height > textureHeight[previousIndex]) {
                                splatWeights[i] = 1;
                            } else {
                                splatWeights[i] = 0;
                            }
                        }
                        else {
                            splatWeights[i] = (height <= textureHeight[i]) ? 1 : 0;
                        }
                    }
                }


                float z = splatWeights.Sum();

                for (int i = 0; i < terrainData.alphamapLayers; i++) {
                    splatWeights[i] /= z;
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}