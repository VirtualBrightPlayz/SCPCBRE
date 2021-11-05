using UnityEngine;

public class RMeshData
{
    public MeshData visibleData;
    public Mesh invisibleMesh;
    public RMTriggerBox[] triggerBoxes;
    public Mesh collisionMesh;
    public GameObject[] entities;
}

public class RMTriggerBox
{
    public Mesh mesh;
    public string name;
}