using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CC_SpriteGame : MonoBehaviour
{
    public Sprite spriteData;
    public SpriteRenderer spriteRenderer;
    public Button spriteButton;
    public CC_SpriteGame(Sprite image, SpriteRenderer renderer, Button button = null)
    {
        spriteData = image;
        spriteRenderer = renderer;
        spriteButton = button;
        spriteRenderer.sprite = spriteData;

    }
    public CC_SpriteGame()
    {
        spriteData = null;
        spriteRenderer = null;
        spriteButton = null;
    }
    public void SetSprite(Sprite image)
    {
        spriteData = image;

        spriteRenderer.sprite = spriteData;
    }
    public void SetSpritePos(Vector2 pos)
    {
        spriteRenderer.transform.position = pos;
    }
}
