using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CC_SpriteGame : MonoBehaviour
{
    public Sprite spriteData;
    public SpriteRenderer spriteRenderer;


    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();

        }
        
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
  
    }
}
