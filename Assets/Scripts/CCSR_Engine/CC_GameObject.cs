using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_Types;

[Serializable]
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
    private int[] _location;
    private string _Type;
    public string type {
        get => _Type; set => _Type = value; 
    }
    public int[] location {
        get => _location; set => _location = value; 
    }
    private int _width;
    public int width {
        get => _width; set => _width = value; 
    }
    private int _height;
    public int height {
        get => _height; set => _height = value;
    }
    private int _wshift, _hshift;
    public int WSHIFT {
        get => _wshift; set => _wshift = value;
    }
    public int HSHIFT {
        get => _hshift; set => _hshift = value;
    }
    private GameObjectData _gdata;
    public GameObjectData data { 
        get => _gdata; set => _gdata = value;
    }

    public readonly string mapName;
    readonly int mapOffsetX;
    readonly int mapOffsetY;

    public float posX;
    public float posY;

    public readonly float originalPosX;
    public readonly float originalPosY;

    private bool visible = true;
    public CC_SpriteGame sprite;
    public int speed = 8;
    private bool inWalkingAnimation = false;
    private float walkAnimStartMS = 0;
    public Pos lastPos, nextPos;
    int MovableGameObject.speed
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
      
    public int frame = 0;
    public int frameIndex = 0;
    public bool isFrameObject = false;

    public CC_GameObject(IGameObject obj, string mapName) {
        this.member = obj.member.ToLower();
        this.type = obj.type;
        this.location = obj.location;
        this.width = obj.width;
        this.height = obj.height;
        this.WSHIFT = obj.WSHIFT;
        this.HSHIFT = obj.HSHIFT;
        this.data = obj.data;
        this.mapName = mapName;
        int[] offset = CC_Game.getMapOffset(this.mapName);
        this.mapOffsetX = offset[0];
        this.mapOffsetY = offset[1];
        int offsetX = this.mapOffsetX * 32 * 13;
        int offsetY = this.mapOffsetY * 32 * 10;
        this.posX = this.location[0] * 16 + offsetX + this.WSHIFT;
        this.posY = this.location[1] * 16 + offsetY + this.HSHIFT;
        if (!this.member.Contains("tile"))
        {
            this.posX -= Mathf.Round(this.width / 2);
            this.posY -= Mathf.Round(this.height / 2);
        }
        if (this.posX == 2608 && this.posY == 1248 && this.member == "block.119")
        {
            this.posY += 32;
        }

        this.originalPosX = this.posX;
        this.originalPosY = this.posY;

        this.lastPos = new Pos(this.originalPosX, this.originalPosY);
        this.nextPos = this.lastPos;
        this.movePos = this.lastPos;

        Texture2D objTexture;
        //Clean up sorting issues
        this.sprite = new GameObject(member).AddComponent<CC_SpriteGame>();
        if (this.member.ToLower().Contains("tile"))
        {

            sprite.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            objTexture = CC_Game.getMemberTexture(this.member + ".png", "map.tiles");
            sprite.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        }
        else
        {
            
             objTexture = CC_Game.getMemberTexture(this.member +".png", "map.visuals");

        }

        if (this.member.ToLower().Contains("tile.1.x") && this.data.item.type == GameObjectType.WALL.GetDescription())
        {
            this.width = 16;
            this.posX -= 16;
        }

        this.sprite.SetSprite(Sprite.Create(objTexture, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(objTexture.width, objTexture.height)), Vector2.one * 0.5f));
        sprite.SetSpritePos(new Vector2(this.posX, this.posY));
        sprite.spriteRenderer.sortingOrder = 1;
        if (this.member.ToLower().Contains("tile") )
        {
      

            sprite.spriteRenderer.size = new Vector2(this.width /32, this.height /32);
            sprite.spriteRenderer.sortingOrder = 0;
            if (this.member.ToLower().Contains("tile.2"))
            {
               
              sprite.spriteRenderer.sortingOrder = -1;
            }
        }
        
       



    }
 
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
       sprite.transform.position = new Vector2(this.nextPos.x, this.nextPos.y);
    }

    public CC_Types.Rect getMoveBounds()
    {
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
        sprite.transform.position = new Vector2(x, y);
    }
    public void setPosition(float x, float y)
    {
        this.posX = x;
        this.posY = y;
        sprite.transform.position = new Vector2(x, y);
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

    public bool isStatic()
    {
        
        List <string> dynamicTypes = new List<string>
        {
            GameObjectType.CHAR.GetDescription(), // characters
            GameObjectType.ITEM.GetDescription(), // items
        };

        if (dynamicTypes.Contains(this.data.item.type))
        {
            return false;
        }

        if (this.data.move.COND != GameObjectMoveCond.NONE)
        {
            return false;
        }

        if ((this.data.item.visi).getValue().Insert((this.data.item.visi).getValue().Length - 1,"") != "")
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
