using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CC_Types;

public class CC_Game 
{
    public static float MAP_WIDTH = 416;
    public static float MAP_HEIGHT = 320;

    public static float UI_WIDTH_PERCENT = 0.6f;
    public static float UI_HEIGHT_PERCENT = 0.6f;


    public CC_Player player;
    public CC_GameObject[]  gameObjects;
    public CC_GameObject[] movingObjects;

    public float numMapsX = 0;
    public float numMapsY = 0;
    public CC_Types.Rect worldRect;
    public CC_Sound.GameSound sound;
    public CC_Inventory.GameInventory inventory;
    public CC_Camera camera;

    public enum EngineType
    {
        CCSR,
        Scooby
            //
    }
    public EngineType engineType;
    public static float[,] getMapOffset(string mapName)
    {
        float xIndex = float.Parse(mapName.Substring(0, 2));
        float yIndex = float.Parse(mapName.Substring(2, 4));

        // Subtract 1 to convert to zero based indexing.
        return   new float[(int)((xIndex) - 1), (int)((yIndex) - 1)];
         
    }
    public static CC_Types.Pos getMapOffset(string mapName, int i = 0)
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

    public static int randBetween(int min, int max)
    {
        return (int)Mathf.Floor(Random.Range(0, 1) * (max - min + 1) + min);
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

}
