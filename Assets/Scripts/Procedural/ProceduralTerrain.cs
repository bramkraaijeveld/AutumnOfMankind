using UnityEngine;
using System.Collections;

public class ProceduralTerrain {

    private int chunkSize;
    private int nChunks;

    private GameObject root;

    private Material material;

    private GameObject[,] chunks;

    public ProceduralTerrain(Matrix heightMap, Material material, Texture2D texture = null, int chunkSize = 64) {
        this.root = new GameObject("Procedural Terrain");
        this.material = material;
        this.material.mainTexture = texture;
        this.chunkSize = chunkSize;
        nChunks = heightMap.Size / chunkSize;

        chunks = GenerateChunks(GenerateMeshes(heightMap));
    }

    public void Update(Matrix heightMap, Texture2D texture) {
        Update(heightMap);
        material.mainTexture = texture;
    }

    public void Update(Matrix heightMap) {
        for (int cx = 0; cx < nChunks; cx++) {
            for (int cy = 0; cy < nChunks; cy++) {

                Vector3[] vertices = new Vector3[chunkSize * chunkSize * 6];
                int n = 0;

                for (int x = 0; x < chunkSize; x++) {
                    for (int y = 0; y < chunkSize; y++) {
                        vertices[n] = new Vector3(x, heightMap[cx * chunkSize + x, cy * chunkSize + y], y);
                        vertices[n + 1] = new Vector3(x + 1, heightMap[cx * chunkSize + x + 1, cy * chunkSize + y + 1], y + 1);
                        vertices[n + 2] = new Vector3(x + 1, heightMap[cx * chunkSize + x + 1, cy * chunkSize + y], y);

                        vertices[n + 3] = new Vector3(x, heightMap[cx * chunkSize + x, cy * chunkSize + y], y);
                        vertices[n + 4] = new Vector3(x, heightMap[cx * chunkSize + x, cy * chunkSize + y + 1], y + 1);
                        vertices[n + 5] = new Vector3(x + 1, heightMap[cx * chunkSize + x + 1, cy * chunkSize + y + 1], y + 1);

                        n += 6;
                    }
                }

                Mesh mesh = chunks[cx, cy].GetComponent<MeshFilter>().mesh;
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
            }
        }
    }

    private GameObject[,] GenerateChunks(Mesh[,] meshes) {
        GameObject[,] chunks = new GameObject[meshes.GetLength(0), meshes.GetLength(1)];

        for (int x = 0; x < meshes.GetLength(0); x++) {
            for (int y = 0; y < meshes.GetLength(1); y++) {
                chunks[x, y] = new GameObject("(" + x + ", " + y + ")");
                chunks[x, y].AddComponent<MeshFilter>().mesh = meshes[x, y];
                chunks[x, y].AddComponent<MeshRenderer>().material = material;
                chunks[x, y].transform.parent = root.transform;
                chunks[x, y].transform.position = new Vector3(x * chunkSize, 0, y * chunkSize);
            }
        }

        return chunks;
    }

    private Mesh[,] GenerateMeshes(Matrix heightMap) {
        Mesh[,] meshes = new Mesh[nChunks, nChunks];

        for (int cx = 0; cx < nChunks; cx++) {
            for (int cy = 0; cy < nChunks; cy++) {

                Vector3[] vertices = new Vector3[chunkSize * chunkSize * 6];
                Vector2[] uvs = new Vector2[chunkSize * chunkSize * 6];
                int[] triangles = new int[chunkSize * chunkSize * 6];

                int n = 0;
                for (int x = 0; x < chunkSize; x++) {
                    for (int y = 0; y < chunkSize; y++) {
                        vertices[n] = new Vector3(x, heightMap[cx * chunkSize + x, cy * chunkSize + y], y);
                        vertices[n + 1] = new Vector3(x + 1, heightMap[cx * chunkSize + x + 1, cy * chunkSize + y + 1], y + 1);
                        vertices[n + 2] = new Vector3(x + 1, heightMap[cx * chunkSize + x + 1, cy * chunkSize + y], y);

                        vertices[n + 3] = new Vector3(x, heightMap[cx * chunkSize + x, cy * chunkSize + y], y);
                        vertices[n + 4] = new Vector3(x, heightMap[cx * chunkSize + x, cy * chunkSize + y + 1], y + 1);
                        vertices[n + 5] = new Vector3(x + 1, heightMap[cx * chunkSize + x + 1, cy * chunkSize + y + 1], y + 1);

                        uvs[n] = new Vector2(cx * chunkSize + x, cy * chunkSize + y) / heightMap.Size;
                        uvs[n + 1] = new Vector2(cx * chunkSize + x, cy * chunkSize + y) / heightMap.Size;
                        uvs[n + 2] = new Vector2(cx * chunkSize + x, cy * chunkSize + y) / heightMap.Size;

                        uvs[n + 3] = new Vector2(cx * chunkSize + x, cy * chunkSize + y) / heightMap.Size;
                        uvs[n + 4] = new Vector2(cx * chunkSize + x, cy * chunkSize + y) / heightMap.Size;
                        uvs[n + 5] = new Vector2(cx * chunkSize + x, cy * chunkSize + y) / heightMap.Size;

                        n += 6;
                    }
                }

                for (int i = 0; i < triangles.Length; i++) triangles[i] = i;

                meshes[cx, cy] = new Mesh();
                meshes[cx, cy].vertices = vertices;
                meshes[cx, cy].triangles = triangles;
                meshes[cx, cy].uv = uvs;
                meshes[cx, cy].RecalculateNormals();
                meshes[cx, cy].MarkDynamic();
            }
        }

        return meshes;
    }
}
