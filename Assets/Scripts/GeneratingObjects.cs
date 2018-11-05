using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratingObjects : MonoBehaviour {

    [Header("Available tags: Cover, Environment")]

    public List<GameObject> allGameObjects;
    public int[] frequencyPerGameObject;

    [Tooltip("An array of allowed rotations that will be randomly chosen for each cover. If no angles are specified a random number between 0 and 360 will be chosen.")]
    public int[] allowedRotationAgles = { 0, 45, 90 };

    [Tooltip("The maximum allowed steepness of a slope on which a cover can be placed.")]
    [Header("Slope setting")]
    public int maxSlopeAngle;

    [Tooltip("Any height difference that is greater than this number, will be seen as an edge (which means the cover will reposition to not hang over the edge).")]
    public float slopeHeightDifferenceMargin;

    [Header("Cover settings")]
    public float coverXOffset;
    public float coverZOffset;

    private Terrain terrain;

    public void generateCover () {
        if (maxSlopeAngle > 360) {
            maxSlopeAngle = 360;
        } else if (maxSlopeAngle < 0) {
            maxSlopeAngle = 0;
        }

		if (allGameObjects.Count > 0) {
            terrain = GetComponent<Terrain>();

            foreach (GameObject curGameObject in allGameObjects) {
                Renderer curGameObjectRenderer = curGameObject.GetComponent<Renderer>();
                float curGameObjectWidth = curGameObjectRenderer.bounds.size.x;
                float curGameObjectDepth = curGameObjectRenderer.bounds.size.z;

                print(curGameObject.name + ": " + curGameObjectWidth + "     " + curGameObjectDepth);

                switch (curGameObject.tag) {
                    case "Cover":
                        float biggestObjectRadius = (curGameObjectWidth >= curGameObjectDepth ? curGameObjectWidth : curGameObjectDepth) / 2;

                        for (float x = biggestObjectRadius; x < terrain.terrainData.size.x; x += curGameObjectWidth + coverXOffset) {
                            for (float z = biggestObjectRadius; z < terrain.terrainData.size.z - biggestObjectRadius + curGameObjectRenderer.bounds.extents.z; z += curGameObjectDepth + coverZOffset) {
                                if (Random.Range(0, 100) < frequencyPerGameObject[allGameObjects.IndexOf(curGameObject)]) {
                                    GameObject curInstantiatedCover = Instantiate(curGameObject);
                                    curInstantiatedCover.name = "Cover" + x + "_" + z;

                                    // Set random rotation of the cover
                                    Vector3 generatedRotation = generateCoverRotation();
                                    curInstantiatedCover.transform.Rotate(generatedRotation);

                                    // Set position of the cover
                                    curInstantiatedCover.transform.position = generateObjectPosition(curGameObjectRenderer, x, z);
                                    generateCoverPosition(curInstantiatedCover, curGameObjectRenderer, x, z);

                                    // Check if the cover will be placed on a slope
                                    float steepness = terrain.terrainData.GetSteepness(
                                        curInstantiatedCover.transform.position.x / terrain.terrainData.size.x,
                                        curInstantiatedCover.transform.position.z / terrain.terrainData.size.z
                                    );

                                    // Allign cover with the rotation of the terrain
                                    if (steepness > 0 && steepness < maxSlopeAngle) {
                                        Vector3 terrainRotation = terrain.terrainData.GetInterpolatedNormal(
                                            curInstantiatedCover.transform.position.x / terrain.terrainData.size.x,
                                            curInstantiatedCover.transform.position.z / terrain.terrainData.size.z
                                        );

                                        Vector3 yRotation = new Vector3(0, curInstantiatedCover.transform.rotation.eulerAngles.y, 0);

                                        Quaternion rotationDifference = Quaternion.FromToRotation(Vector3.up, terrainRotation);

                                        curInstantiatedCover.transform.rotation = new Quaternion(rotationDifference.x, 0, rotationDifference.z, rotationDifference.w);
                                        curInstantiatedCover.transform.Rotate(yRotation);
                                    }

                                    // If the angle of the slope the object will be placed on is greater than the set maximum slope angle, the object will be destroyed
                                    if (steepness > maxSlopeAngle) {
                                        Destroy(curInstantiatedCover);
                                    }
                                }
                            }
                        }
                        break;

                    case "Environment":
                        for (float x = curGameObjectWidth; x < terrain.terrainData.size.x; x += 5) {
                            for (float z = curGameObjectDepth; z < terrain.terrainData.size.z; z += 5) {
                                if (Random.Range(0, 100) < frequencyPerGameObject[allGameObjects.IndexOf(curGameObject)]) {
                                    GameObject curInstantiatedGameObject = Instantiate(curGameObject);

                                    // Set position of the gameobject
                                    Vector3 generatedPosition = generateObjectPosition(curGameObjectRenderer, x, z);

                                    generatedPosition = new Vector3(generatedPosition.x, 
                                        generatedPosition.y - curInstantiatedGameObject.GetComponent<Renderer>().bounds.extents.x,
                                        generatedPosition.z);

                                    // Apply generated position and rotation to the gameobject
                                    curInstantiatedGameObject.transform.position = generatedPosition;
                                }
                            }
                        }
                        break;
                }
            }
        }
	}

    private Vector3 generateObjectPosition(Renderer renderer, float x, float z) {
        Vector3 generatedPosition = Vector3.zero;

        float curObjectVerticalRadius = renderer.bounds.extents.y;
        float curTerrainPointHeight = terrain.SampleHeight(new Vector3(x, 0, z));
        float coverPlacementHeight = curTerrainPointHeight + curObjectVerticalRadius;
        generatedPosition = new Vector3(x, coverPlacementHeight, z);

        return generatedPosition;
    }

    private void generateCoverPosition(GameObject curCover, Renderer coverRenderer, float x, float z) {
        Vector3 curCoverPosition = curCover.transform.position;

        // Check if gameobject is 'hanging' over a ledge
        Vector3 leftSideRenderer = new Vector3(curCoverPosition.x - coverRenderer.bounds.extents.x, 0, curCoverPosition.z);
        Vector3 rightSideRenderer = new Vector3(curCoverPosition.x + coverRenderer.bounds.extents.x, 0, curCoverPosition.z);
        Vector3 topSideRenderer = new Vector3(curCoverPosition.x, 0, curCoverPosition.z + coverRenderer.bounds.extents.z);
        Vector3 bottomSideRenderer = new Vector3(curCoverPosition.x, 0, curCoverPosition.z - coverRenderer.bounds.extents.z);
        
        float terrainHeightOnCurrentPos = terrain.SampleHeight(curCoverPosition) - slopeHeightDifferenceMargin;

        if (terrain.SampleHeight(leftSideRenderer) < terrainHeightOnCurrentPos) {
            //adjustedPosition += new Vector3(renderer.bounds.extents.x, 0, 0);
            curCover.transform.Translate(new Vector3(coverRenderer.bounds.extents.x, 0, 0));
        }

        if (terrain.SampleHeight(rightSideRenderer) < terrainHeightOnCurrentPos) {
            //adjustedPosition -= new Vector3(renderer.bounds.extents.x, 0, 0);
            curCover.transform.Translate(new Vector3(-coverRenderer.bounds.extents.x, 0, 0));
        }

        if (terrain.SampleHeight(topSideRenderer) < terrainHeightOnCurrentPos) {
            //adjustedPosition -= new Vector3(0, 0, renderer.bounds.extents.z);
            curCover.transform.Translate(new Vector3(0, 0, -coverRenderer.bounds.extents.z));
        }

        if (terrain.SampleHeight(bottomSideRenderer) < terrainHeightOnCurrentPos) {
            //adjustedPosition += new Vector3(0, 0, renderer.bounds.extents.z);
            curCover.transform.Translate(new Vector3(0, 0, coverRenderer.bounds.extents.z));
        }

        curCover.transform.position = new Vector3(
            curCover.transform.position.x, 
            terrain.SampleHeight(curCover.transform.position) + coverRenderer.bounds.extents.y, 
            curCover.transform.position.z
        );
    }

    private Vector3 generateCoverRotation() {
        Vector3 generatedRotation;
        if (allowedRotationAgles.Length > 0) {
            generatedRotation  = new Vector3(0, allowedRotationAgles[Random.Range(0, allowedRotationAgles.Length)], 0);
        } else {
            generatedRotation = new Vector3(0, Random.Range(0, 360), 0);
        }

        return generatedRotation;
    }

    float getSlopeAngle(Vector3 surfNormal) {
        return Vector3.Angle(surfNormal, Vector3.up);
    }
}
