using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_Episode1 : CC_Script
{
    
    public CC_Episode1(CC_Game game) : base(game)
    {
        this.game = game;
    }
    public override void onNewMap(string nextMap) { }

    public override void init() {
        CC_Types.Rect m = CC_Game.getMapsRect("0101", "0606");
   
        this.game.camera.setCameraBounds("0101", "0606");

        string startMap = "0106";
        this.game.setMap(startMap);
        this.game.player.setMapAndPosition(startMap, 6, 10);

        // imitate the starting position in original episode
        // starts facing left, but facing right after first move
        this.game.player.horizontalDirection = PlayerDirection.LEFT;
        this.game.player.refreshTexture();
        this.game.player.horizontalDirection = PlayerDirection.RIGHT;

        this.game.camera.setScale();
        this.game.camera.snapCameraToMap(startMap);
        this.game.addScene("ending", new CC_Scene1(this.game));
        /*
        // DEBUG STUFF
        const i = [
          "bandaid",
          "ducktape",
          "gum",
          "tape",
          "sock",
          "firstaid", //
          "hammer",
          "chocbar",
          "sunscreen",
          "wrench",
          "tennis",
        ];
        i.map((x) => this.game.inventory.addItem(x));
        this.game.playScene("ending");
        */
  }
}
