using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static CC_Inventory;
public static class CC_Types 
{
    public enum Key
    {
        UP ,//= "ArrowUp",
        DOWN,// = "ArrowDown",
        LEFT, //= "ArrowLeft",
        RIGHT,// = "ArrowRight",
        ENTER,// = "Enter",
        SPACE,// = " ",
        W ,//= "w",
        A ,//= "a",
        S ,//= "s",
        D //= "d",
    }

    public class FilmLoopTexture
    {
        public string[] loopTextures;
        public float delay;
    }

    public delegate void FilmLoopCallback(CC_GameObject gameObject);

    public interface FilmLoop
    {
        FilmLoopTexture texture { get; set; }
        FilmLoopCallback callback { get; set; }
    }

    public interface FilmLoopData
    {
        FilmLoop this[string key] { get; set; }
    }


    public class GameData
    {
        public string walkIntoWater;
        public string noItems;
        public GameMessages scene;
        public List<GameInventoryItemData> inventory = new List<GameInventoryItemData>();
    }

    public interface GameMessages
    {
        GameMessages this[string key] { get;set; }
    }
    public delegate void setPosition(int x, int y);
    public delegate void initMove(Pos fromPos, Pos toPos);
    public delegate void endMove();

    public interface MovableGameObject
    {

        float speed { get; set; }
         bool inWalkingAnimation { get; set; }
        float walkAnimStartMS { get; set; }
        Pos lastPos { get; set; }
        Pos nextPos { get; set; }
    }

    public struct Pos
    {
        public Pos(float _x, float _y)
        {
            x = _x;
            y = _y;
        }
       public float x;
        public float y;
    }

    public class Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }
    [Serializable]
    public class GameMapArea
    {
        public string roomID;
        public int roomStatus;
        public IGameObject[] data;
    }

    public  enum GameObjectType
    {
        FLOR,// = "FLOR",
        WALL, //= "WALL",
        CHAR, //= "char",
        ITEM, //= "item",
        DOOR, //= "DOOR",
        WATER, //= "WATER",
    }

    public class IGameObject
    {
        public string member { get; set; }
        public GameObjectType type { get; set; }
        public float[,] location { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float WSHIFT { get; set; }
        public float HSHIFT { get; set; }
        public GameObjectData data { get; set; }
    }

    public class GameObjectData
    {
        public GameObjectItem item;
        public GameObjectMove move;
        public GameObjectMessage[] message;
    }

    public class GameObjectMessage
    {
        public string plrAct;
        public string plrObj;
        public string text;
    }

    public class GameObjectItem
    {
        public string name;
        public GameObjectType type;
        public GameObjectVisibility visi;
        public GameObjectCond[] COND;
    }

    public class GameObjectCond
    {
        public string hasObj;
        public string hasAct;
        public string giveObj;
        public string giveAct;
    }

    public class GameObjectMove
    {
       public int U;
        public int D;
        public int L;
        public int R;
        public GameObjectMoveCond COND ;
        public float TIMEA;
        public float TIMEB;
}

    public enum GameObjectMoveCond
    {
        NONE = 1,
        AUTO,
        PUSH,
        // The following conditions are in the engine but are not implemented
        // and are not present in the map data
        // PULL
        // MOVEX
        // MOVEY
    }

    public class GameObjectVisibility
    {
        public string visiObj;
        public string visiAct;
        public string inviObj;
        public string inviAct;
        public string VALUE
        {
            get { return this.visiObj + this.visiAct + this.inviObj + this.inviAct; }
        }
    }

}
