using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_Types;

/**
 * Generic class for a game Object.
 *
 * Every object in a game shares common properties,
 * whether they are used or not, the structure
 * of every object in the game is the same.
 */

public class CC_GameObject : IGameObject, MovableGameObject
{
    public static readonly int[,] MOVE_DIRECTIONS =
    {
        {0, -1},
        {-1, 0},
        {0, 1},
        {1, 0}
    };
    private string _member;
    public string member {
        get => _member; set => _member = value;
    }
    private float[,] _location;
    public GameObjectType type {
        get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); 
    }
    public float[,] location {
        get => _location; set => _location = value; 
    }
    private float _width;
    public float width {
        get => _width; set => _width = value; 
    }
    private float _height;
    public float height {
        get => _height; set => _height = value;
    }
    private float _wshift, _hshift;
    public float WSHIFT {
        get => _wshift; set => _wshift = value;
    }
    public float HSHIFT {
        get => _hshift; set => _hshift = value;
    }
    private GameObjectData _gdata;
    public GameObjectData data { 
        get => _gdata; set => _gdata = value;
    }

    public readonly string mapName;
    readonly float mapOffsetX;
    readonly float mapOffsetY;

    public float posX;
    public float posY;

    public readonly float originalPosX;
    public readonly float originalPosY;

    private bool visible = true;
    public GameObject baseObj;
    public CC_SpriteGame sprite;
    public float speed = 8;
    private bool inWalkingAnimation = false;
    private float walkAnimStartMS = 0;
    public Pos lastPos, nextPos;
    float MovableGameObject.speed
        {
            get
            {
               return speed;
            }
            set
            {
                speed = value;
            }
        }

    bool MovableGameObject.inWalkingAnimation 
    {
        get
        {
            return inWalkingAnimation;
        }
        set
        {
            inWalkingAnimation = value;
        }
    }
    float MovableGameObject.walkAnimStartMS {
        get
        {
            return walkAnimStartMS;
        }
        set
        {
            walkAnimStartMS = value;
        }
    }
    Pos MovableGameObject.lastPos {
        get
        {
            return lastPos;
        }
        set
        {
            lastPos = value;
        }
    }
    Pos MovableGameObject.nextPos {
        get
        {
            return nextPos;
        }
        set
        {
            nextPos = value;
        }
    }

 

    public float moveDirection = -1;
    public Pos movePos = new Pos(0,0);
      

