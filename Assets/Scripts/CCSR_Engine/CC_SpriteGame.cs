using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CC_SpriteGame : MonoBehaviour
{
    public Sprite spriteData;
    public SpriteRenderer spriteRenderer;
    public Button spriteButton;
    public CC_SpriteGame(Sprite image)
    {
        spriteData = image;
        spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
        spriteButton = this.gameObject.AddComponent<Button>();
        spriteRenderer.sprite = spriteData;

    }
    public CC_SpriteGame()
    {
        spriteData = null;
        spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();
        spriteButton = this.gameObject.AddComponent<Button>();
    }
    public void SetSprite(Sprite image)
    {
        spriteData = image;

        spriteRenderer.sprite = spriteData;
    }
    public void SetSpritePos(Vector2 pos)
    {
        transform.position = pos;
    }
    public void Visible(bool isVisible)
    {
        spriteRenderer.enabled = isVisible;
        spriteButton.enabled = isVisible;
    }
}
