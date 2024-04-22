using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static CC_Types;

public class AssetData
{
    public string name;
    public string path;
}
public class EngineManager : MonoBehaviour
{
    //Handles object initiation and engine / game control
    // Start is called before the first frame update
    /*TODO: 
     * game
     * Manager
     */
    public static EngineManager instance;
    public AudioSource SFX, BGM;
    public Canvas mainCanvas;
    public CC_Game currentGame;
    public string episode;
    //UNITY
    /// <summary>
    ///  Functions to bridge to unity functionality
    /// </summary>
    public UnityEvent tickEvent;
    //Text Control
    public List<AssetData> assets;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }
    //Sprite Control
    public SpriteRenderer CreateObjectRenderer(string name)
    {
        GameObject obj = new GameObject(name);
        SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
        return rend;
    }
    public GameMapArea GetGameMapAreaJson(string mapName)
    {
        GameMapArea data = new GameMapArea();
        string root = Application.dataPath + "/game/" + EngineManager.instance.episode + "/map.data/";
        string jsonString = File.ReadAllText(root + mapName + ".txt");
        try
        {
           
            JsonUtility.FromJsonOverwrite(jsonString, data);
        }
        catch (ArgumentException ex)
        {
            Debug.LogError("Error parsing JSON: " + ex.Message);
            Debug.LogError("Problematic JSON string: " + jsonString);
        }

        return data;
    }
    public CC_SpriteGame GenerateGameSprite(string name)
    {
        return new GameObject(name).AddComponent<CC_SpriteGame>();
    }
    public CC_SpriteUI GenerateUISprite(string name)
    {
        return new GameObject(name).AddComponent<CC_SpriteUI>();
    }
    public GameMessages GetGameMessages(AssetData asset)
    {
        GameMessages data = null;
        if (asset != null)
        {
            JsonUtility.FromJsonOverwrite(asset.path, data);
        }

        return data;
    }
    public GameData GetGameData(AssetData asset)
    {
        GameData data = new GameData();
        string jsonString = File.ReadAllText(asset.path);
        
        JsonUtility.FromJsonOverwrite(jsonString, data);
        

        return data;
    }
    public void LoadJsonAssets(string episodeNumber, string lang, Action doneCallBack)
    {
        string root = Application.dataPath + "/game/" + episodeNumber + "/";
        assets = new List<AssetData>{

            new AssetData
            {
                name = "textures",
                path = root + "ep" + episodeNumber + ".json"
            },
            new AssetData
            {
                name = "ending",
                path = root + "ep" + episodeNumber + "_ending.json"
            },
             new AssetData
            {
                name = "map",
                path = root + "map.data" + ".txt"
            },
              new AssetData
            {
                name = "game",
                path = root + lang + "/game.json"
            },
                 new AssetData
            {
                name = "message",
                path = root + lang + "/message.json"
            },
        };
        if (lang != "en")
        {
            assets.Add(new AssetData
            {
                name = "translated",
                path = root + lang + "/ep" + episodeNumber + ".json",
            });
        }
        doneCallBack();
    }
    void Start()
    {
        episode = "1";
        currentGame = new CC_Game(episode, "en");
    }

    // Update is called once per frame
    void Update()
    {
        tickEvent.Invoke();
       
        //Run Controls Callbacks here
    }
    //
}
