using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

    public int textureSize;
    public float terrainHeight;
    public float terrainFalloff;
    public float terrainScale;
    public float soilHeight;
    public float talusAngle;
    public float pipeArea;
    public float deltaTime;
    public int cycles;
    public int iterations;

    private Matrix heightMap;
    private Matrix soilMap;

    public void Start() {
        heightMap = Matrix.Perlin(textureSize, terrainHeight, terrainFalloff, terrainScale, new Vector4(Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000)));
        soilMap = Matrix.Ones(textureSize) * soilHeight;

        StartCoroutine(playThermalErosion());
    }

    IEnumerator playThermalErosion() {
        ProceduralTerrain terrain = new ProceduralTerrain(heightMap + soilMap, Resources.Load<Material>("Terrain"));

        int i = 0;
        foreach (Matrix soil in Erosion.Thermal(heightMap, soilMap, talusAngle, pipeArea, 1, deltaTime, cycles, iterations)) {
            terrain.Update(heightMap + soil);
            Debug.Log(++i);
            yield return null;
        }
        Debug.Log("DONE");
    }
}
