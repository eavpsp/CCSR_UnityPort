using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_Types;

public enum PlayerStatus
{
    MOVE,
    TALK,
    READ,
    STOP,
}

public enum PlayerState
{
    [EnumStringValue("player.normal")]
    NORMAL,
    [EnumStringValue("player.boat")]
    BOAT
}

[AttributeUsage(AttributeTargets.Field)]
public class EnumStringValueAttribute : Attribute
{
    public string Value { get; }

    public EnumStringValueAttribute(string value)
    {
        Value = value;
    }
}

public static class PlayerStateExtensions
{
    public static string GetStringValue(this PlayerState state)
    {
        var fieldInfo = state.GetType().GetField(state.ToString());
        var attribute = (EnumStringValueAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(EnumStringValueAttribute));
        return attribute != null ? attribute.Value : state.ToString();
    }
}

public enum PlayerDirection
{
    [EnumStringValue("left")]
    LEFT ,//= "left",
    [EnumStringValue("right")]
    RIGHT,// = "right",
    [EnumStringValue("up")]
    UP,// = "up",
    [EnumStringValue("down")]
    DOWN //= "down",
}


public static class PlayerDirectionExtensions
{
    public static string GetStringValue(this PlayerDirection state)
    {
        var fieldInfo = state.GetType().GetField(state.ToString());
        var attribute = (EnumStringValueAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(EnumStringValueAttribute));
        return attribute != null ? attribute.Value : state.ToString();
    }
}

public class CC_Player : MovableGameObject
{
    private int speed;
    private bool inWalkingAnimation = false;
    private float walkAnimStartMS = 0;
    private Pos lastPos = new Pos(0, 0);
    private Pos nextPos = new Pos(0, 0);
    int MovableGameObject.speed {
        get => speed; set => speed = value;
    }
    bool MovableGameObject.inWalkingAnimation {
        get => inWalkingAnimation; set => inWalkingAnimation = value;
    }
    float MovableGameObject.walkAnimStartMS {
        get => walkAnimStartMS; set => walkAnimStartMS = value;
    }
    Pos MovableGameObject.lastPos {
        get => lastPos; set => lastPos = value;
    }
    Pos MovableGameObject.nextPos {
        get => nextPos; set => nextPos = value;
    }

    private CC_Game game;
    public CC_SpriteGame gameSprite;
    public CC_SpriteGame scoobySprite;
    public PlayerStatus status;
    public int animNum;
    public float lastMove;
    public PlayerState state;
    public PlayerDirection horizontalDirection;
    public PlayerDirection characterDirection;
    public PlayerDirection scoobyDirection;
    public int frameOfAnimation;
    private float posX;
    private float posY;
    public string currentMap;
    public string lastMap = "0101";
    public GameObject baseObject;


    public CC_Player(CC_Game game)
    {
        this.game = game;
        //  this.gameSprite = new Sprite();
        //this.scoobySprite = new Sprite();
        this.status = PlayerStatus.MOVE;
        this.speed = 8;
        this.animNum = 1;
        this.lastMove = 0;
        this.state = PlayerState.NORMAL;
        this.horizontalDirection = PlayerDirection.RIGHT;
        this.characterDirection = PlayerDirection.RIGHT;
        this.scoobyDirection = PlayerDirection.RIGHT;
        this.frameOfAnimation = 1;
        this.posX = 0;
        this.posY = 0;
    }

