using UnityEngine;
using System.Collections;

public class Geometry {
    public static Mesh Pyramid(float height, float radius, int n) {

        Vector3[] vertices = new Vector3[n * 3];

        for (int i=0; i<n; i++) {
            vertices[i * 3] = new Vector3(radius * Mathf.Cos(i * 2 * Mathf.PI / n), 0, radius * Mathf.Sin(i * 2 * Mathf.PI / n));
            vertices[i * 3 + 1] = new Vector3(0, height, 0);
            vertices[i * 3 + 2] = new Vector3(radius * Mathf.Cos((i + 1) * 2  * Mathf.PI / n), 0, radius * Mathf.Sin((i + 1) * 2 * Mathf.PI / n));
        }

        return flatMesh(vertices);
    }

    private static Mesh flatMesh(Vector3[] vertices) {
        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) triangles[i] = i;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.Optimize();
        return mesh;
    }
}
