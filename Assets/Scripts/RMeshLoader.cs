using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class RMeshLoader : MonoBehaviour
{
    public string filename;

    void Start()
    {
        RMeshData data = LoadRMesh(filename);
        GetComponent<MeshRenderer>().sharedMaterials = data.visibleData.materials;
        GetComponent<MeshFilter>().sharedMesh = data.visibleData.mesh;
        GetComponent<MeshCollider>().sharedMesh = data.collisionMesh;
        for (int i = 0; i < data.triggerBoxes.Length; i++)
        {
            GameObject go = new GameObject(data.triggerBoxes[i].name);
            MeshCollider triggerCollider = go.AddComponent<MeshCollider>();
            triggerCollider.sharedMesh = data.triggerBoxes[i].mesh;
            go.transform.SetParent(transform);
        }
    }

    public static RMeshData LoadRMesh(string filename)
    {
        using (FileStream stream = File.Open(filename, FileMode.Open))
        {
            bool hasTriggerBox = false;
            string header = ReadString(stream);
            if (header == "RoomMesh")
            {
            }
            else if (header == "RoomMesh.HasTriggerBox")
            {
                hasTriggerBox = true;
            }
            else
            {
                return null;
            }
            string path = Path.GetDirectoryName(filename);
            List<Material> materials = new List<Material>();
            List<CombineInstance> combines = new List<CombineInstance>();
            int count = ReadInt(stream);
            for (int i = 0; i < count; i++)
            {
                string[] tex = new string[2];
                for (int j = 0; j < 2; j++)
                {
                    byte temp1b = ReadByte(stream);
                    // if (temp1b != 0)
                    {
                        string temp1s = ReadString(stream);
                        tex[j] = Path.Combine(path, temp1s);
                    }
                }

                Material mat = LoadMaterial(tex);
                materials.Add(mat);

                // verts
                int count2 = ReadInt(stream);
                Vector3[] vertices = new Vector3[count2];
                Vector2[] uv = new Vector2[count2];
                Vector2[] uv2 = new Vector2[count2];
                Color32[] colors = new Color32[count2];

                for (int j = 0; j < count2; j++)
                {
                    // world coords
                    float x = ReadFloat(stream);
                    float y = ReadFloat(stream);
                    float z = ReadFloat(stream);
                    vertices[j] = new Vector3(x, y, z);
                    
                    // texture coords uv
                    float u = ReadFloat(stream);
                    float v = 1f - ReadFloat(stream);
                    uv[j] = new Vector2(u, v);

                    // texture coords uv2
                    float u2 = ReadFloat(stream);
                    float v2 = 1f - ReadFloat(stream);
                    uv2[j] = new Vector2(u2, v2);

                    // colors
                    byte r = ReadByte(stream);
                    byte g = ReadByte(stream);
                    byte b = ReadByte(stream);
                    colors[j] = new Color32(r, g, b, 255);
                }

                // tris
                count2 = ReadInt(stream);
                int[] tris = new int[count2 * 3];

                for (int j = 0; j < count2; j++)
                {
                    tris[j * 3] = ReadInt(stream);
                    tris[j * 3 + 1] = ReadInt(stream);
                    tris[j * 3 + 2] = ReadInt(stream);
                }

                Mesh tempmesh = new Mesh();
                tempmesh.vertices = vertices;
                tempmesh.triangles = tris;
                tempmesh.uv = uv;
                tempmesh.uv2 = uv2;
                tempmesh.colors32 = colors;
                tempmesh.RecalculateNormals();
                tempmesh.RecalculateTangents();
                combines.Add(new CombineInstance()
                {
                    mesh = tempmesh,
                    transform = Matrix4x4.identity
                });
            }
            Mesh mesh = new Mesh();
            mesh.name = Path.GetFileNameWithoutExtension(filename);
            mesh.CombineMeshes(combines.ToArray(), false, true, false);
            combines.Clear();

            // invisible collision mesh
            count = ReadInt(stream);

            for (int i = 0; i < count; i++)
            {
                // vertices
                int count2 = ReadInt(stream);
                Vector3[] vertices = new Vector3[count2];

                for (int j = 0; j < count2; j++)
                {
                    // world coords
                    float x = ReadFloat(stream);
                    float y = ReadFloat(stream);
                    float z = ReadFloat(stream);
                    vertices[j] = new Vector3(x, y, z);
                }

                // tris
                count2 = ReadInt(stream);
                int[] tris = new int[count2 * 3];

                for (int j = 0; j < count2; j++)
                {
                    tris[j * 3] = ReadInt(stream);
                    tris[j * 3 + 1] = ReadInt(stream);
                    tris[j * 3 + 2] = ReadInt(stream);
                }

                Mesh tempmesh = new Mesh();
                tempmesh.vertices = vertices;
                tempmesh.triangles = tris;
                combines.Add(new CombineInstance()
                {
                    mesh = tempmesh,
                    transform = Matrix4x4.identity
                });
            }
            Mesh invisMesh = new Mesh();
            invisMesh.name = Path.GetFileNameWithoutExtension(filename);
            invisMesh.CombineMeshes(combines.ToArray(), false, true, false);
            combines.Clear();

            // trigger box
            List<RMTriggerBox> boxes = new List<RMTriggerBox>();
            if (hasTriggerBox)
            {
                count = ReadInt(stream);
                for (int i = 0; i < count; i++)
                {
                    int count2 = ReadInt(stream);
                    for (int j = 0; j < count2; j++)
                    {
                        // vertices
                        int count3 = ReadInt(stream);
                        Vector3[] vertices = new Vector3[count3];
                        for (int k = 0; k < count3; k++)
                        {
                            // world coords
                            float x = ReadFloat(stream);
                            float y = ReadFloat(stream);
                            float z = ReadFloat(stream);
                            vertices[j] = new Vector3(x, y, z);
                        }

                        // tris
                        count3 = ReadInt(stream);
                        int[] tris = new int[count3 * 3];

                        for (int k = 0; k < count3; k++)
                        {
                            tris[j * 3] = ReadInt(stream);
                            tris[j * 3 + 1] = ReadInt(stream);
                            tris[j * 3 + 2] = ReadInt(stream);
                        }

                        Mesh tempmesh = new Mesh();
                        tempmesh.vertices = vertices;
                        tempmesh.triangles = tris;
                        combines.Add(new CombineInstance()
                        {
                            mesh = tempmesh,
                            transform = Matrix4x4.identity
                        });
                    }
                    string triggerName = ReadString(stream);
                    RMTriggerBox box = new RMTriggerBox()
                    {
                        mesh = new Mesh(),
                        name = triggerName
                    };
                    box.mesh.CombineMeshes(combines.ToArray(), false, true, false);
                    combines.Clear();
                    boxes.Add(box);
                }
            }

            RMeshData final = new RMeshData();
            final.visibleData = new MeshData(mesh, materials.ToArray());
            final.invisibleMesh = invisMesh;
            final.triggerBoxes = boxes.ToArray();
            final.collisionMesh = new Mesh();
            for (int j = 0; j < mesh.subMeshCount; j++)
            {
                combines.Add(new CombineInstance()
                {
                    mesh = mesh,
                    transform = Matrix4x4.identity,
                    subMeshIndex = j
                });
            }
            combines.Add(new CombineInstance()
            {
                mesh = invisMesh,
                transform = Matrix4x4.identity
            });
            final.collisionMesh.CombineMeshes(combines.ToArray(), false, true, false);

            return final;
        }
    }

    public static Material LoadMaterial(string[] tex)
    {
        Material mat = new Material(Resources.Load<Material>("RoomMaterial"));
        mat.SetTexture("_MainTex", LoadTexture(tex[1]));
        mat.SetTexture("_BumpMap", LoadTextureBump(tex[1]));
        mat.SetTexture("_AltTex", LoadTexture(tex[0]));
        mat.name = Path.GetFileNameWithoutExtension(tex[1]);
        return mat;
    }

    public static string GetFileNameIgnoreCase(string filename)
    {
        if (File.Exists(filename))
        {
            return filename;
        }
        string file = Path.GetFileName(filename);
        string dir = Path.GetDirectoryName(filename);
        foreach (string item in Directory.GetFiles(dir))
        {
            if (Path.GetFileName(item).ToLower() == file.ToLower())
            {
                return Path.Combine(dir, item);
            }
        }
        return filename;
    }

    public static Texture2D LoadTexture(string path)
    {
        path = GetFileNameIgnoreCase(path);
        if (File.Exists(path))
        {
            Texture2D tex = new Texture2D(1, 1);
            if (tex.LoadImage(File.ReadAllBytes(path)))
            {
                return tex;
            }
        }
        return null;
    }

    public static Texture2D LoadTextureBump(string path)
    {
        path = GetFileNameIgnoreCase(path);
        if (GameData.instance.bumpMaterials.ContainsKey(Path.GetFileName(path)))
        {
            string pathbump = Path.Combine(GameData.instance.gameDir, GameData.instance.bumpMaterials[Path.GetFileName(path)]);
            pathbump = GetFileNameIgnoreCase(pathbump);
            if (File.Exists(pathbump))
            {
                Texture2D tex = new Texture2D(1, 1);
                if (tex.LoadImage(File.ReadAllBytes(pathbump)))
                {
                    return tex;
                }
            }
        }
        return null;
    }

    public static string ReadString(Stream stream)
    {
        int len = ReadInt(stream);
        byte[] bytes = new byte[len];
        stream.Read(bytes, 0, bytes.Length);
        return Encoding.ASCII.GetString(bytes);
    }

    public static int ReadInt(Stream stream)
    {
        byte[] bytes = new byte[sizeof(int)];
        stream.Read(bytes, 0, bytes.Length);
        return BitConverter.ToInt32(bytes, 0);
    }

    public static float ReadFloat(Stream stream)
    {
        byte[] bytes = new byte[sizeof(float)];
        stream.Read(bytes, 0, bytes.Length);
        return BitConverter.ToSingle(bytes, 0);
    }

    public static byte ReadByte(Stream stream)
    {
        return (byte)stream.ReadByte();
    }
}
