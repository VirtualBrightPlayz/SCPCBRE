using System.Collections;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;
using UnityEngine;
using UnityEngine.Networking;

public class LoadingScreen : MonoBehaviour
{
    [System.Serializable]
    public class LoadingScreenOption
    {
        public string Title;
        public string ImagePath;
        public LoadingScreenAlignX AlignX;
        public LoadingScreenAlignY AlignY;
        public bool DisableBackground;
        public string[] Text;
        public Texture2D Background;
        public Texture2D Image;
    }

    public enum LoadingScreenAlignX
    {
        Right,
        Center,
        Left
    }

    public enum LoadingScreenAlignY
    {
        Top,
        Center,
        Bottom
    }

    public static LoadingScreen instance;
    public List<LoadingScreenOption> loadingScreens = new List<LoadingScreenOption>();
    public LoadingScreenOption current;
    [Range(-1, 101)]
    public int percent = 0;
    private GUIStyle style;
    private GUIStyle style2;
    public Font font;
    private AudioClip horror8;
    public int textIndex = 0;
    private Texture2D blinkMeterImage;

    void OnGUI()
    {
        if (percent == -1)
        {
            current = loadingScreens[Random.Range(0, loadingScreens.Count)];
            StartCoroutine(LoadScreen());
            percent = 0;
        }
        if (current != null)
        {
            textIndex = Mathf.FloorToInt(percent / 100f * current.Text.Length);
            if (current.Background != null)
            {
                Rect rect = new Rect(0, 0, current.Background.width / 2f, current.Background.height / 2f);
                rect.x = Screen.width / 2f - rect.width / 2f;
                rect.y = Screen.height / 2f - rect.height / 2f;
                GUI.Label(rect, current.Background, GUIStyle.none);
            }
            if (current.Image != null)
            {
                Rect rect = new Rect(0, 0, current.Image.width / 2f, current.Image.height / 2f);
                switch (current.AlignX)
                {
                    case LoadingScreenAlignX.Right:
                    rect.x = Screen.width - rect.width;
                    break;
                    case LoadingScreenAlignX.Center:
                    rect.x = Screen.width / 2f - rect.width / 2f;
                    break;
                    case LoadingScreenAlignX.Left:
                    rect.x = 0f;
                    break;
                }
                switch (current.AlignY)
                {
                    case LoadingScreenAlignY.Bottom:
                    rect.y = Screen.height - rect.height;
                    break;
                    case LoadingScreenAlignY.Center:
                    rect.y = Screen.height / 2f - rect.height / 2f;
                    break;
                    case LoadingScreenAlignY.Top:
                    rect.y = 0f;
                    break;
                }
                GUI.Label(rect, current.Image, GUIStyle.none);
            }
            if (percent == 100)
            {
                AudioSource.PlayClipAtPoint(horror8, Vector3.zero);
                percent = 101;
            }
            if (percent == 101)
            {
                GUIContent content = new GUIContent("PRESS ANY KEY TO CONTINUE");
                Vector2 size = style.CalcSize(content);
                GUI.color = Color.black;
                GUI.Label(new Rect(Screen.width / 2f + 1f - size.x / 2f, Screen.height - 50f + 1f - size.y / 2f, size.x, size.y), content, style);
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width / 2f - size.x / 2f, Screen.height - 50f - size.y / 2f, size.x, size.y), content, style);
            }
            // TITLE
            {
                GUIContent content = new GUIContent(current.Title);
                Vector2 size = style.CalcSize(content);
                GUI.color = Color.black;
                GUI.Label(new Rect(Screen.width / 2f + 1f - size.x / 2f, Screen.height / 2f + 80f + 1f - size.y / 2f, size.x, size.y), content, style2);
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width / 2f - size.x / 2f, Screen.height / 2f + 80f - size.y / 2f, size.x, size.y), content, style2);
            }
            // desc
            if (textIndex >= 0 && textIndex < current.Text.Length)
            {
                GUIContent content = new GUIContent(current.Text[textIndex]);
                Vector2 size = style.CalcSize(content);
                size.x = Mathf.Min(size.x, 400f);
                size.y = Mathf.Min(size.y, 300f);
                GUI.color = Color.black;
                GUI.Label(new Rect(Screen.width / 2f + 1f - size.x / 2f, Screen.height / 2f + 1f - size.y / 2f, size.x, size.y), content, style);
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width / 2f - size.x / 2f, Screen.height / 2f - size.y / 2f, size.x, size.y), content, style);
            }
            // LOADING...
            {
                GUIContent content = new GUIContent($"LOADING - {Mathf.Clamp(percent, 0, 100)} %");
                Vector2 size = style.CalcSize(content);
                GUI.color = Color.black;
                GUI.Label(new Rect(Screen.width / 2f + 1f - size.x / 2f, Screen.height / 2f - 100f + 1f - size.y / 2f, size.x, size.y), content, style);
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width / 2f - size.x / 2f, Screen.height / 2f - 100f - size.y / 2f, size.x, size.y), content, style);
            }
            // Loading bar
            {
                float width = 300;
                float height = 20;
                float x = Screen.width / 2f - width / 2f;
                float y = Screen.height / 2f + 30 - 100;
                // TODO: the outline on the bar
                for (int i = 0; i < (int)((width - 2) * (percent / 100f) / 10); i++)
                {
                    Rect rect = new Rect(x + 3 + 10 * i, y + 3, blinkMeterImage.width, blinkMeterImage.height);
                    GUI.Label(rect, blinkMeterImage, GUIStyle.none);
                }
            }
        }
    }

    IEnumerator LoadScreen()
    {
        textIndex = 0;
        if (horror8 == null)
        {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{Path.Combine(GameData.instance.sfxDir, "Horror", "Horror8.ogg")}", AudioType.UNKNOWN);
            yield return www.SendWebRequest();
            horror8 = DownloadHandlerAudioClip.GetContent(www);
        }
        if (blinkMeterImage == null)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{Path.Combine(GameData.instance.gfxDir, "BlinkMeter.jpg")}");
            yield return www.SendWebRequest();
            blinkMeterImage = DownloadHandlerTexture.GetContent(www);
        }
        if (!current.DisableBackground)
        {
            string back = Path.Combine(GameData.instance.loadingScreenDir, "loadingback.jpg");
            if (current.Background == null)
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{back}");
                yield return www.SendWebRequest();
                Texture2D tex = DownloadHandlerTexture.GetContent(www);
                current.Background = tex;
            }
        }
        string path = Path.Combine(GameData.instance.loadingScreenDir, current.ImagePath);
        if (current.Image == null)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{path}");
            yield return www.SendWebRequest();
            Texture2D tex = DownloadHandlerTexture.GetContent(www);
            current.Image = tex;
        }
    }

    void Start()
    {
        style = new GUIStyle();
        style.font = font;
        style.fontSize = 16;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.wordWrap = true;
        style2 = new GUIStyle(style);
        style2.wordWrap = false;
        style2.fontSize = 24;
        loadingScreens.Clear();
        FileIniDataParser parser = new FileIniDataParser();
        IniData loadingScreenData = parser.ReadFile(GameData.instance.loadingScreensFile);
        foreach (SectionData item in loadingScreenData.Sections)
        {
            LoadingScreenAlignX x = LoadingScreenAlignX.Center;
            LoadingScreenAlignY y = LoadingScreenAlignY.Center;
            System.Enum.TryParse<LoadingScreenAlignX>(item.Keys["align x"], true, out x);
            System.Enum.TryParse<LoadingScreenAlignY>(item.Keys["align y"], true, out y);
            bool disableBG = false;
            if (item.Keys.ContainsKey("disablebackground"))
            {
                bool.TryParse(item.Keys["disablebackground"], out disableBG);
            }
            List<string> texts = new List<string>();
            foreach (var key in item.Keys)
            {
                if (key.KeyName.StartsWith("text"))
                {
                    texts.Add(key.Value);
                }
            }
            loadingScreens.Add(new LoadingScreenOption()
            {
                Title = item.SectionName,
                ImagePath = item.Keys["image path"],
                AlignX = x,
                AlignY = y,
                DisableBackground = disableBG,
                Text = texts.ToArray()
            });
        }
        current = loadingScreens[Random.Range(0, loadingScreens.Count)];
        StartCoroutine(LoadScreen());
    }

    void Awake()
    {
        instance = this;
    }
}
