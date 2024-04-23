using System;
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
        //EngineManager.instance.tickEvent.AddListener(Update);
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
                obj.moveDirection = randBetween(0, CC_GameObject.MOVE_DIRECTIONS.Length - 1);
            }

            // If we have reached our destination
            if (obj.posX == obj.movePos.x && obj.posY == obj.movePos.y)
            {
                float i = obj.moveDirection + 1;
                obj.moveDirection = i >= CC_GameObject.MOVE_DIRECTIONS.Length ? 0 : i;
                CC_Types.Rect bounds = obj.getMoveBounds();
                float dx =
                  randBetween(0, Mathf.Floor(bounds.width / obj.speed) * obj.speed);
                float dy =
                  randBetween(0, Mathf.Floor(bounds.width / obj.speed) * obj.speed);
                Pos movePos = new Pos
                (
                      obj.posX + CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection,0] * dx,
                      obj.posY + CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection, 1] * dy
                );
            obj.movePos = movePos;
        } else
        {
                // We are not where we want to be, try to step forward.
                int dx = CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection, 0];
                int dy = CC_GameObject.MOVE_DIRECTIONS[(int)obj.moveDirection, 1];
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
    //TODO
    /*
    public void movePlayer(float dx, float dy)
    {
        
            this.debug.updateItemText();

            // player isn't moving, stop the walking sound
            if (dx == 0 && dy == 0)
            {
                // TODO
                return;
            }

            const pos = this.player.getPosition();
            const newX = pos.x + dx * this.player.speed;
            const newY = pos.y + dy * this.player.speed;

            // TODO

            // Check to see if we need to scroll the map
            // This should be a toggleable setting in the future

            // Check interaction with game objects
            // Search through objects in reverse order
            // Get the first object that we collide with

            // This could be a tiny bug if the player's texture is
            //  supposed to change and it's not a 32x32 size
            const newPlayerRect = this.player.getCollisionRectAtPoint(newX, newY);

            if (!rectAinRectB(newPlayerRect, this.worldRect!))
            {
                return;
            }

            const collisionObject = this.gameObjects.find(
              (obj) => obj.isVisible() && intersect(newPlayerRect, obj.getRect())
            );

            if (collisionObject === undefined)
            {
                console.log("No game object was found where you tried to walk!");
                return;
            }

            // Determine if this object has a message to show.
            const messages = collisionObject.data.message;

            // TODO: possibly try to clean up this mess in the future
            // It just looks sloppy, but I wrote it this way to match
            // the logic of the original game.
            let message = "";

            if (messages.length > 0)
            {
                console.log(collisionObject);
                // Find the message which is relavant to our player's state
                for (const m of messages) {
                    if (!m.plrAct && !m.plrObj)
                    {
                        message = m.text;
                        continue;
                    }
                    if (!m.plrObj && m.plrAct)
                    {
                        if (this.inventory.has(m.plrAct))
                        {
                            message = m.text;
                            //break;
                        }
                        continue;
                    }
                    if (m.plrObj && !m.plrAct)
                    {
                        if (this.inventory.has(m.plrObj))
                        {
                            message = m.text;
                            //break;
                        }
                        continue;
                    }
                    if (m.plrObj && m.plrAct)
                    {
                        if (this.inventory.has(m.plrObj) && this.inventory.has(m.plrAct))
                        {
                            message = m.text;
                            //break;
                        }
                    }
                }
            }

            if (message)
            {
                if (
                  collisionObject.data.item.type == GameObjectType.CHAR ||
                  collisionObject.data.item.type == GameObjectType.ITEM
                )
                {
                    this.sign.showCharacterMessage(collisionObject.member, message);
                }
                else
                {
                    this.sign.showMessage(message);
                }
            }

            const conds = collisionObject.data.item.COND.filter(
              (c): c is GameObjectCond => c !== null
    );

            let getItem = false;
            let condIndex = -1;

            if (conds.length > 0)
            {
                for (const c of conds) {
                    condIndex++;
                    if (!c.hasObj && !c.hasAct)
                    {
                        // console.log("!hasObj && !hasAct");
                        getItem = true;
                        continue;
                    }
                    if (!c.hasObj && c.hasAct)
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
                    if (c.hasObj && !c.hasAct)
                    {
                        // console.log("hasObj && !hasAct");
                        if (this.inventory.has(c.hasObj))
                        {
                            getItem = true;
                            this.inventory.removeItem(c.hasObj);
                        }
                        break;
                    }
                    if (c.hasObj && c.hasAct)
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
                const c = conds[condIndex];
                if (
                  c.giveObj &&
                  !this.inventory.has(c.giveObj) &&
                  !this.inventory.has("got" + c.giveObj)
                )
                {
                    this.inventory.addItem(c.giveObj);

                    // fugly scooby hack
                    if (this.engineType === EngineType.Scooby)
                    {
                        if (c.giveObj === "nobats")
                        {
                            this.sound.soundBank["bunch_o_bats"].stop();
                        }
                    }

                    this.sign.setOnClose(() => {
                        // for some reason I have to wrap this in a timeout
                        // or else the inventory won't display...
                        // stupid bugs, but who cares at this point
                        // I'm not going to be the one who figures out
                        // why it doesn't work the normal way
                        setTimeout(() => {
                            this.inventory.openInventory(c.giveObj);
                        }, 1);
                    });
                }
                if (c.giveAct && !this.inventory.has(c.giveAct))
                {
                    this.inventory.addAct(c.giveAct);
                }
            }

            let inWater = false;

            
            const name = collisionObject.data.item.name;
            if (name && !name.includes("="))
            {
                if (!this.inventory.has(name))
                {
                    this.inventory.names.push(name);
                    this.inventory.addAct("got" + name);
                }
            }

            // Switch over object type and handle each case differently
            switch (collisionObject.data.item.type)
            {
                case GameObjectType.FLOR:
                    break;
                case GameObjectType.CHAR:
                case GameObjectType.WALL:
                    {
                        if (collisionObject.data.move.COND == GameObjectMoveCond.PUSH)
                        {
                            const toPos = this.posAfterDeltaMove(collisionObject, dx, dy);
                            if (this.canMoveGameObject(collisionObject, toPos))
                            {
                                // Make sure the player doesn't collide with something
                                // in his new coordinates that isn't the push object
                                const futureCollision = this.gameObjects.find(
                                  (obj) =>
                                    obj.isVisible() &&
                                    intersect(newPlayerRect, obj.getRect()) &&
                                    obj.data.item.type == GameObjectType.WALL &&
                                    obj != collisionObject
                                );

                                if (futureCollision)
                                {
                                    return;
                                }

                                const lastPos = {
              x: collisionObject.posX,
              y: collisionObject.posY,
            };
                            collisionObject.initMove(lastPos, toPos);
                            this.movingObjects.push(collisionObject);
                            this.sound.push.play();
                            break;
                        }
                    }
                    if (collisionObject.data.item.type == GameObjectType.WALL && !message)
                    {
                        if (this.engineType === EngineType.CCSR)
                        {
                            this.sound.once(this.sound.bump);
                        }
                        else if (this.engineType === EngineType.Scooby)
                        {
                            const bumpSounds = ["bump", "ruh_oh", undefined, undefined];
                            const randIndex = Math.floor(Math.random() * bumpSounds.length);
                            // console.log(randIndex)
                            const randSound = bumpSounds[randIndex];
                            if (randSound !== undefined)
                            {
                                if (!this.sound.soundBank["bump"].playing() &&
                                  !this.sound.soundBank["ruh_oh"].playing())
                                {
                                    this.sound.dynamicSoundOnce(randSound);
                                }
                            }
                            this.sound.dynamicSoundOnce("bloop");
                        }
                    }
                    return;
            }
      case GameObjectType.ITEM: {
                this.removeGameObject(collisionObject);
                break;
            }
      case GameObjectType.WATER: {
                // only allow water travel if you have a boat
                if (!this.inventory.has("scuba"))
                {
                    this.sign.showMessage(this.gameData!.walkIntoWater);
                    return;
                }
                inWater = true;
                break;
            }
      case GameObjectType.DOOR: {
                const data = collisionObject.data.item.name.split("=");
                const action = data[0].toUpperCase();

                this.sound.chimes.play();

                switch (action)
                {
                    case "FRAME":
                        {
                            this.playScene(data[1]);
                            return;
                        }
                    case "ROOM":
                        {
                            const coords = data[1].split(".").map((x) => parseInt(x));
                            const mapX = coords[0].toString().padStart(2, "0");
                            const mapY = coords[1].toString().padStart(2, "0");
                            const map = mapX + mapY;
                            const x = coords[2];
                            const y = coords[3];
                            this.script.onNewMap(map);
                            this.setMap(map);
                            this.player.setMapAndPosition(map, x, y);
                            this.camera.snapCameraToMap(map);

                            return;
                        }
                }

                console.log(data);
                break;
            }
            default:
        console.log(collisionObject);
            break;
        }

        const newState = inWater ? PlayerState.BOAT : PlayerState.NORMAL;
        this.player.state = newState;

        // Update map and do bookkeeping when leaving a zone
        const nextMap = this.gameObjects.find(
          (obj) =>
            obj.mapName !== this.player.currentMap &&
            obj.isVisible() &&
            intersect(newPlayerRect, obj.getRect())
        );

        const fullyInMap = rectAinRectB(
          newPlayerRect,
          getMapRect(this.player.currentMap)
        );

        if (nextMap && !fullyInMap)
        {
            //console.log("from", this.player.currentMap, "to:", nextMap.mapName);
            const pos = this.player.getPosition();
            const bounds = getMapRect(nextMap.mapName);

            let nextX = pos.x;
            let nextY = pos.y;

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

            let nextPos: Pos;

            if (this.camera.getMode() == CameraMode.PAN_BETWEEN_MAPS)
            {
                nextPos = { x: nextX, y: nextY };

                // pan scooby as well
                if (this.engineType === EngineType.Scooby)
                {
                    const delta = { ...nextPos };
                    delta.x -= pos.x;
                    delta.y -= pos.y;

                    // ugly hack to fix camera panning when moving north
                    // Idk why it's so broken but I don't care at all to understand it
                    // we're just gonna hotfix it
                    if (delta.y === 0 && delta.x === 0)
                    {
                        nextPos.y -= 32;
                        this.player.scooby.y -= 32
                    }

                    this.player.scooby.x += delta.x;
                    this.player.scooby.y += delta.y;
                }
            }
            else
            {
                nextPos = { x: newX, y: newY };
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
            const lastPos = this.player.getPosition();
            const nextPos = { x: newX, y: newY };
        this.player.initMove(lastPos, nextPos);
    }

    const nextFrame = this.player.frameOfAnimation + 1;
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

    }
    public void Update()
    {
        private update(delta: number) {
            const now = Date.now();
            const scene = this.currentScene;

            if (scene)
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
                return;
            }

            const moveables: MovableGameObject[] = [this.player, ...this.movingObjects];

            if (now < this.lastUpdate + this.MSperTick)
            {
                this.camera.tick();
                if (this.smoothAnimations)
                {
                    for (const obj of moveables) {
                        if (obj.inWalkingAnimation)
                        {
                            const endTime = obj.walkAnimStartMS + this.MSperTick;
                            const completed = this.MSperTick - (endTime - now);
                            const percentage = completed / this.MSperTick;
                            const dx = percentage * (obj.nextPos.x - obj.lastPos.x);
                            const dy = percentage * (obj.nextPos.y - obj.lastPos.y);
                            obj.setPosition(obj.lastPos.x + dx, obj.lastPos.y + dy);
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

                return;
            }

            for (const obj of moveables) {
                if (obj.inWalkingAnimation)
                {
                    obj.endMove();
                }
            }

            // Remove any non auto-walking entities
            this.movingObjects = this.movingObjects.filter(
              (obj) => obj.data.move.COND == GameObjectMoveCond.AUTO
            );

            const pushers = this.movingObjects.filter(
              (o) => o.data.move.COND == GameObjectMoveCond.PUSH
            );

            if (pushers.length == 0)
            {
                this.sound.push.pause();
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
                if (this.keyPressed(Key.ENTER))
                {
                    this.introScreen.close(this);
                }
                return;
            }

            if (this.sign.isOpen() || this.inventory.isOpen())
            {
                if (this.sign.isOpen())
                {
                    if (this.keyPressed(Key.ENTER))
                    {
                        this.sign.closeMessage();
                    }
                }
                if (this.inventory.isOpen())
                {
                    if (this.keyPressed(Key.ENTER))
                    {
                        this.inventory.closeInventory();
                    }
                }
            }
            else
            {
                if (
                  !this.player.inWalkingAnimation &&
                  this.player.status == PlayerStatus.MOVE
                )
                {
                    const left =
                      this.keyPressed(Key.LEFT) || this.keyPressed(Key.A) ? -1 : 0;
                    const right =
                      this.keyPressed(Key.RIGHT) || this.keyPressed(Key.D) ? 1 : 0;
                    const up = this.keyPressed(Key.UP) || this.keyPressed(Key.W) ? -1 : 0;
                    const down =
                      this.keyPressed(Key.DOWN) || this.keyPressed(Key.S) ? 1 : 0;
                    this.movePlayer(left + right, up + down);
                }
                if (this.keyPressed(Key.ENTER))
                {
                    this.inventory.openInventory();
                }
            }
            this.keysPressed.delete(Key.ENTER);
            this.keysPressed.delete(Key.SPACE);
        }

    }
*/

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
    //
    private void initObjects()
    {
       
        EngineManager.instance.assets.ForEach((x) =>
        {
            if (x.name == "map")
            {
               area = EngineManager.instance.GetGameMapAreaJson(x);
            }
        });
        List<GameMessages> messages = new List<GameMessages>();
        EngineManager.instance.assets.ForEach((x) =>
        {
            if (x.name == "messagaes")
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
               /* if (messages != null)
                {
                    foreach(GameObjectMessage msg in obj.data.message) {
                        byte[] msgBytes = Encoding.UTF8.GetBytes(msg.text);
                        byte[] hashBytes;
                        string msgHash = "";
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            hashBytes = sha256.ComputeHash(msgBytes);
                        };
                         msgHash = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, messages.message.Length);
                         msgHash = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, messages.message.Length);

                          msg.text = messages.message;
                        
                      
                    }
                }*/

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
