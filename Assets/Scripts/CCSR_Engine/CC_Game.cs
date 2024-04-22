﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static CC_Types;

public class CC_Game 
{
  

    public static float MAP_WIDTH = 416;
    public static float MAP_HEIGHT = 320;

    public static float UI_WIDTH_PERCENT = 0.6f;
    public static float UI_HEIGHT_PERCENT = 0.6f;


    public CC_Player player;
    public List<CC_GameObject> gameObjects = new List<CC_GameObject>();
    public CC_GameObject[] movingObjects;

    public float numMapsX = 0;
    public float numMapsY = 0;
    public CC_Types.Rect worldRect;
    public CC_Sound.GameSound sound;
    public CC_Inventory.GameInventory inventory;
    public CC_Camera camera;
    public GameObject worldContainer, sceneContainer;
    public CC_SpriteGame backgroundSprite;
    public CC_Sign sign;
    private string currentMap = "";
    private CC_Script script;
    public List<CC_GameObject> filmLoopObjects = new List<CC_GameObject>();
    public List<FilmLoopData> filmLoopData = new List<FilmLoopData>();
    private Dictionary<string, CC_Scene.GameScene> scenes = new Dictionary<string, CC_Scene.GameScene>();
    private CC_Scene.GameScene currentScene;

    public float targetFps = 13;
    public float MSperTick = 1000 / 13;

    private float lastUpdate = DateTime.Now.Millisecond;

    public bool smoothAnimations = true;
    public GameData gameData;
    public float ratio = 416 / 320;
    public CC_Intro introScreen;
    public enum EngineType
    {
        CCSR,
        Scooby
          
    }
    public EngineType engineType;

    public CC_Game(string episode, string language)
    {
        this.engineType = episode.ToLower().Contains("scooby")
         ? EngineType.Scooby
         : EngineType.CCSR;
        this.introScreen = new CC_Intro(episode);

    }

    public void addScene(string name, CC_Scene scene)
    {
        this.scenes.Add(name, new CC_Scene.GameScene(this));
    }