    public void init()
    {
        Texture2D newTex = CC_Game.getMemberTexture(this.getTextureString());
        this.gameSprite = new GameObject("player").AddComponent<CC_SpriteGame>();
        this.gameSprite.SetSprite(Sprite.Create(newTex, new UnityEngine.Rect(new Vector2(this.posX, this.posY), new Vector2(newTex.width, newTex.height)), Vector2.one * 0.5f));

        if (this.game.engineType == CC_Game.EngineType.Scooby)
        {
            Texture2D newScoobTex = CC_Game.getMemberTexture("scooby.down.1");
            this.scoobySprite = new GameObject("scooby").AddComponent<CC_SpriteGame>();
            this.scoobySprite.SetSprite(Sprite.Create(newScoobTex, new UnityEngine.Rect(new Vector2(this.posX, this.posY), new Vector2(newTex.width, newTex.height)), Vector2.one * 0.5f));

        }
    }
    public void SetStatus(PlayerStatus stat)
    {
        this.status = stat;
        if (this.status == PlayerStatus.MOVE)
        {
            this.game.updateAllVisibility();
        }

    }
    public void setPosition(int x, int y)
    {
        this.posX = x;
        this.posY = y;
        this.gameSprite.SetSpritePos(new Vector2(this.posX, this.posY));

    }

    public void initMove(Pos fromPos, Pos toPos)
    {
        this.inWalkingAnimation = true;
        this.lastPos = fromPos;
        this.nextPos = toPos;
        this.walkAnimStartMS = DateTime.Now.Millisecond;

        // Only CCSR has footstep sounds
        if (this.game.engineType == CC_Game.EngineType.CCSR)
        {
            EngineManager.instance.SFX.Stop();

            if (this.state == PlayerState.BOAT)
            {
                this.game.sound.SetSFXClip(this.game.sound.boat);
            }
            else
            {
                this.game.sound.SetSFXClip(this.game.sound.walk);

            }
            EngineManager.instance.SFX.Play();
        }
    }

    public void endMove()
    {
        this.inWalkingAnimation = false;
        this.setPosition(this.nextPos.x, this.nextPos.y);

        if (this.game.camera.getMode() == CameraMode.CENTER_ON_PLAYER)
        {
            this.game.camera.centerCameraOnPlayer();
        }

        if (this.game.engineType == CC_Game.EngineType.Scooby)
        {
            this.updateScooby();
        }

        EngineManager.instance.SFX.Stop();
    }

    public Pos getPosition() {
        return new Pos((int)this.gameSprite.transform.position.x, (int)this.gameSprite.transform.position.y);

    }
    public void setMapAndPosition(string map, int xIndex, int yIndex)
    {
        this.lastMap = this.currentMap;
        this.currentMap = map;
        CC_Types.Rect offset = CC_Game.getMapRect(map);
        int x = xIndex * 16 + offset.x;
        int y = yIndex * 16 + offset.y;
        this.setPosition(x, y);
        this.inWalkingAnimation = false;
        this.lastPos = new Pos(x, y);
        this.nextPos = new Pos(x, y);

        if (this.game.engineType == CC_Game.EngineType.Scooby)
        {
            this.scoobyDirection = PlayerDirection.RIGHT;
            this.baseObject.transform.position = new Vector2(this.posX - 32, this.posY);
            this.updateScooby();
        }
    }

    private bool isPerpendicular(PlayerDirection dir)
    {
        // console.log("scoob: ", this.scoobyDirection, "shag: ", dir)
        switch (this.scoobyDirection)
        {
            case PlayerDirection.UP:
            case PlayerDirection.DOWN:
                {
                    return (PlayerDirection.LEFT.GetStringValue() +" "+ PlayerDirection.RIGHT.GetStringValue()).Contains(dir.GetStringValue());
                }
            case PlayerDirection.LEFT:
            case PlayerDirection.RIGHT:
                {
                    return (PlayerDirection.UP.GetStringValue() + " " + PlayerDirection.DOWN.GetStringValue()).Contains(dir.GetStringValue());
                }
            default: return false;
        }

    }

