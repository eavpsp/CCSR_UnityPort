using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EngineManager : MonoBehaviour
{
    //Handles object initiation and engine / game control
    // Start is called before the first frame update
    /*TODO: 
     * game
     * load
     * intro
     * script
     * sign
     * scene
     * Manager
     */
    public static EngineManager instance;
    public AudioSource SFX, BGM;
    public Canvas mainCanvas;
    //Text Control
    
    //Sprite Control
    public SpriteRenderer CreateObjectRenderer(string name)
    {
        GameObject obj = new GameObject(name);
        SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
        return rend;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
