using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_Intro 
{
    public GameObject container;
    private CC_SpriteUI message;
    private string episode;
    public bool inIntro = true;


    public CC_Intro(string episode)
    {
        this.container = new GameObject("intro_container");
        this.episode = episode;
        this.message = new GameObject("messages").AddComponent<CC_SpriteUI>();

    }
    public void close(CC_Game game)
    {
        this.message.Visible(false);
        game.sound.playTheme();
        this.inIntro = false;
    }
    public void init(CC_Game game)
    {
        string tex = "summer.instructs." + this.episode.PadLeft(2, '0') + ".png";
        if (this.episode == "scooby-1") tex = "scooby instructions 01.png";
        if (this.episode == "scooby-2") tex = "scooby instructions 02.png";

        Texture2D texture = CC_Game.getMemberTexture(tex, "Internal");
        this.message.SetSprite(Sprite.Create(texture, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(texture.width, texture.height)), Vector2.one * 0.5f));
        this.resize(game);
        game.sound.SetSFXClip(game.sound.message);
        EngineManager.instance.SFX.Play();

        this.message.spriteButton.interactable = true;
  

        this.message.spriteButton.onClick.AddListener(delegate{
            this.close(game);
        });
    }
    public void resize(CC_Game game)
    {
        Camera mainCam = Camera.main;
        float w = (Screen.width );
        float h = (Screen.height );
        if (this.message.spriteData != null)
        {
            if (w > h)
            {
                float ratio = this.message.spriteData.rect.width / this.message.spriteData.rect.height;
                float height = h * 0.75f;
                this.message.spriteRenderer.rectTransform.sizeDelta = new Vector2(height, height * ratio);
            }
            else
            {
                float ratio = this.message.spriteData.rect.height / this.message.spriteData.rect.width;
                float width = w * 0.75f;
                this.message.spriteRenderer.rectTransform.sizeDelta = new Vector2(width, width * ratio);
            };
            this.message.SetSpritePos(new Vector2(Mathf.Round(w / 2), Mathf.Round(h / 2)));

        }
        
            

    }
    
}