    private Pos getScoobyOffset(PlayerDirection thisDir, bool isPerpendicular)
    {


        /*
          the Y offsets used to be +/-17 instead of 16,
          but this caused a bug in the conditional in a different function,
          causing it to be off by one and make scooby alternate between up/down
          when walking sideways. Changing the Y value fixes this issue.
          This whole scooby code is a giant dumpster fire anyway,
          but we want to support those games and they aren't coming out with more,
          so who cares
        */

        Dictionary<string, Pos> scoobyOffsetList = new Dictionary<string, Pos>
            {
                { "left" , new Pos(48, 16) },
                {"top",  new Pos(0, 60) },
                { "right",  new Pos(-48, 16) },
                { "pLeft",  new Pos(48, 16)},
                { "bottom",  new Pos(0, -60)},
                { "pRight",  new Pos(-48, 16)},
                {  "pBottom",  new Pos(0, 0)},
                {  "pTop",  new Pos(0, 0)},
            };
        if (!isPerpendicular)
        {
            switch (thisDir)
            {
                case PlayerDirection.LEFT: return scoobyOffsetList["left"];
                case PlayerDirection.UP: return scoobyOffsetList["top"];
                case PlayerDirection.RIGHT: return scoobyOffsetList["right"];
                case PlayerDirection.DOWN: return scoobyOffsetList["bottom"];
            }
        }
        else
        {
            switch (thisDir)
            {
                case PlayerDirection.LEFT: return scoobyOffsetList["pLeft"];
                case PlayerDirection.UP: return scoobyOffsetList["pTop"];
                case PlayerDirection.RIGHT: return scoobyOffsetList["pRight"];
                case PlayerDirection.DOWN: return scoobyOffsetList["pBottom"];
            }
        }
        return new Pos(0, 0);
    }
    public void updateScooby()
    {

        Pos thisLoc = this.nextPos;
        PlayerDirection thisDir = this.characterDirection;
        int thisFrame = this.frameOfAnimation;
        float thisSpeed = this.speed;
        CC_Types.Rect thisRect = this.getCollisionRectAtPoint(thisLoc.x, thisLoc.y);

        bool isPerpendicular = this.isPerpendicular(thisDir);
        Pos thisOffset = this.getScoobyOffset(thisDir, isPerpendicular);
        // console.log("\n scooby dir:", this.scoobyDirection)
        // console.log("chardir: ", this.characterDirection, "is perp:", isPerpendicular)
        // console.log("offset", thisOffset)

        // If scooby's collision rectangle isn't in the players, move him
        if (this.scoobyCanMove(thisRect))
        {
            // console.log("thisLoc: ", thisLoc)
            Pos thisDelta = this.scoobyGetDelta(thisLoc, thisOffset);
            Vector2 scooby = this.baseObject.transform.position;
            Pos newLoc = new Pos((int)(scooby.x + thisDelta.x), (int)(scooby.y + thisDelta.y));
            // console.log("delta:", thisDelta)
            thisRect = this.getCollisionRectAtPoint(newLoc.x + thisDelta.x, newLoc.y + thisDelta.y);
            PlayerDirection newDir = this.scoobyGetDir(thisDelta);
            
            this.scoobyDirection = newDir;
            Texture2D newTex = this.getScoobyTexture(newDir, thisFrame);
            this.scoobySprite.SetSprite(Sprite.Create(newTex, new UnityEngine.Rect(new Vector2(this.posX, this.posY), new Vector2(newTex.width, newTex.height)), Vector2.one * 0.5f));
            this.baseObject.transform.position = new Vector2(scooby.x += thisDelta.x, scooby.y += thisDelta.y);
            
        }
        else
        {
            // sprite(me.spriteNum).locZ = me.spriteNum
            // me.pDelta = thisSpeed
        }
    }

