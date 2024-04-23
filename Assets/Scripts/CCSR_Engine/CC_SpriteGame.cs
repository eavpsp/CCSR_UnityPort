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
        this.gameObject.transform.localScale = new Vector2(3,3);
    }
    public void SetSprite(Sprite image)
    {
        spriteData = image;

        spriteRenderer.sprite = spriteData;
    }
    public void SetSpritePos(Vector2 pos)
    {
        transform.position = new Vector2(Mathf.Round(pos.x /32) ,-Mathf.Round(pos.y /32));
    }
    public void Visible(bool isVisible)
    {
        spriteRenderer.enabled = isVisible;
  
    }
}
