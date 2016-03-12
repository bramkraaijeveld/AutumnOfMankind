using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

    public int size;
    public float water;
    public float gravity;
    public float deltaTime;
    public int iterations;

    private Texture2D texture;
    private Texture2D texture2;

    private Matrix heightMap;
    private Matrix waterMap;

    public void Start() {
        heightMap = Matrix.Gaussian(size, Vector2.one);
        waterMap = Matrix.Ones(size) * water;

        Debug.Log(heightMap);
        Debug.Log(waterMap);

        texture = new Texture2D(128, 128);
        texture2 = new Texture2D(128, 128);

        StartCoroutine(playHydraulicErosion());
    }

    IEnumerator playHydraulicErosion() {
        foreach (Matrix m in Matrix.HydraulicErosion(heightMap, waterMap, 1, 1, gravity, deltaTime, iterations)) {
            texture = (m).ToTexture();
            texture2 = (m + heightMap).ToTexture();
            yield return null;
        }
        Debug.Log("DONE");
    }

    public void OnGUI() {
        int s = Mathf.Min(Screen.width, Screen.height);
        GUI.DrawTexture(new Rect(0, 0, s, s), texture);
        GUI.DrawTexture(new Rect(s, 0, s, s), texture2);
    }
}