/*
Some objects in the original game were "Film Loops"
which is just a keyframed animation wrapped up in a cast member.
We can mimic this behavior by telling the game which objects
are animated on startup, and what their animation frames are,
and have the engine update the textures of those every frame.
*/
    public int frame = 0;
    public int frameIndex = 0;
    public bool isFrameObject = false;

    public CC_GameObject(IGameObject obj, string mapName) {
        //base obj contains renderer
        this.member = obj.member.ToLower();
        this.type = obj.type;
        this.location = obj.location;
        this.width = obj.width;
        this.height = obj.height;
        this.WSHIFT = obj.WSHIFT;
        this.HSHIFT = obj.HSHIFT;
        this.data = obj.data;
        this.mapName = mapName;
        float[,] offset = CC_Game.getMapOffset(this.mapName);
        this.mapOffsetX = offset[0, 0]; // Access the element at row 0, column 0
        this.mapOffsetY = offset[0, 1]; // Access the element at row 0, column 1
        float offsetX = this.mapOffsetX * 32 * 13;
        float offsetY = this.mapOffsetY * 32 * 10;
        this.posX = this.location[0, 0] * 16 + offsetX + this.WSHIFT;
        this.posY = this.location[0, 1] * 16 + offsetY + this.HSHIFT;
        if (!this.member.Contains("tile"))
        {
            this.posX -= (int)Mathf.Round(this.width / 2);
            this.posY -= (int)Mathf.Round(this.height / 2);
        }

        // HACK to fix broken floor tile in hotel in episodes 2, 3, and 4
        // It spans across map boundaries.
        // In the original game this probably was cut off or didn't render at all,
        // but in the remake where everything is rendered at once, this bad tile
        // causes problems visually and breaks map collision detection
        // So let's just shove it down one tile's space so it is fully in its map
        if (this.posX == 2608 && this.posY == 1248 && this.member == "block.119")
        {
            this.posY += 32;
        }

        this.originalPosX = this.posX;
        this.originalPosY = this.posY;

        this.lastPos = new Pos(this.originalPosX, this.originalPosY);
        this.nextPos = this.lastPos;
        this.movePos = this.lastPos;

        Texture2D objTexture = CC_Game.getMemberTexture(this.member);

        // make  this object a tiling sprite if it includes "tile"
        // and the width/height is bigger than the texture



        if (this.member.ToLower().Contains("tile"))
        {
            string tileRepeat = this.member.ToLower();

            sprite.spriteRenderer.drawMode = SpriteDrawMode.Tiled;

        }


        // HACK to fix ocean walls on the north eastern side
        // of the maps in episodes 2-4
        // they spill over into other levels, so they need to be adjusted
        if (this.member.Contains("tile.1.x") && this.data.item.type == GameObjectType.WALL)
        {
            sprite.SetSpritePos(new Vector2(baseObj.transform.position.x - 16, baseObj.transform.position.y));
            this.width = 16;
           
        }
        this.sprite = new CC_SpriteGame(Sprite.Create(objTexture, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(width, height)), Vector2.one * 0.5f));
        sprite.SetSpritePos(new Vector2(this.posX, this.posY));

    }
    //END OF CON
    public void initMove(Pos fromPos, Pos toPos)
    {
        this.inWalkingAnimation = true;
        this.lastPos = fromPos;
        this.nextPos = toPos;
        this.walkAnimStartMS = DateTime.Now.Millisecond;
    }



    public void endMove()
    {
        this.inWalkingAnimation = false;
        baseObj.transform.position = new Vector2(this.nextPos.x, this.nextPos.y);
    }

    public CC_Types.Rect getMoveBounds()
    {
        // bounds for each side seem to be taken from
        // the edge of the sprite + n * 16
        // where n is the move number
        GameObjectMove move = this.data.move;

        int t = 16;

        float top = this.originalPosY - move.U * t;
        float bottom = this.originalPosY + this.height + move.D * t;

        float left = this.originalPosX - move.L * t;
        float right = this.originalPosX + this.width + move.R * t;

        CC_Types.Rect bounds = new CC_Types.Rect {
            x= left,
            y= top,
            height= bottom - top,
            width = right - left,
        };

        return bounds;
    }

    public bool isVisible()
    {
        return this.visible;
    }

    public void setVisible(bool isVisible) 
    {
        this.visible = isVisible;
        this.sprite.spriteRenderer.enabled = isVisible;
    }

    public void setPosition(int x, int y)
    {
        this.posX = x;
        this.posY = y;
        baseObj.transform.position = new Vector2(x, y);
    }

    public CC_Types.Rect getRect()
    {
        return new CC_Types.Rect{
        x = this.posX,
        y= this.posY,
        width= this.width,
        height= this.height,
        };
    }

/**
* Determines if the game object will ever move or not.
* Only dynamic objects will be added to the PIXI scene graph.
* Having thousands of objects in the scene graph which never
* update just slows down the rendering and adds unnecessary work.
*
* @returns true if the game object will ever move or disappear
*/
    public bool isStatic()
    {
        // Always include these types of objects' sprites in the scene
        List <GameObjectType> dynamicTypes = new List<GameObjectType>
        {
            GameObjectType.CHAR, // characters
            GameObjectType.ITEM, // items
        };

        if (dynamicTypes.Contains(this.data.item.type))
        {
            return false;
        }

        if (this.data.move.COND != GameObjectMoveCond.NONE)
        {
            return false;
        }

        if ((this.data.item.visi).VALUE.Insert((this.data.item.visi).VALUE.Length - 1,"") != "")
        {
            return false;
        }

        if (this.isFrameObject)
        {
            return false;
        }
            return true;
        }
    }