    public void refreshTexture()
    {
        if (this.gameSprite.spriteData == null)
        {
            string texStr = this.getTextureString();
            Texture2D texture = CC_Game.getMemberTexture(texStr);
            this.gameSprite.SetSprite(Sprite.Create(texture, new UnityEngine.Rect(new Vector2(this.posX, this.posY), new Vector2(texture.width, texture.height)), Vector2.one * 0.5f));
        }

    }
    private bool scoobyCanMove(CC_Types.Rect thisRect)
    {
        Vector2 scoob = this.baseObject.transform.position;
        CC_Types.Rect scoobRect = this.getCollisionRectAtPoint((int)scoob.x, (int)scoob.y);
        return !CC_Collision.Intersect(scoobRect, thisRect);
    }
    private Pos scoobyGetDelta(Pos shaggyLoc, Pos thisOffset)
    {
        Vector2 scooby = this.baseObject.transform.position;
        Pos thisDelta = new Pos(0, 0);
        int thisUnit = 8;
        Pos thisLoc = new Pos(shaggyLoc.x + thisOffset.x, shaggyLoc.y + thisOffset.y);
        Pos myLoc = new Pos((int)scooby.x, (int)scooby.y);



        if (myLoc.x < thisLoc.x)
        {
            thisDelta.x += thisUnit;
        }
        else
        {
            if (myLoc.x > thisLoc.x)
            {
                thisDelta.x -= thisUnit;
            }
            else
            {
                thisDelta.x = 0;
            }
        }

        if (myLoc.y > thisLoc.y)
        {
            thisDelta.y -= thisUnit;
        }
        else
        {
            if (myLoc.y < thisLoc.y)
            {
                thisDelta.y += thisUnit;
            }
            else
            {
                thisDelta.y = 0;
            }
        }

        return thisDelta;
    }

    private PlayerDirection scoobyGetDir(Pos delta)
    {
        if (delta.y > 0)
            return PlayerDirection.DOWN;
        if (delta.y < 0)
            return PlayerDirection.UP;
        if (delta.x > 0)
            return PlayerDirection.RIGHT;
        if (delta.x < 0)
            return PlayerDirection.LEFT;
    return 0;
  }



public CC_Types.Rect getCollisionRectAtPoint(float x, float y)  
    {
    int padding = 2;
    int w = Mathf.Min(32, (int)this.gameSprite.spriteData.rect.width);
    int h = Mathf.Min(32, (int)this.gameSprite.spriteData.rect.height);

    CC_Types.Rect result = new CC_Types.Rect
    {
        x = (int)(x - Mathf.Round(w / 2) + padding),
        y = (int)(y - Mathf.Round(h / 2) + padding),
        width = w - padding * 2,
        height = h - padding * 2,
    };

    if (this.game.engineType == CC_Game.EngineType.Scooby) {
        result.x -= padding;
        result.y -= padding;
        result.width = w;
        result.height = h;

        // move to bottom half of 64 height sprite
        result.y += 16;
    };

        return result;
    }
    private string getTextureString()
    {
        string normal = this.state.GetStringValue() + "." + this.horizontalDirection.GetStringValue() + "." + this.frameOfAnimation;
        string boat = this.state.GetStringValue() + "." + this.characterDirection.GetStringValue();

        if (this.state != PlayerState.NORMAL)
        {
          
            if (this.game.engineType == CC_Game.EngineType.Scooby)
            {
                string shaggyState = this.state.GetStringValue() + "." + this.characterDirection.GetStringValue() +"." + this.frameOfAnimation;
                return shaggyState + ".png";
            }

            return boat + ".png";
        }

      
        if (this.game.engineType == CC_Game.EngineType.Scooby)
        {
            string shaggyState = this.state.GetStringValue() + "." + this.characterDirection.GetStringValue() + "." + this.frameOfAnimation;
            return shaggyState + ".png";
        }

        return normal + ".png";
    }

    public int getAnimationFrameCount() 
    {
        return (this.game.engineType == CC_Game.EngineType.Scooby) ? 3 : 2;
    }

    private Texture2D getScoobyTexture(PlayerDirection thisDir, int thisFrame)
    {
        string textureString = "scooby." +thisDir +"."+ thisFrame;
       
        return CC_Game.getMemberTexture(textureString);

    }
}
