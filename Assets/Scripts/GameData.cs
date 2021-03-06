using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using IniParser;
using IniParser.Model;
using UnityEngine;
using UnityEngine.Networking;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    public string gameDir;
    public string gfxDir => Path.Combine(gameDir, "GFX");
    public string sfxDir => Path.Combine(gameDir, "SFX");
    public string mapDir => Path.Combine(gfxDir, "map");
    public string propsDir => Path.Combine(mapDir, "Props");
    public string dataDir => Path.Combine(gameDir, "Data");
    public string materialFile => Path.Combine(dataDir, "materials.ini");
    public string roomsFile => Path.Combine(dataDir, "rooms.ini");
    public string optionsFile => Path.Combine(gameDir, "options.ini");
    public string loadingScreenDir => Path.Combine(gameDir, "Loadingscreens");
    public string loadingScreensFile => Path.Combine(loadingScreenDir, "loadingscreens.ini");
    public GameOptions options;
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
        if (!Directory.Exists(dir))
        {
            return filename;
        }
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
    }

    IEnumerator Start()
    {
        yield return LoadMaterials();
        #if UNITY_EDITOR
        CancellationTokenSource cts = new CancellationTokenSource();
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                cts.Cancel();
            }
        };
        #else
        CancellationTokenSource cts = default;
        #endif
        yield return LoadRooms(cts).ToCoroutine();
        yield return FindObjectOfType<MapGenerator>().CreateMap(cts).ToCoroutine();
    }

    public IEnumerator LoadOptions()
    {
        FileIniDataParser parser = new FileIniDataParser();
        IniData optionsData = parser.ReadFile(optionsFile);
        options = new GameOptions();
        int key = 0;
        if (int.TryParse(optionsData["binds"]["right key"], out key))
        {
            options.RightKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["left key"], out key))
        {
            options.LeftKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["up key"], out key))
        {
            options.UpKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["down key"], out key))
        {
            options.DownKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["blink key"], out key))
        {
            options.BlinkKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["sprint key"], out key))
        {
            options.SprintKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["inventory key"], out key))
        {
            options.InventoryKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["save key"], out key))
        {
            options.SaveKey = (KeyCode)key;
        }
        if (int.TryParse(optionsData["binds"]["console key"], out key))
        {
            options.ConsoleKey = (KeyCode)key;
        }
        if (float.TryParse(optionsData["options"]["mouse smoothing"], out float smoothing))
        {
            options.MouseSmoothing = smoothing;
        }
        yield break;
    }

    public IEnumerator LoadMaterials()
    {
        FileIniDataParser parser = new FileIniDataParser();
        IniData materialData = parser.ReadFile(materialFile);
        foreach (SectionData item in materialData.Sections)
        {
            if (item.Keys.ContainsKey("bump") && !bumpMaterials.ContainsKey(item.SectionName))
            {
                bumpMaterials.Add(item.SectionName, item.Keys["bump"]);
            }
        }
        yield break;
    }

    public async UniTask LoadRooms(CancellationTokenSource token = default)
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
                    if (roomAmbientAudio.ContainsKey(ke))
                    {
                        continue;
                    }
                    UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{GetFileNameIgnoreCase(Path.Combine(gameDir, key.Value))}", AudioType.UNKNOWN);
                    await www.SendWebRequest().WithCancellation(token.Token);
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    roomAmbientAudio.Add(ke, clip);
                }
            }
        }
    }
}