    public void closeScene()
    {
        if (this.currentScene != null)
        {
            this.currentScene.exit();
            this.player.SetStatus(PlayerStatus.MOVE);
            this.currentScene.container.SetActive(false);
            this.currentScene = null;
        }
    }
    public void playScene(string name)
    {
        CC_Scene.GameScene entry; 
        this.scenes.TryGetValue(name, out entry);

        if (entry != null)
        {
            this.player.SetStatus(PlayerStatus.STOP);
            entry.container.SetActive(true);
            entry.init();
            entry.resize();
            entry.play();
            this.currentScene = entry;
        }
        else
        {
            Debug.Log("Scene not found!" + name);
        }
    }
    public void resize()
    {
        this.camera.setScale();
        this.camera.snapCameraToMap(this.currentMap);
        this.sign.resize();
        this.inventory.resize();
        if (this.currentScene != null)
        {
            this.currentScene.resize();
        }
        this.introScreen.resize(this);
    }
    public void updateAutoMoveObjects()
    {
        // Direction is always incremented counter clockwise
        Pos pp = this.player.getPosition();
        CC_Types.Rect playerRect = this.player.getCollisionRectAtPoint(pp.x, pp.y);

        foreach(CC_GameObject obj in this.movingObjects) {
            // Initialize movers
            if (obj.moveDirection == -1)
            {
                obj.speed = 4;
                obj.moveDirection = randBetween(0, CC_GameObject.MOVE_DIRECTIONS.Length - 1);
            }

            // If we have reached our destination
            if (obj.posX == obj.movePos.x && obj.posY == obj.movePos.y)
            {
                float i = obj.moveDirection + 1;
                obj.moveDirection = i >= CC_GameObject.MOVE_DIRECTIONS.Length ? 0 : i;
                CC_Types.Rect bounds = obj.getMoveBounds();
                float dx =
                  randBetween(0, Mathf.Floor(bounds.width / obj.speed)) * obj.speed;
                float dy =
                  randBetween(0, Mathf.Floor(bounds.width / obj.speed)) * obj.speed;
                Pos movePos = new Pos
                (
                      obj.posX + CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection,0] * dx,
                      obj.posY + CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection, 1] * dy
                );
            obj.movePos = movePos;
        } else
        {
                // We are not where we want to be, try to step forward.
                float dx = CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection, 0];
                float dy = CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection, 1];
                CC_Types.Rect bounds = obj.getMoveBounds();
                Pos nextPos = this.posAfterDeltaMove(obj, dx, dy);
                CC_Types.Rect nextRect = new CC_Types.Rect { x = nextPos.x, y = nextPos.y, width = obj.width, height = obj.height};
                          
        

            bool inPlayer = CC_Collision.Intersect(obj.getRect(), playerRect);
            bool willBeInPlayer = CC_Collision.Intersect(nextRect, playerRect);

            if (CC_Collision.rectAinRectB(nextRect, bounds) &&
              (inPlayer || (!inPlayer && !willBeInPlayer)) &&
              this.canMoveGameObject(obj, nextPos))
            {
                obj.initMove(obj.nextPos, nextPos);
            }
            else
            {
                obj.movePos = obj.nextPos;
            }
        }
    }
}

    public static float[,] getMapOffset(string mapName)
    {
        float xIndex = float.Parse(mapName.Substring(0, 2));
        float yIndex = float.Parse(mapName.Substring(2, 4));

        // Subtract 1 to convert to zero based indexing.
        return   new float[(int)((xIndex) - 1), (int)((yIndex) - 1)];
         
    }
    public static CC_Types.Pos getMapOffset(string mapName, float i = 0)
    {
        float xIndex = int.Parse(mapName.Substring(0, 2));
        float yIndex = int.Parse(mapName.Substring(2, 4));

        // Subtract 1 to convert to zero based indexing.
        return new CC_Types.Pos((xIndex) - 1, (yIndex) - 1);

    }

    public static Texture2D getMemberTexture(string memeberName, string subFolder = "character.visuals")
    {
        // Load the PNG file from the specified file path
        string name = memeberName.ToLower();
        name = name + ".png";
        name = name.Replace(".x",".");

        //get translation sata
        byte[] fileData = System.IO.File.ReadAllBytes(Application.dataPath +"/images/" + subFolder +"/"+ name);

        // Create a new Texture2D
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // Load the image data into the texture

        return texture;
    }

    public  static CC_Types.Rect getMapRect(string mapName) 
    {
      CC_Types.Pos offset = getMapOffset(mapName, 0);
      return new CC_Types.Rect { 
        x = offset.x* MAP_WIDTH,
        y = offset.y* MAP_HEIGHT,
        width = MAP_WIDTH,
        height = MAP_HEIGHT,
      };
    }

    public void setFilmLoopObjects()
    {
        foreach(var obj in gameObjects)
        {
            // Iterate through film loop data
            foreach (var vals in filmLoopData)
            {
                // Check if the member exists in film loop data
                if (vals != null && vals[obj.member] != null)
                {
                    // Set isFrameObject flag to true and add object to filmLoopObjects list
                    obj.isFrameObject = true;
                    filmLoopObjects.Add(obj);
                    break; // Exit the inner loop since the object is already added
                }
            }
        }
    }
    public static int randBetween(float min, float max)
    {
        return (int)Mathf.Floor(UnityEngine.Random.Range(0, 1) * (max - min + 1) + min);
    }

    public void updateAllVisibility()
    {
        foreach (CC_GameObject obj in gameObjects)
        {
            updateVisibility(obj);
        }
        
    }

    private void updateVisibility(CC_GameObject obj)
    {
        bool showObj = false;
        bool secret = false;
        CC_Types.GameObjectVisibility v = obj.data.item.visi;

        if (v.visiObj != "" || v.visiAct != "")
        {
            if (inventory.has(v.visiObj) || this.inventory.has(v.visiAct))
            {
                showObj = true;
                secret = true;
            }
            else
            {
                showObj = false;
            }
        }
        else
        {
            bool hasItem = this.inventory.has(v.inviObj);
            bool hasAct = this.inventory.has(v.inviAct);
            showObj = hasItem || hasAct ? false : true;
        }

        if (secret && !obj.isVisible())
        {
            this.sound.once(this.sound.secret);

            // shitty hack for scooby
            if (this.engineType == EngineType.Scooby)
            {
                if (this.inventory.items.Contains("seebats"))
                {
                    this.sound.dynamicSoundOnce("bunch_o_bats");
                }
                if (this.inventory.items.Contains("max"))
                {
                    this.sound.dynamicSoundOnce("ghost_02");
                }
            }
        }

        obj.setVisible(showObj);
    }

    private void updateFilmLoopObjects()
    {
        FilmLoop filmLoop;
        this.filmLoopObjects.ForEach((obj) => {
            this.filmLoopData.ForEach(val =>
            {
                if (val[obj.member] != null)
                {
                    filmLoop = val[obj.member];
                    if (filmLoop.texture != null)
                    {
                        //console.log("texture", obj)
                        string[] textures = filmLoop.texture.loopTextures;
                        if (obj.frame % filmLoop.texture.delay == 0)
                        {
                            Texture2D tex = getMemberTexture(textures[obj.frameIndex++ % textures.Length]);
                            obj.sprite.spriteData = Sprite.Create(tex, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(tex.width, tex.height)), Vector2.one * 0.5f);
                            if (obj.frameIndex > textures.Length)
                            {
                                obj.frameIndex = 0;
                            }
                        }
                        obj.frame++;
                    }
                    // handle callback film loops
                    else if (filmLoop.callback != null)
                    {
                        filmLoop.callback(obj);
                        obj.frame++;
                    }
                }
            });

        });
    }

    private Pos posAfterDeltaMove(CC_GameObject obj, float dx, float dy) 
    {
        CC_Types.Rect pos = obj.getRect();
        float newX = pos.x + dx * obj.speed;
        float newY = pos.y + dy * obj.speed;
        return new Pos(newX, newY);
 
  }

    private bool canMoveGameObject(CC_GameObject gameObj, Pos toPos)
    {
        CC_Types.Rect newRect = new CC_Types.Rect
        {
            x = toPos.x,
            y = toPos.y,
            width = gameObj.width,
            height = gameObj.height,
        };

        CC_Types.Rect mapRect = getMapRect(gameObj.mapName);

        // If the new position is outside of the current map, disallow pushing
        if (CC_Collision.rectAinRectB(newRect, mapRect) == false)
        {
            return false;
        }
        return true;
    }
}