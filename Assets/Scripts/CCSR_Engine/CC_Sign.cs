using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CC_Sign 
{
    private CC_Game game;
    private CC_SpriteUI sprite;
    private CC_SpriteUI characterSprite;
    public TextMeshProUGUI textElement;
    private bool adaptiveScale, isMessageShowing, isCharacterMessage;
    private float originalHeight, originalWidth;
    private CC_Game.EngineType engine;

    private UnityEvent onCloseCallback;
    private float scale = 1;
        
    
    public CC_Sign(CC_Game game, CC_Game.EngineType engine)
    {
        this.engine = engine;
        this.game = game;
        this.textElement = new GameObject("textElement_Sign").AddComponent<TextMeshProUGUI>();
        this.textElement.transform.SetParent(EngineManager.instance.mainCanvas.transform);
        this.textElement.text = "";
        this.adaptiveScale = true;
        this.sprite = new GameObject("sign_sprite").AddComponent<CC_SpriteUI>();
        this.sprite.transform.SetParent(EngineManager.instance.mainCanvas.transform);
        this.characterSprite = new GameObject("char sprite").AddComponent<CC_SpriteUI>();
        this.sprite.transform.SetParent(EngineManager.instance.mainCanvas.transform);

    }
    public bool isOpen()
    {
        return this.isMessageShowing;
    }
    public void SetOnClose(UnityEvent eventData)
    {
        onCloseCallback = eventData;
    }
    public void init()
    {
        Texture2D spriteTexture = CC_Game.getMemberTexture("sign.bkg.png", "message.visuals");
        this.sprite.SetSprite(Sprite.Create(spriteTexture, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(spriteTexture.width, spriteTexture.height)), Vector2.one * 0.5f));
        this.sprite.Visible(true);
        this.sprite.spriteButton.onClick.AddListener(delegate
        {
            this.closeMessage();
        });

        this.originalHeight = this.sprite.spriteData.rect.height;
        this.originalWidth = this.sprite.spriteData.rect.width;
        this.resize();
    }
    public void showMessage(string message)
    {
        this.game.sound.SetSFXClip(this.game.sound.message);
        EngineManager.instance.SFX.Play();
        this.game.inventory.closeInventory();
        this.game.player.SetStatus(PlayerStatus.READ);
        this.isCharacterMessage = false;
        this.isMessageShowing = true;
        this.setTextDimensions(true);
        Texture2D spriteTexture = CC_Game.getMemberTexture("sign.bkg.png", "message.visuals");
        this.sprite.SetSprite(Sprite.Create(spriteTexture, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(spriteTexture.width, spriteTexture.height)), Vector2.one * 0.5f));
        this.sprite.Visible(true);
        this.textElement.text = message;

    }
    public void showCharacterMessage(string charName, string message)
    {
        this.game.sound.once(this.game.sound.message);
        this.game.inventory.closeInventory();
        this.game.player.SetStatus(PlayerStatus.READ);
        this.isCharacterMessage = true;
        this.isMessageShowing = true;
        this.setTextDimensions(false);
        Texture2D spriteTexture = CC_Game.getMemberTexture("talk.bkg");
        this.sprite.SetSprite(Sprite.Create(spriteTexture, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(spriteTexture.width, spriteTexture.height)), Vector2.one * 0.5f));

        this.sprite.Visible(true);

        Texture2D charSpriteTexture = CC_Game.getMemberTexture(charName + ".face");
        this.characterSprite.SetSprite(Sprite.Create(charSpriteTexture, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(charSpriteTexture.width, charSpriteTexture.height)), Vector2.one * 0.5f));

        this.characterSprite.Visible(true);

        this.textElement.text = message;
    }
    private void setTextDimensions(bool isSign)
    {
        float width = isSign ? 242 : 165;
        float height = isSign ? 136 : 160;

        float l = isSign ? 30 : 120;
        float t = isSign ? 34 : 18;

        // tweak for scooby
        if (this.engine == CC_Game.EngineType.Scooby)
        {
            if (!isSign)
            {
                l -= 10;
                width -= 8;
                height -= 5;
            }
            else
            {
                l += 30;
                width -= 30;
                height += 24;
                t -= 20;
            }
        }
        float boxWidth = width * this.scale;
        float boxHeight = height * this.scale;

        float halfWidth = Mathf.Round(this.sprite.spriteData.rect.width / 2);
        float halfHeight = Mathf.Round(this.sprite.spriteData.rect.height / 2);


        float leftAdjust = l * this.scale;
        float topAdjust = t * this.scale;

        float left = this.sprite.transform.position.x - halfWidth + leftAdjust;
        float top = this.sprite.transform.position.y - halfHeight + topAdjust;
  
    }

    public void closeMessage()
    {
        this.game.player.SetStatus(PlayerStatus.MOVE);
        this.isMessageShowing = false;
        this.sprite.Visible(false);
        this.characterSprite.Visible(false);
        this.textElement.text =
          "I see you, poking around in the developer console";
        

       
    }
    public void resize()
    {
        
        float width = (Screen.width);
        float height = (Screen.height);
        float x = Mathf.Round(width / 2);
        float y = Mathf.Round(height / 2);
        this.sprite.SetSpritePos(new Vector2(x, y));
        this.characterSprite.SetSpritePos(new Vector2(- 88, -25));

        if (this.engine == CC_Game.EngineType.Scooby)
        {
            
            this.characterSprite.SetSpritePos(new Vector2(-148, -90));
        }

        // In the original game, the message takes up
        // 65% of the screen's height more or less
        float targetHeight = height * CC_Game.UI_HEIGHT_PERCENT;
        float targetWidth = width * CC_Game.UI_WIDTH_PERCENT;
        float scaleY = targetHeight / this.originalHeight;
        float scaleX = targetWidth / this.originalWidth;

        this.scale = 1;
        if (this.adaptiveScale)
        {
            this.scale = width > height ? scaleY : scaleX;
        }

        this.sprite.transform.localScale = new Vector2(this.scale, this.scale);

        this.textElement.fontSize = 90 * this.scale / 100;
        this.setTextDimensions(!this.isCharacterMessage);
    }
}
