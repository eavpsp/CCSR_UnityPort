﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Security.Cryptography;
using System.Text;
using static CC_Types;

public class CC_Game 
{
  
    public static int MAP_WIDTH = 416;
    public static int MAP_HEIGHT = 320;
    public static float UI_WIDTH_PERCENT = 0.6f;
    public static float UI_HEIGHT_PERCENT = 0.6f;
    public CC_Player player;
    public List<CC_GameObject> gameObjects = new List<CC_GameObject>();
    public List<CC_GameObject> movingObjects = new List<CC_GameObject>();
    public int numMapsX = 0;
    public int numMapsY = 0;
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
    public GameMapAreaDataContainer area = new GameMapAreaDataContainer();
    public float ratio = 416 / 320;
    public CC_Intro introScreen;
    public string episode;
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
        this.script = new CC_Episode1(this);
     

        this.sign = new CC_Sign(this, this.engineType);
        this.inventory = new CC_Inventory.GameInventory(this, this.engineType);
        this.camera = new CC_Camera(this);
        this.sound = new CC_Sound.GameSound(this.engineType, episode);

        EngineManager.instance.LoadJsonAssets(episode, language, () => {
            Debug.Log("Done loading assets!");
            this.init(episode);
        });
    }
    public void addScene(string name, CC_Scene.GameScene scene)
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
                obj.moveDirection = randBetween(0, CC_GameObject.MOVE_DIRECTIONS.Count - 1);
            }

            // If we have reached our destination
            if (obj.posX == obj.movePos.x && obj.posY == obj.movePos.y)
            {
                float i = obj.moveDirection + 1;
                obj.moveDirection = i >= CC_GameObject.MOVE_DIRECTIONS.Count ? 0 : i;
                CC_Types.Rect bounds = obj.getMoveBounds();
                float dx =
                  randBetween(0, Mathf.Floor(bounds.width / obj.speed) * obj.speed);
                float dy =
                  randBetween(0, Mathf.Floor(bounds.width / obj.speed) * obj.speed);
                Pos movePos = new Pos
                (obj.posX + CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection][0] * dx,
                  obj.posY + CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection][1] * dy
                ); 
            obj.movePos = movePos;
        } else
        {
                // We are not where we want to be, try to step forward.
                int dx = CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection][0];
                int dy = CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection][1];
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
    public static int[] getMapOffset(string mapName)
    { 
        string xIndex = (mapName.Substring(0, 2));
        string yIndex = (mapName.Substring(2, 2));
        return new int[] { int.Parse(xIndex) - 1, int.Parse(yIndex) - 1 } ;
    }
    public static CC_Types.Pos getMapOffset(string mapName, float i = 0)
    {
       
            string xIndex = mapName.Substring(0, 2);
            string yIndex = mapName.Substring(2, 2);
            return new CC_Types.Pos(int.Parse(xIndex) - 1, int.Parse(yIndex) - 1);
        // Subtract 1 to convert to zero based indexing.

    }
    public void setMap(string mapName)
    {
       this.currentMap = mapName;

        
    }
    
    public void movePlayer(float dx, float dy)
    {

            // player isn't moving, stop the walking sound
            if (dx == 0 && dy == 0)
            {
                // TODO
                return;
            }

            Pos pos = this.player.getPosition();
            float newX = pos.x + dx * this.player.speed;
            float newY = pos.y + dy * this.player.speed;

            // TODO

            // Check to see if we need to scroll the map
            // This should be a toggleable setting in the future

            // Check interaction with game objects
            // Search through objects in reverse order
            // Get the first object that we collide with

            // This could be a tiny bug if the player's texture is
            //  supposed to change and it's not a 32x32 size
            CC_Types.Rect newPlayerRect = this.player.getCollisionRectAtPoint(newX, newY);

            if (!CC_Collision.rectAinRectB(newPlayerRect, this.worldRect))
            {
               
            }

            CC_GameObject collisionObject = this.gameObjects.Find(
              (obj) => obj.isVisible() && CC_Collision.Intersect(newPlayerRect, obj.getRect())
            );

            if (collisionObject == null)
            {
                Debug.Log("No game object was found where you tried to walk!");
                
            }

        // Determine if this object has a message to show.
        bool inWater = false;
        if (collisionObject != null)
        {
            GameObjectMessage[] messages = collisionObject.data.message.ToArray();
            string message = "";

            if (messages.Length > 0)
            {
                Debug.Log(collisionObject);
                // Find the message which is relavant to our player's state
                foreach (GameObjectMessage m in messages)
                {
                    if (m.plrAct == "" && m.plrObj == "")
                    {
                        message = m.text;
                        continue;
                    }
                    if (m.plrObj == "" && m.plrAct != "")
                    {
                        if (this.inventory.has(m.plrAct))
                        {
                            message = m.text;
                            //break;
                        }
                        continue;
                    }
                    if (m.plrObj != "" && m.plrAct == "")
                    {
                        if (this.inventory.has(m.plrObj))
                        {
                            message = m.text;
                            //break;
                        }
                        continue;
                    }
                    if (m.plrObj != "" && m.plrAct != "")
                    {
                        if (this.inventory.has(m.plrObj) && this.inventory.has(m.plrAct))
                        {
                            message = m.text;
                            //break;
                        }
                    }
                }
            }

            if (message != "")
            {
                if (
                  collisionObject.data.item.type == GameObjectType.CHAR.GetDescription() ||
                  collisionObject.data.item.type == GameObjectType.ITEM.GetDescription()
                )
                {
                    this.sign.showCharacterMessage(collisionObject.member, message);
                }
                else
                {
                    this.sign.showMessage(message);
                }
            }

            GameObjectCond[] conds = collisionObject.data.item.COND.Where(
              (c) => c != null
    ).ToArray();

            bool getItem = false;
            int condIndex = -1;

            if (conds.Length > 0)
            {
                foreach (GameObjectCond c in conds)
                {
                    condIndex++;
                    if (c.hasObj == "" && c.hasAct == "")
                    {
                        // console.log("!hasObj && !hasAct");
                        getItem = true;
                        continue;
                    }
                    if (c.hasObj == "" && c.hasAct != "")
                    {
                        // console.log("!hasObj && hasAct");
                        if (this.inventory.has(c.hasAct))
                        {
                            getItem = true;
                        }
                        break;
                        // Possible bug? Check this if there are problems in game
                        // continue;
                    }
                    if (c.hasObj != "" && c.hasAct == "")
                    {
                        // console.log("hasObj && !hasAct");
                        if (this.inventory.has(c.hasObj))
                        {
                            getItem = true;
                            this.inventory.removeItem(c.hasObj);
                        }
                        break;
                    }
                    if (c.hasObj != "" && c.hasAct != "")
                    {
                        // console.log("hasObj && hasAct");
                        if (this.inventory.has(c.hasObj) && this.inventory.has(c.hasAct))
                        {
                            getItem = true;
                            this.inventory.removeItem(c.hasObj);
                        }
                        break;
                    }
                }
            }

            if (getItem)
            {
                GameObjectCond c = conds[condIndex];
                if (
                  c.giveObj != "" &&
                  !this.inventory.has(c.giveObj) &&
                  !this.inventory.has("got" + c.giveObj)
                )
                {
                    this.inventory.addItem(c.giveObj);

                    // fugly scooby hack
                    if (this.engineType == EngineType.Scooby)
                    {
                        if (c.giveObj == "nobats")
                        {
                            // this.sound.soundBank["bunch_o_bats"];
                        }
                    }
                    UnityEvent onCloseEvent = new UnityEvent();
                    onCloseEvent.AddListener(delegate
                    {
                        this.inventory.openInventory(c.giveObj);
                    });
                    this.sign.SetOnClose(onCloseEvent);
                }
                if (c.giveAct != "" && !this.inventory.has(c.giveAct))
                {
                    this.inventory.addAct(c.giveAct);
                }
            }

            


            string name = collisionObject.data.item.name;
            if (name != "" && !name.Contains("="))
            {
                if (!this.inventory.has(name))
                {
                    this.inventory.names.Add(name);
                    this.inventory.addAct("got" + name);
                }
            }

            // Switch over object type and handle each case differently
            switch (collisionObject.data.item.type)
            {
                case "FLOR":
                    break;
                case "CHAR":
                case "WALL":
                    {
                        if (collisionObject.data.move.COND == GameObjectMoveCond.PUSH)
                        {
                            Pos toPos = this.posAfterDeltaMove(collisionObject, dx, dy);
                            if (this.canMoveGameObject(collisionObject, toPos))
                            {
                                // Make sure the player doesn't collide with something
                                // in his new coordinates that isn't the push object
                                CC_GameObject futureCollision = this.gameObjects.Find(
                                  (obj) =>
                                    obj.isVisible() &&
                                    CC_Collision.Intersect(newPlayerRect, obj.getRect()) &&
                                    obj.data.item.type == GameObjectType.WALL.GetDescription() &&
                                    obj != collisionObject
                                );

                                if (futureCollision != null)
                                {
                                    break;
                                }

                                Pos lastPos = new Pos
                                {
                                    x = collisionObject.posX,
                                    y = collisionObject.posY,
                                };
                                collisionObject.initMove(lastPos, toPos);
                                this.movingObjects.Add(collisionObject);
                                this.sound.SetSFXClip(this.sound.push);
                                EngineManager.instance.SFX.Play();
                                break;
                            }
                        }
                        if (collisionObject.data.item.type == GameObjectType.WALL.GetDescription() && message != "")
                        {
                            if (this.engineType == EngineType.CCSR)
                            {
                                this.sound.once(this.sound.bump);
                            }
                            else if (this.engineType == EngineType.Scooby)
                            {
                                string[] bumpSounds = new string[] { "bump", "ruh_oh", "", "" };
                                int randIndex = (int)Mathf.Floor(UnityEngine.Random.Range(0, 1) * bumpSounds.Length);
                                // console.log(randIndex)
                                string randSound = bumpSounds[randIndex];
                                if (randSound != string.Empty)
                                {
                                    if (!EngineManager.instance.SFX.isPlaying)
                                    {
                                        this.sound.dynamicSoundOnce(randSound);
                                    }
                                }
                                this.sound.dynamicSoundOnce("bloop");
                            }
                            break;

                        }
                        break;

                    }
                case "ITEM":
                    {
                        this.removeGameObject(collisionObject);
                        break;
                    }
                case "WATER":
                    {
                        // only allow water travel if you have a boat
                        if (!this.inventory.has("scuba"))
                        {
                            this.sign.showMessage(this.gameData.walkIntoWater);

                        }
                        inWater = true;
                        break;
                    }
                case "DOOR":
                    {
                        string[] data = collisionObject.data.item.name.Split('=');
                        string action = data[0].ToUpper();

                        this.sound.SetSFXClip(this.sound.chimes);
                        EngineManager.instance.SFX.Play();

                        switch (action)
                        {
                            case "FRAME":
                                {
                                    this.playScene(data[1]);
                                    break;
                                }
                            case "ROOM":
                                {
                                    string coordsString = data[1];
                                    int[] coords = coordsString.Split('.').Select((dat) => int.Parse(dat)).ToArray();
                                    string mapX = coords[0].ToString().PadLeft(2, '0');
                                    string mapY = coords[1].ToString().PadLeft(2, '0');
                                    string map = mapX + mapY;
                                    int x = coords[2];
                                    int y = coords[3];
                                    this.script.onNewMap(map);
                                    this.setMap(map);
                                    this.player.setMapAndPosition(map, x, y);
                                    this.camera.snapCameraToMap(map);
                                    break;


                                }
                        }

                        Debug.Log(data);
                        break;
                    }
                default:
                    Debug.Log(collisionObject);
                    break;
            }


        }

        // TODO: possibly try to clean up this mess in the future
        // It just looks sloppy, but I wrote it this way to match
        // the logic of the original game.

        PlayerState newState = inWater ? PlayerState.BOAT : PlayerState.NORMAL;
        this.player.state = newState;

        // Update map and do bookkeeping when leaving a zone
        CC_GameObject nextMap = this.gameObjects.Find(
          (obj) =>
            obj.mapName != this.player.currentMap &&
            obj.isVisible() &&
            CC_Collision.Intersect(newPlayerRect, obj.getRect())
        );

        bool fullyInMap = CC_Collision.rectAinRectB(
          newPlayerRect,
          getMapRect(this.player.currentMap)
        );

        if (nextMap != null && !fullyInMap)
        {
            //console.log("from", this.player.currentMap, "to:", nextMap.mapName);
            pos = this.player.getPosition();
            CC_Types.Rect bounds = getMapRect(nextMap.mapName);

            float nextX = pos.x;
            float nextY = pos.y;

            if (nextX < bounds.x)
            {
                nextX = bounds.x + 16;
            }

            if (nextY < bounds.y)
            {
                nextY = bounds.y + 16;
            }

            if (nextX > bounds.x + bounds.width)
            {
                nextX = bounds.x + bounds.width - 16;
            }

            if (nextY > bounds.y + bounds.height)
            {
                nextY = bounds.y + bounds.height - 16;
            }

            Pos nextPos;

            if (this.camera.getMode() == CameraMode.PAN_BETWEEN_MAPS)
            {
                nextPos = new Pos{ x= nextX, y= nextY };

                // pan scooby as well
                if (this.engineType == EngineType.Scooby)
                {
                    Pos delta = new Pos(nextPos.x - pos.x, nextPos.y - pos.y);
                    delta.x -= pos.x;
                    delta.y -= pos.y;

                    // ugly hack to fix camera panning when moving north
                    // Idk why it's so broken but I don't care at all to understand it
                    // we're just gonna hotfix it
                    if (delta.y == 0 && delta.x == 0)
                    {
                        nextPos.y -= 32;
                        this.player.scoobySprite.transform.position = new Vector2(this.player.scoobySprite.transform.position.x, this.player.scoobySprite.transform.position.y - 32);
                    }

                    this.player.scoobySprite.transform.position = new Vector2(this.player.scoobySprite.transform.position.x + delta.x, this.player.scoobySprite.transform.position.y + delta.y);
                }
            }
            else
            {
                nextPos = new Pos{ x= newX, y= newY };
            }
            this.player.initMove(pos, nextPos);

            this.script.onNewMap(nextMap.mapName);

            this.player.lastMap = this.player.currentMap;
            this.player.currentMap = nextMap.mapName;
            this.resetMovableObjects(this.player.lastMap);
            this.camera.panToMap(this.player.currentMap);
        }
        else
        {
            Pos lastPos = this.player.getPosition();
            Pos nextPos = new Pos{ x= newX, y= newY };
        this.player.initMove(lastPos, nextPos);
    }

    int nextFrame = this.player.frameOfAnimation + 1;
    this.player.frameOfAnimation =
      nextFrame > this.player.getAnimationFrameCount()? 1 : nextFrame;

    if (dx > 0) this.player.characterDirection = PlayerDirection.RIGHT;
    if (dx< 0) this.player.characterDirection = PlayerDirection.LEFT;
    if (dx == 0)
      this.player.characterDirection = this.player.horizontalDirection;

    if (this.player.state == PlayerState.NORMAL)
      this.player.horizontalDirection = this.player.characterDirection;

    if (dy > 0) this.player.characterDirection = PlayerDirection.DOWN;
    if (dy< 0) this.player.characterDirection = PlayerDirection.UP;

    this.player.refreshTexture();
  }


    public void Update(float delta)
    {
       
            int now = DateTime.Now.Millisecond;
            CC_Scene.GameScene scene = this.currentScene;

            if (scene != null)
            {
                if (scene.isPlaying())
                {
                    if (now < scene.lastUpdate + scene.frameRate)
                    {
                        scene.tick(now);
                    }
                    else
                    {
                        scene.nextFrame(now);
                    }
                }
               
            }

        
        var moveablesList = new List<MovableGameObject>();
        moveablesList.Add(this.player);
        moveablesList.AddRange(this.movingObjects.FindAll(x => x.isStatic() == false));
        MovableGameObject[] moveables = moveablesList.ToArray();
        if (now < this.lastUpdate + this.MSperTick)
            {
                this.camera.tick();
                if (this.smoothAnimations)
                {
                    foreach(MovableGameObject obj in moveables) {
                        if (obj.inWalkingAnimation)
                        {
                            float endTime = obj.walkAnimStartMS + this.MSperTick;
                            float completed = this.MSperTick - (endTime - now);
                            float percentage = completed / this.MSperTick;
                            float dx = percentage * (obj.nextPos.x - obj.lastPos.x);
                            float dy = percentage * (obj.nextPos.y - obj.lastPos.y);
                            if ((obj as CC_GameObject) != null)
                            {
                                (obj as CC_GameObject).sprite.SetSpritePos(new Vector2(obj.lastPos.x + dx, obj.lastPos.y + dy));
                            }
                        }
                    }
                }

                if (
                  this.camera.getMode() == CameraMode.CENTER_ON_PLAYER &&
                  this.player.inWalkingAnimation
                )
                {
                    this.camera.centerCameraOnPlayer();
                }

                
            }

            foreach(MovableGameObject obj in moveables) {
                if (obj.inWalkingAnimation)
                {
                    if ((obj as CC_GameObject) != null)
                    {
                        (obj as CC_GameObject).endMove();

                    }
                }
            }

            // Remove any non auto-walking entities
            this.movingObjects = this.movingObjects.FindAll(
              (obj) => obj.data.move.COND == GameObjectMoveCond.AUTO
            );

            List<CC_GameObject> pushers = this.movingObjects.FindAll(
              (o) => o.data.move.COND == GameObjectMoveCond.PUSH
            );

            if (pushers.Count == 0)
            {
                //this.sound.push.pause();
            }

            this.lastUpdate = now;

            // Update film loop objects with next frame texture
            this.updateFilmLoopObjects();

            if (this.player.status == PlayerStatus.MOVE)
            {
                this.updateAutoMoveObjects();
            }

            if (this.introScreen.inIntro)
            {
                if (Input.GetKey(KeyCode.Return))
                {
                    this.introScreen.close(this);
                }
                
            }

            if (this.sign.isOpen() || this.inventory.isOpen())
            {
                if (this.sign.isOpen())
                {
                    if (Input.GetKey(KeyCode.Return))
                    {
                        this.sign.closeMessage();
                    }
                }
                if (this.inventory.isOpen())
                {
                    if (Input.GetKey(KeyCode.Return))
                    {
                        this.inventory.closeInventory();
                    }
                }
            }
            else
            {
                if (!this.player.inWalkingAnimation && this.player.status == PlayerStatus.MOVE)
                {
                    float left =
                      Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") == -1 ? -1 : 0;
                    float right =
                       Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") == 1  ? 1 : 0;
                    float up =
                        Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") == 1 ? -1 : 0;
                    float down =
                        Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") == -1 ? 1 : 0;
                    
                        this.movePlayer(left + right, up + down);
                }
                if (Input.GetKey(KeyCode.Return))
                {
                    this.inventory.openInventory();
                }
            }

    }

    public string getMap()
    {
        return this.currentMap;
    }
    private void drawObjectToBackground(CC_GameObject obj)
    {
        this.backgroundSprite = obj.sprite;
        this.backgroundSprite.Visible(true);
         
    }
    private void init(string episode)
    {
        this.player = new CC_Player(this);
        this.player.init();
        this.initObjects();
        this.initWorldInfo();
        //scripts
        this.episode = episode;
        switch (episode)
        {
           /* case "2":
                this.script = new Episode2(this);
                break;
            case "3":
                this.script = new Episode3(this);
                break;
            case "4":
                this.script = new Episode4(this);
                break;
            case "scooby-1":
                this.script = new Scooby1(this);
                break;
            case "scooby-2":
                this.script = new Scooby2(this);
                break;*/
            default:
                this.script = new CC_Episode1(this);
                break;
        }
        this.script.init();
        this.initRenderObjects();

        
        this.sign.init();
        this.inventory.init();

        this.gameData = EngineManager.instance.GetGameData(EngineManager.instance.assets.Find(x => x.name == "game")); 

        if (this.engineType == EngineType.CCSR)
            this.inventory.initItems(this.gameData.inventory);

       /* this.app.renderer.addListener("resize", () => {
            this.resize();
        });*/

       

        this.introScreen.init(this);
    }
    
    private void initObjects()
    {
       
        EngineManager.instance.assets.ForEach((x) =>
        {
            if (x.name == "map")
            {
               area = EngineManager.instance.GetGameMapAreaJson(x);
            }
        });
        List<GameMes> messages = new List<GameMes>();
        EngineManager.instance.assets.ForEach((x) =>
        {
            if (x.name == "message")
            {
                messages = EngineManager.instance.GetGameMessages(x);
            }
        });
        // Convert all objects in map data to GameObjects
        foreach(GameMapArea area in this.area.data) {
            // Do not load unused maps.
            // They have a suffix and are longer than 4 characters
            if (area.name.Length > 4)
            {
                continue;
            }

            foreach(IGameObject obj in area.data) {
                // Either the parser is bugged or there is a bug in the map data
                // for episode 2, there's an object which is bad. Skip past it.
                if (obj.member == null)
                {
                    continue;
                }

                // Replace the message text with the translation for the chosen language
                if (messages.Count > 0)
                {
                    foreach(GameObjectMessage msg in obj.data.message) {

                        string hashHex = "";
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(msg.text));
                            hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            if (hashHex != "")
                            {
                                hashHex.Substring(0, messages[0].ToString().Length);

                            }
                        }
                       
                        Debug.Log("HASH: "+ hashHex);
                        Debug.Log("MSG: "+ msg.text);
                        msg.text = messages.Find(x => x[hashHex] == x[hashHex]).ToString();
                        
                      
                    }
                }

                CC_GameObject gameObject = new CC_GameObject(obj, area.name);
                this.gameObjects.Add(gameObject);
            }
        }
    }
    private void initRenderObjects()
    {
       
        this.gameObjects.Reverse();

        this.updateAllVisibility();

       
        this.movingObjects = this.gameObjects.FindAll(obj => obj.data.move.COND == GameObjectMoveCond.AUTO);

        Debug.Log("Game objects: " + this.gameObjects.Count);
       
    }
    private void initWorldInfo()
    {
        HashSet<string> mapSet = new HashSet<string>();
        foreach (var obj in gameObjects)
        {
            mapSet.Add(obj.mapName);
        }
        if (mapSet.Count > 0)
        {
            int xMax = mapSet.Select(s => int.Parse(s.Substring(0, 2))).Max();
            int yMax = mapSet.Select(s => int.Parse(s.Substring(2, 2))).Max();

            numMapsX = xMax;
            numMapsY = yMax;

            worldRect = new CC_Types.Rect
            {
                x = 0,
                y = 0,
                width = numMapsX * MAP_WIDTH,
                height = numMapsY * MAP_HEIGHT
            };
        }
        
    }
    public static Texture2D ConvertWhiteToTransparent(Texture2D oldTexture)
    {
        

        
        Texture2D newTexture = new Texture2D(oldTexture.width, oldTexture.height, TextureFormat.RGBA32, false);

       
        for (int y = 0; y < oldTexture.height; y++)
        {
            for (int x = 0; x < oldTexture.width; x++)
            {
              
                Color pixelColor = oldTexture.GetPixel(x, y);

                
                if (pixelColor.r == 1f && pixelColor.g == 1f && pixelColor.b == 1f)
                {
                    
                    newTexture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    
                    newTexture.SetPixel(x, y, pixelColor);
                }
            }
        }

        // Apply the changes to the new texture
        newTexture.Apply();

        // Update the sprite texture in the SpriteRenderer
        return newTexture;
    }
    public static Texture2D getMemberTexture(string memeberName, string subFolder = "character.visuals")
        {
            // Load the PNG file from the specified file path
            string name = memeberName.ToLower();
            name = name.Replace(".png", "");
            name = name.Replace(".x", "");
            subFolder = "img";
            //get translation sata
            // Create a new Texture2D
            Texture2D texture = Resources.Load<Texture2D>("game/" + EngineManager.instance.episode + "/" + subFolder + "/" + name);
            texture = ConvertWhiteToTransparent(texture);

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
    public static CC_Types.Rect getMapsRect(string topLeft, string bottomRight){
        CC_Types.Rect TL = getMapRect(topLeft);
        CC_Types.Rect BR = getMapRect(bottomRight);
     
            int xs =
                int.Parse(bottomRight.Substring(0, 2)) - int.Parse(topLeft.Substring(0, 2)) + 1;
            int ys =
              int.Parse(bottomRight.Substring(2, 2)) - int.Parse(topLeft.Substring(2, 2)) + 1;

            float width = xs * TL.width;
            float height = ys * TL.height;

            CC_Types.Rect result = new CC_Types.Rect
            {
                x = TL.x,
                y = TL.y,
                width = 0,
                height = 0,
            };
            return result;

        
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
    public static float randBetween(float min, float max)
    {
        return Mathf.Floor(UnityEngine.Random.Range(0, 1) * (max - min + 1) + min);
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
    private void removeGameObject(CC_GameObject obj)
    {
        
        int index = this.gameObjects.FindIndex((o) => o == obj);
        this.gameObjects.RemoveAt(index);
        GameObject.Destroy(obj.sprite);
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
    private void resetMovableObjects(string mapName)
    {
        this.gameObjects
          .FindAll(
            (obj) =>
              obj.data.move.COND == GameObjectMoveCond.PUSH &&
              obj.mapName == mapName &&
              (obj.posX != obj.originalPosX || obj.posY != obj.originalPosY)
          )
          .ForEach((obj) => obj.setPosition(obj.originalPosX, obj.originalPosY));
    }
}
