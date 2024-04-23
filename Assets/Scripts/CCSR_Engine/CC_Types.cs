using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.ComponentModel;
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

    [Serializable]

    public class GameData
    {
        public string walkIntoWater;
        public string noItems;
        public GameMessages scene;
        public List<GameInventoryItemData> inventory = new List<GameInventoryItemData>();
    }
    

    public class GameMessages
    {
        public string message;
    }
    public delegate void setPosition(int x, int y);
    public delegate void initMove(Pos fromPos, Pos toPos);
    public delegate void endMove();

    public interface MovableGameObject
    {

        int speed { get; set; }
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
        public IGameObject[] data;
        public string roomID;
        public int roomStatus;
        public string name;

    }
    [Serializable]

    public class GameMapAreaDataContainer
    {
        public List<GameMapArea> data = new List<GameMapArea>();
    }

    [Serializable]

    public enum GameObjectType
    {
        [Description("FLOR")]
        FLOR,// = "FLOR",
        [Description("WALL")]

        WALL, //= "WALL",
        [Description("CHAR")]

        CHAR, //= "char",
        [Description("ITEM")]

        ITEM, //= "item",
        [Description("DOOR")]


        DOOR, //= "DOOR",
        [Description("WATER")]

        WATER, //= "WATER",
    }
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute == null ? value.ToString() : attribute.Description;
    }
    [Serializable]
    public class Location
    {
        public int x { get; set; }
        public int y { get; set; }

        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    [Serializable]
    public class IGameObject
    {
        public string member;
        public string type;
        public int[] location = new int[2];
        public int width;
        public int WSHIFT;
        public int height;
        public int HSHIFT;
        public GameObjectData data;
    }
    [Serializable]

    public class GameObjectData
    {
        public GameObjectItem item;
        public GameObjectMove move;
        public GameObjectMessage[] message;
    }
    [Serializable]

    public class GameObjectMessage
    {
        public string text;
        public string plrAct;
        public string plrObj;
    }
    [Serializable]
    public class GameObjectItem
    {
        public string name;
        public string type;
        public GameObjectVisibility visi;
        public GameObjectCond[] COND = new GameObjectCond[4];
    }
    [Serializable]
    public class GameObjectCond
    {
        public string hasObj;
        public string hasAct;
        public string giveObj;
        public string giveAct;
    }
    [Serializable]

    public class GameObjectMove
    {
        public int U;
        public int D;
        public int L;
        public int R;
        public GameObjectMoveCond COND ;
        public int TIMEA;
        public int TIMEB;
}
    [Serializable]

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
    [Serializable]

    public class GameObjectVisibility
    {
        public string visiObj;
        public string visiAct;
        public string inviObj;
        public string inviAct;
        private string VALUE
        {
            get { return this.visiObj + this.visiAct + this.inviObj + this.inviAct; }
        }
        public string getValue()
        {
            return VALUE;
        }
    }

}
