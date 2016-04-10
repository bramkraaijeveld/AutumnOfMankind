using UnityEngine;

[System.Serializable]
public abstract class ProceduralObject {
    public abstract Mesh Mesh { get; }

    public GameObject GameObject() {
        GameObject g = new GameObject(Mesh.name);
        g.AddComponent<MeshFilter>().mesh = Mesh;
        g.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Default");
        return g;
    }
}
