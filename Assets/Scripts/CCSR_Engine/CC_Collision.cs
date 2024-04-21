using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CC_Types;

public static class CC_Collision
{
    public static bool Intersect(CC_Types.Rect a, CC_Types.Rect b) {
        float x = Mathf.Max(a.x, b.x);
        float n1 = Mathf.Min(a.x + a.width, b.x + b.width);
        float y = Mathf.Max(a.y, b.y);
        float n2 = Mathf.Min(a.y + a.height, b.y + b.height);
        return n1 > x && n2 > y;
    }

    public static bool rectAinRectB(CC_Types.Rect rectA , CC_Types.Rect rectB) 
    {
        CC_Types.Rect a = rectA;
        Pos[] points = {
            new Pos(a.x, a.y),
            new Pos(a.x + a.width, a.y ),
            new Pos(a.x, a.y + a.height ),
            new Pos(a.x + a.width, a.y + a.height )
        };
        return points.All(p => pointInRect(p, rectB));
    }

    public static bool pointInRect(Pos point, CC_Types.Rect rect)
    {
        return (
          point.x >= rect.x &&
          point.x <= rect.x + rect.width &&
          point.y >= rect.y &&
          point.y <= rect.y + rect.height
        );
    }
}
