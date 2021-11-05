using System.Collections;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;
using UnityEngine;
using UnityEngine.Networking;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    public string gameDir;
    public string gfxDir => Path.Combine(gameDir, "GFX");
    public string mapDir => Path.Combine(gfxDir, "map");
    public string dataDir => Path.Combine(gameDir, "Data");
    public string materialFile => Path.Combine(dataDir, "materials.ini");
    public string roomsFile => Path.Combine(dataDir, "rooms.ini");
    public Dictionary<string, string> bumpMaterials = new Dictionary<string, string>();
    public Dictionary<int, AudioClip> roomAmbientAudio = new Dictionary<int, AudioClip>();

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

    void Awake()
    {
        instance = this;
        FileIniDataParser parser = new FileIniDataParser();
        IniData materialData = parser.ReadFile(materialFile);
        foreach (SectionData item in materialData.Sections)
        {
            if (item.Keys.ContainsKey("bump"))
            {
                bumpMaterials.Add(item.SectionName, item.Keys["bump"]);
            }
        }
    }

    IEnumerator Start()
    {
        yield return LoadRooms();
    }

    public IEnumerator LoadRooms()
    {
        FileIniDataParser parser = new FileIniDataParser();
        IniData roomsData = parser.ReadFile(roomsFile);
        foreach (SectionData item in roomsData.Sections)
        {
            if (item.SectionName == "room ambience")
            {
                foreach (var key in item.Keys)
                {
                    int ke = int.Parse(key.KeyName.Replace("ambience", ""));
                    UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{GetFileNameIgnoreCase(Path.Combine(gameDir, key.Value))}", AudioType.UNKNOWN);
                    yield return www.SendWebRequest();
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    roomAmbientAudio.Add(ke, clip);
                }
            }
            else
            {
                string key = item.Keys["mesh path"];
                RMeshData rmesh = RMeshLoader.LoadRMesh(GetFileNameIgnoreCase(Path.Combine(gameDir, key)));
                rmesh.gameObject.SetActive(false);
                Debug.Log(rmesh.name);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        yield break;
    }
}