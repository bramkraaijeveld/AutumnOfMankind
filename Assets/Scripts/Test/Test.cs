using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

    public int seed;
    public int size;
    public float height;
    public float persistance;

    private Texture2D texture;

    public void Start() { UpdateMatrix(); }
    //public void OnValidate() { UpdateMatrix(); }

    public void UpdateMatrix() {
        seed += 1;
        Matrix matrix = Matrix.DiamondSquare(seed, size, height, persistance);

        new ProceduralTerrain(matrix, 8);

        texture = matrix.ToTexture();
    }

    public void OnGUI() {
        GUI.DrawTexture(new Rect(0, 0, texture.width, texture.height), texture);
    }
}
