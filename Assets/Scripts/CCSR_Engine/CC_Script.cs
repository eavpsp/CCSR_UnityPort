using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_Script 
{
    public CC_Game game;

    public virtual void init() { }
    public virtual void onNewMap(string nextMap) { }

    CC_Script(CC_Game game)
    {
        this.game = game;
    }
}
