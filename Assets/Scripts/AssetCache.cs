using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using IniParser;
using IniParser.Model;
using UnityEngine;
using UnityEngine.Networking;

public class AssetCache : MonoBehaviour
{
    public static AssetCache instance;
    public Dictionary<string, RMeshData> rooms = new Dictionary<string, RMeshData>();
    public Dictionary<string, MeshData> models = new Dictionary<string, MeshData>();
    public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

    void Awake()
    {
        instance = this;
    }

    private static string Shape(MapGenerator.RoomType type)
    {
        switch (type)
        {
            case MapGenerator.RoomType.ROOM1:
                return "1";
            case MapGenerator.RoomType.ROOM2:
                return "2";
            case MapGenerator.RoomType.ROOM2C:
                return "2C";
            case MapGenerator.RoomType.ROOM3:
                return "3";
            case MapGenerator.RoomType.ROOM4:
                return "4";
            default:
                return string.Empty;
        }
    }

    public static async UniTask<RMeshData> LoadRoom(string name, MapGenerator.RoomType type, int zone, System.Random rng, CancellationTokenSource token)
    {
        FileIniDataParser parser = new FileIniDataParser();
        IniData roomsData = parser.ReadFile(GameData.instance.roomsFile);
        List<string> names = new List<string>();
        foreach (var item in roomsData.Sections)
        {
            bool inzone = false;
            foreach (var key in item.Keys)
            {
                if (key.KeyName.StartsWith("zone") && key.Value == zone.ToString())
                {
                    inzone = true;
                    break;
                }
            }
            if (item.Keys.ContainsKey("mesh path") && item.Keys.ContainsKey("shape") && item.Keys.ContainsKey("commonness") && inzone)
            {
                string key = item.Keys["mesh path"].Replace("\\", "/");
                string shape = item.Keys["shape"];
                if (shape.ToUpper() == Shape(type))
                {
                    for (int i = 0; i < int.Parse(item.Keys["commonness"]); i++)
                    {
                        names.Add(item.SectionName);
                    }
                }
            }
        }
        if (string.IsNullOrEmpty(name) && names.Count > 0)
        {
            name = names[rng.Next(0, names.Count)];
        }
        if (string.IsNullOrEmpty(name) || !roomsData.Sections.ContainsSection(name))
        {
            Debug.Log(type);
            Debug.Log(zone);
            return null;
        }
        return await LoadRoomMesh(Path.Combine(GameData.instance.gameDir, roomsData[name]["mesh path"].Replace("\\", "/")), token);
    }

    public static async UniTask<RMeshData> LoadRoomMesh(string path, CancellationTokenSource token)
    {
        string key = Path.GetFileName(path).ToLower();
        if (instance.rooms.ContainsKey(key))
        {
            return instance.rooms[key];
        }
        RMeshData data = await RMeshLoader.LoadRMesh(path, token);
        data.gameObject.SetActive(false);
        DontDestroyOnLoad(data.gameObject);
        instance.rooms.Add(key, data);
        return data;
    }

    public static async UniTask<Texture2D> LoadTexture(string path, CancellationTokenSource token)
    {
        string key = Path.GetFileName(path).ToLower();
        if (instance.textures.ContainsKey(key))
        {
            return instance.textures[key];
        }
        string file = GameData.GetFileNameIgnoreCase(path);
        if (!File.Exists(file))
        {
            return null;
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{file}");
        await www.SendWebRequest().WithCancellation(token.Token);
        Texture2D tex = DownloadHandlerTexture.GetContent(www);
        instance.textures.Add(key, tex);
        return tex;
    }

    public static async UniTask<Texture2D> LoadTextureBump(string path, CancellationTokenSource token)
    {
        path = GameData.GetFileNameIgnoreCase(path);
        if (GameData.instance.bumpMaterials.ContainsKey(Path.GetFileName(path)))
        {
            string pathbump = Path.Combine(GameData.instance.gameDir, GameData.instance.bumpMaterials[Path.GetFileName(path)]);
            pathbump = GameData.GetFileNameIgnoreCase(pathbump);
            if (File.Exists(pathbump))
            {
                return await LoadTexture(pathbump, token);
            }
        }
        return null;
    }

    public static async UniTask<MeshData> LoadModel(string file, CancellationTokenSource token)
    {
        string key = Path.GetFileName(file).ToLower();
        if (instance.models.ContainsKey(key))
        {
            return instance.models[key];
        }
        string path = GameData.GetFileNameIgnoreCase(Path.Combine(GameData.instance.propsDir, file));
        Assimp.AssimpContext ctx = new Assimp.AssimpContext();
        Assimp.Scene scene = ctx.ImportFile(path);
        List<int> indices = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        int vertexOffset = 0;
        foreach (Assimp.Mesh item in scene.Meshes)
        {
            List<Assimp.Vector3D> verts = item.Vertices;
            List<Assimp.Vector3D> norms = item.HasNormals ? item.Normals : null;
            List<Assimp.Vector3D> uv = item.HasTextureCoords(0) ? item.TextureCoordinateChannels[0] : null;
            for (int i = 0; i < verts.Count; i++)
            {
                vertices.Add(ToVector3(verts[i]));
                if (norms != null)
                    normals.Add(ToVector3(norms[i]));
                if (uv != null)
                    uvs.Add(ToVector3(uv[i]));
            }

            List<Assimp.Face> faces = item.Faces;
            for (int i = 0; i < faces.Count; i++)
            {
                Assimp.Face face = faces[i];

                if (face.IndexCount != 3)
                {
                    indices.Add(0);
                    indices.Add(0);
                    indices.Add(0);
                    continue;
                }

                indices.Add(face.Indices[0] + vertexOffset);
                indices.Add(face.Indices[1] + vertexOffset);
                indices.Add(face.Indices[2] + vertexOffset);
            }

            vertexOffset += verts.Count;
        }
        Mesh mesh = new Mesh();
        mesh.name = Path.GetFileName(file);
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        List<Material> materials = new List<Material>();
        if (scene.HasMaterials)
        {
            foreach (Assimp.Material item in scene.Materials)
            {
                materials.Add(await LoadMaterial(path, item, token));
            }
        }
        MeshData data = new MeshData(mesh, materials.ToArray());
        instance.models.Add(key, data);
        return data;
    }

    public static async UniTask<Material> LoadMaterial(string basePath, Assimp.Material material, CancellationTokenSource token)
    {
        string path = Path.GetDirectoryName(basePath);
        string diffuse = GameData.GetFileNameIgnoreCase(Path.Combine(path, material.HasTextureDiffuse ? material.TextureDiffuse.FilePath : string.Empty));
        string bump = GameData.GetFileNameIgnoreCase(Path.Combine(path, material.HasTextureNormal ? material.TextureNormal.FilePath : string.Empty));
        Material mat = new Material(Resources.Load<Material>("ModelMaterial"));
        mat.SetTexture("_MainTex", await LoadTexture(diffuse, token));
        mat.SetTexture("_BumpMap", await LoadTexture(bump, token));
        mat.name = Path.GetFileNameWithoutExtension(diffuse);
        return mat;
    }

    public static Vector3 ToVector3(Assimp.Vector3D vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
}
