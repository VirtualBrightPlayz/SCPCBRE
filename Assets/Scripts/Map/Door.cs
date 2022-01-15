using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Door : MonoBehaviour
{
    public static string DoorPath => Path.Combine(GameData.instance.mapDir, "Door01.x");
    public static string BigDoor0Path => Path.Combine(GameData.instance.mapDir, "ContDoorLeft.x");
    public static string BigDoor1Path => Path.Combine(GameData.instance.mapDir, "ContDoorRight.x");
    public static string HeavyDoor0Path => Path.Combine(GameData.instance.mapDir, "HeavyDoor1.x");
    public static string HeavyDoor1Path => Path.Combine(GameData.instance.mapDir, "HeavyDoor2.x");
    public static string DoorCollPath => Path.Combine(GameData.instance.mapDir, "DoorColl.x");
    public static string DoorFramePath => Path.Combine(GameData.instance.mapDir, "DoorFrame.x");

    public static async UniTask<Door> CreateDoor(CancellationTokenSource token, int lvl, Vector3 xyz, float angle, RMeshData room, bool dopen = false, int big = 0, bool keycard = false, string code = "", bool useCollisionMesh = false)
    {
        GameObject go = null;

        switch (big)
        {
            case 1:
            {
                go = new GameObject("BigDoor", typeof(Door));
                Door door = go.GetComponent<Door>();
                door.parts = new ModelEntity[2];
                door.frames = new ModelEntity[1];

                MeshData data = await AssetCache.LoadModel(BigDoor0Path, token);
                GameObject go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ModelEntity ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                door.parts[0] = ent;

                data = await AssetCache.LoadModel(BigDoor1Path, token);
                go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                door.parts[1] = ent;

                data = await AssetCache.LoadModel(DoorCollPath, token);
                go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                ent.GetComponent<MeshRenderer>().enabled = false;
                door.frames[0] = ent;
            }
            break;
            case 2:
            {
                go = new GameObject("HeavyDoor", typeof(Door));
                Door door = go.GetComponent<Door>();
                door.parts = new ModelEntity[2];
                door.frames = new ModelEntity[1];

                MeshData data = await AssetCache.LoadModel(HeavyDoor0Path, token);
                GameObject go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ModelEntity ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                door.parts[0] = ent;

                data = await AssetCache.LoadModel(HeavyDoor1Path, token);
                go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                door.parts[1] = ent;

                data = await AssetCache.LoadModel(DoorFramePath, token);
                go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                ent.GetComponent<MeshRenderer>().enabled = false;
                door.frames[0] = ent;
            }
            break;
            case 3: // TODO: elevator
            {
            }
            break;
            default:
            {
                go = new GameObject("NormalDoor", typeof(Door));
                Door door = go.GetComponent<Door>();
                door.parts = new ModelEntity[2];
                door.frames = new ModelEntity[1];

                MeshData data = await AssetCache.LoadModel(DoorPath, token);
                GameObject go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ModelEntity ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                door.parts[0] = ent;

                data = await AssetCache.LoadModel(DoorPath, token);
                go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                door.parts[1] = ent;

                data = await AssetCache.LoadModel(DoorFramePath, token);
                go2 = new GameObject(data.mesh.name);
                go2.transform.SetParent(go.transform);
                ent = go2.AddComponent<ModelEntity>();
                ent.visibleData = data;
                ent.RefreshData();
                ent.GetComponent<MeshRenderer>().enabled = false;
                door.frames[0] = ent;
            }
            break;
        }

        return go.GetComponent<Door>();
    }


    public ModelEntity[] parts = new ModelEntity[0];
    public ModelEntity[] frames = new ModelEntity[0];
}