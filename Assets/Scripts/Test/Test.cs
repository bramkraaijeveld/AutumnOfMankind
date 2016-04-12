using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

    public int textureSize;
    public float terrainHeight;
    public float terrainFalloff;
    public float terrainScale;
    public float rainAmount;
    public float evaporation;
    public float maxSedimentCapacity;
    public float maxErosionDepth;
    public float dissolveSpeed;
    public float depositionSpeed;
    public float tiltlim;
    public float pipeArea;
    public float deltaTime;
    public int cycles;
    public int iterations;
    public Gradient gradient;
    public AnimationCurve mapping;

    private Matrix heightMap;
    private Matrix soilMap;

    public void Start() {
        heightMap = Matrix.Perlin(textureSize, terrainHeight, terrainFalloff, terrainScale, new Vector4(Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000)));
        soilMap = Matrix.Ones(textureSize) * rainAmount;

        StartCoroutine(PlayErosion());
    }

    IEnumerator PlayErosion() {
        ProceduralTerrain terrain = new ProceduralTerrain(heightMap + soilMap, Resources.Load<Material>("Terrain"));

        int i = 0;
        foreach (Matrix[] update in Erosion.Hydraulic(heightMap, soilMap, pipeArea, 1, maxSedimentCapacity,  maxErosionDepth, dissolveSpeed, depositionSpeed, evaporation, tiltlim, deltaTime, cycles, iterations)) {
            terrain.Update(update[0] + update[1], update[2].ToTexture(gradient, mapping));
            Debug.Log(++i);
            if (i == iterations) {
                terrain.Update(update[0], update[2].ToTexture(gradient, mapping));
            }
            yield return null;
        }
        Debug.Log("DONE");
    }
}
