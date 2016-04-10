using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class ProceduralTree : ProceduralObject {

    public override Mesh Mesh { get { return mesh; } }

    private Mesh mesh;

    public ProceduralTree(int n = 5, float height = 2, float radius = 0.5f) {
        mesh = Geometry.Pyramid(height, radius, n);
        mesh.name = "Procedural Tree";
    }
}
