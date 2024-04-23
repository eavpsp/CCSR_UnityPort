using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CC_SpriteUI : MonoBehaviour
{
    public Sprite spriteData;
    public Image spriteRenderer;
    public Button spriteButton;
    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = this.gameObject.AddComponent<Image>();
            
        }
        if (spriteButton == null)
        {
            spriteButton = this.gameObject.AddComponent<Button>();
        }
        this.transform.SetParent(EngineManager.instance.mainCanvas.transform);

    }
   
    public void Visible(bool isVisible)
    {
        spriteRenderer.enabled = isVisible;
        spriteButton.enabled = isVisible;
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
}

