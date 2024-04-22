using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    //UNITY
    /// <summary>
    ///  Functions to bridge to unity functionality
    /// </summary>
    public UnityEvent tickEvent;
    //Text Control

    //Sprite Control
    public SpriteRenderer CreateObjectRenderer(string name)
    {
        GameObject obj = new GameObject(name);
        SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
        return rend;
    }
    public void LoadJsonAssets(string episodeNumber, string lang, UnityEvent doneCallBack)
    {
        string root = Application.dataPath + episodeNumber + "/";
        List<AssetData> assets = new List<AssetData>()
        {
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
                path = root + "map" + episodeNumber + ".json"
            },
              new AssetData
            {
                name = "game",
                path = root + lang + "/messages.json"
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
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tickEvent.Invoke();
       
        //Run Controls Callbacks here
    }
    //
}
