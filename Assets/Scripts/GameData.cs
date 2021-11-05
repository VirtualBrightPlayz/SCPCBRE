using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    public string gameDir;
    public string gfxDir => Path.Combine(gameDir, "GFX");
    public string mapDir => Path.Combine(gfxDir, "map");
    public string dataDir => Path.Combine(gameDir, "Data");
    public string materialFile => Path.Combine(dataDir, "materials.ini");
    public Dictionary<string, string> bumpMaterials = new Dictionary<string, string>();

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
}