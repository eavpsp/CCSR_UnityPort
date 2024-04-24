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
     * controls
     * update messages
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
   
    public GameMapAreaDataContainer GetGameMapAreaJson(AssetData asset)
    {
        GameMapAreaDataContainer areas = new GameMapAreaDataContainer();
        TextAsset JSON = new TextAsset();
        JSON = Resources.Load<TextAsset>(asset.path);
        string jsonString = JSON.text;
        JsonUtility.FromJsonOverwrite(jsonString, areas);
        return areas;
    }
    public CC_SpriteGame GenerateGameSprite(string name)
    {
        return new GameObject(name).AddComponent<CC_SpriteGame>();
    }
    public CC_SpriteUI GenerateUISprite(string name)
    {
        return new GameObject(name).AddComponent<CC_SpriteUI>();
    }
    public List<GameMes> GetGameMessages(AssetData asset)
    {
        if (asset != null)
        {
            List<GameMes> data = new List<GameMes>();

            TextAsset JSON = new TextAsset();
            JSON = Resources.Load<TextAsset>(asset.path);
            string jsonString = JSON.text;
            JsonUtility.FromJsonOverwrite(jsonString, data);
            Debug.Log("Messages Loaded");
            Debug.Log(data.Count);
            return data;

        }
        return null;
    }
    public GameData GetGameData(AssetData asset)
    {
        GameData data = new GameData();
        TextAsset JSON = new TextAsset();
        JSON = Resources.Load<TextAsset>(asset.path);
        string jsonString = JSON.text;

        JsonUtility.FromJsonOverwrite(jsonString, data);
        

        return data;
    }
    public void LoadJsonAssets(string episodeNumber, string lang, Action doneCallBack)
    {
        string root = "game/" + episodeNumber + "/";
        assets = new List<AssetData>{

            new AssetData
            {
                name = "textures",
                path = root + "ep" + episodeNumber
            },
            new AssetData
            {
                name = "ending",
                path = root + "ep" + episodeNumber + "_ending"
            },
             new AssetData
            {
                name = "map",
                path = root + "map" + episodeNumber
            },
              new AssetData
            {
                name = "game",
                path = root + lang + "/game"
            },
                 new AssetData
            {
                name = "message",
                path = root + lang + "/messages"
            },
        };
        if (lang != "en")
        {
            assets.Add(new AssetData
            {
                name = "translated",
                path = root + lang + "/ep" + episodeNumber,
            });
        }
        doneCallBack();
    }
    [Serializable]
    public class TestObj
    {
       public List<GameMes> data = new List<GameMes>();
    }

    void Start()
    {
        TestObj data = new TestObj();
        data.data.Add(new GameMes());
        data.data[0]["data"] = "tat";
        Debug.Log(JsonUtility.ToJson(data.data));
        Debug.Log(data.data[0]["data"]);
        episode = "1";
        currentGame = new CC_Game(episode, "en");
    }

    // Update is called once per frame
    void Update()
    {
        
            currentGame.Update(Time.deltaTime);
           //Debug.Log(currentGame == null);

        

        //Run Controls Callbacks here
    }
    //
}
