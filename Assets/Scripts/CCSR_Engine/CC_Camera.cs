using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraMode
{
    PAN_BETWEEN_MAPS,
    CENTER_ON_PLAYER,
}

public class CC_Camera 
{
    //connect to Camera.Main
    private CC_Game game;
    private float mapWidthPixels = 0;
    private float mapHeightPixels = 0;
    private Camera mainCam;
    private float screenWidth = 0;
    private float screenHeight = 0;

    public float scaleX = 0;
    public float scaleY = 0;

    private CC_Types.Pos currentCameraPos = new CC_Types.Pos(0, 0);
    private CC_Types.Pos nextCameraPos = new CC_Types.Pos(0, 0);

    private bool isPanning = false;
    private float panStartMS = 0;
    private float panEndMS = 0;

    private CC_Types.Rect cameraBounds;

    private CameraMode cameraMode = CameraMode.PAN_BETWEEN_MAPS;

    public CC_Camera(CC_Game game)
    {
        Camera mainCam = Camera.main;
        this.game = game;
        EngineManager.instance.tickEvent.AddListener(this.tick);
        this.cameraBounds = new CC_Types.Rect
        {
            height = 0,
            width = 0,
            x = 0,
            y = 0,
        };
        
    }
    public void setCameraMode(CameraMode mode)
    {
        this.cameraMode = mode;
        
    }
    public void setScale()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
        float w = (int)(Screen.width);
        float h = (int)(Screen.height);
        screenWidth = w;
        screenHeight = h;

        CC_Types.Rect rect = CC_Game.getMapRect(game.player.currentMap);
        mapHeightPixels = (int)rect.height;
        mapWidthPixels = (int)rect.width;

        float scaleX = w / (int)rect.width;
        float scaleY = h / (int)rect.height;
        float scale = (w > h) ? scaleY : scaleX;

