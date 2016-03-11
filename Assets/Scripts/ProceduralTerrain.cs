using UnityEngine;
using System.Collections;

public class ProceduralTerrain {

    private GameObject root;

    public ProceduralTerrain(Matrix heightMap, int partitions) {
        root = new GameObject("Procedural Terrain");

        Mesh[,] meshes = heightMap.ToMesh(partitions, true);

        for (int x =0; x<meshes.GetLength(0); x++) {
            for (int y=0; y<meshes.GetLength(1); y++) {
                CreateChunk(meshes[x, y], "["+x+", "+y+"]").transform.position = new Vector3(x, 0, y) * ((heightMap.Size - 1) / partitions);
            }
        }
    }

    public GameObject CreateChunk(Mesh mesh, string name) {
        GameObject g = new GameObject(name);
        g.AddComponent<MeshFilter>().mesh = mesh;
        g.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Terrain");
        g.transform.SetParent(root.transform);
        return g;
    }
}