        mainCam.rect = new Rect(new Vector2(mainCam.rect.x, mainCam.rect.y),new Vector2(scale, scale));
        scaleX = scale;
        scaleY = scale;
    }
    public CameraMode getMode()
    {
        return this.cameraMode;
    }
    public void panToMap(string nextMap)
    {
        if (this.getMode() != CameraMode.PAN_BETWEEN_MAPS)
        {
            return;
        }
     
        this.game.player.SetStatus(PlayerStatus.STOP);

        this.isPanning = true;
       
        CC_Types.Pos lastPos = this.currentCameraPos;
        this.nextCameraPos = this.getMapCameraXY(nextMap);
       

        float deltaX = this.nextCameraPos.x - lastPos.x;

        float panSpeedX = (416 / 16) * 12 + 100;
        float panSpeedY = (320 / 16) * 12 + 100;
        float panTimeMS = deltaX == 0 ? panSpeedX : panSpeedY;

        float now = DateTime.Now.Millisecond;
        this.panStartMS = now;
        this.panEndMS = now + panTimeMS;
    }
    public void centerCameraOnPlayer()
    {
        Debug.Break();

        CC_Types.Pos pos = this.game.player.getPosition();

        int pX = (int)(-pos.x * this.scaleX);
        int pY = (int)(-pos.y * this.scaleY);

        int width = (int)(Screen.width);
        int height = (int)(Screen.height);

        int hW = (int)(Mathf.Round(width / 2));
        int hH = (int)(Mathf.Round(height / 2));

        int cX = pX + hW;
        int cY = pY + hH;

        /*
        const bounds = this.cameraBounds;

        let cameraX = cX;
        let cameraY = cY;

        if (Math.abs(pX) - hW < 0) {
          cameraX = 0;
        }

        if (Math.abs(pY) - hH < 0) {
          cameraY = 0;
        }

        if (Math.abs(pX) + hW > bounds.width * this.scaleX) {
          cameraX = (bounds.width * this.scaleX - hW * 2) * -1;
        }

        if (Math.abs(pY) + hH > bounds.height * this.scaleY) {
          cameraY = (bounds.height * this.scaleY - hH * 2) * -1;
        }

        */
        this.setCamera(cX, cY);
    }
    public void tick()
    {
        if (this.isPanning)
        {
            //console.log("panning!");
            int now = DateTime.Now.Millisecond;

            if (now > this.panEndMS)
            {
                this.isPanning = false;
                this.game.player.SetStatus(PlayerStatus.MOVE);
                this.setCamera((int)this.nextCameraPos.x, (int)this.nextCameraPos.y);
                this.currentCameraPos = this.nextCameraPos;
                return;
            }

            float diff = this.panEndMS - now;
            float totalTime = this.panEndMS - this.panStartMS;
            float percentage = diff / totalTime;
            float dx = percentage * (this.nextCameraPos.x - this.currentCameraPos.x);
            float dy = percentage * (this.nextCameraPos.y - this.currentCameraPos.y);

            CC_Types.Pos p = this.nextCameraPos;
            this.setCamera((int)(p.x - dx), (int)(p.y - dy));
            return;
        }
    }
    public void snapCameraToMap(string mapName)
    {
        if (this.getMode() == CameraMode.CENTER_ON_PLAYER)
        {
            this.centerCameraOnPlayer();
            return;
        }

        CC_Types.Pos pos = this.getMapCameraXY(mapName);
        
        this.currentCameraPos = pos;
        this.setCamera((int)pos.x, (int)pos.y);
    }
    public void setCameraBounds(string mapTopLeft, string mapBottomRight)
    {
        CC_Types.Rect TL = CC_Game.getMapRect(mapTopLeft);
        CC_Types.Rect BR = CC_Game.getMapRect(mapBottomRight);

        this.cameraBounds = new CC_Types.Rect{
            x = TL.x,
            y =TL.y,
            width = BR.x - TL.x + CC_Game.MAP_WIDTH,
            height = BR.y - TL.y + CC_Game.MAP_HEIGHT,
        };

    }
    private CC_Types.Pos getMapCameraXY(string mapName)
    {
        CC_Types.Rect data = CC_Game.getMapRect(mapName);

        float x = (-data.x * mainCam.rect.size.x);
        float y = (-data.y * mainCam.rect.size.y);

        float w = (Screen.width);
        float h = (Screen.height);

        float mapWidth = this.mapWidthPixels * this.scaleX;
        float mapHeight = this.mapHeightPixels * this.scaleY;

        float padX = this.screenWidth - mapWidth;
        float padY = this.screenHeight - mapHeight;

            // only make special adjustments to the camera if the world is bigger than our
            // user's screen
            if (this.cameraBounds.width > w) {
                if (w > h)
                {
                    int half = (int)(Mathf.Round(padX / 2));
                    int test = (int)(Mathf.Abs(x) - half);
                    // Only center the map if no area outside the world would be seen
                    if (test >= 0)
                    {
                        x += half;
                        float worldWidth = this.cameraBounds.width * this.scaleX;
                        float finalX = Mathf.Abs(x) + this.screenWidth;
                        if (finalX > worldWidth)
                        {
                            x += finalX - worldWidth;
                        }
                    }
                }
                else
                {
                    float half = (int)(Mathf.Round(padY / 2));
                    float test = Mathf.Abs(y) - half;
                    if (test >= 0)
                    {
                        y += half;
                        float worldHeight = this.cameraBounds.height * this.scaleY;
                        float finalY = Mathf.Abs(y) + this.screenHeight;
                        if (finalY > worldHeight)
                        {
                            y += finalY - worldHeight;
                        }
                    }
                }
            } else {
                // center map on screen
                x += this.screenWidth / this.scaleX / 2;
            }

            float newX = (int)Mathf.Round(x);
            float newY = (int)Mathf.Round(y);

            return new CC_Types.Pos(newX,newY);
  }

    private void setCamera(int x, int y)
    {
        Debug.Log("X: "+ x+ " Y: "+ y);
        this.mainCam.transform.position = new Vector3(x, y, -10);
    }
}
