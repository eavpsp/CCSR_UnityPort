using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public static class CC_Inventory 
{
    public enum InventoryMode
    {
        NORMAL,
        SELECT,
    }
 

    public interface GameInventoryItemData
    {
        string key { get; set; }
        string name { get; set; } // at first glance, it seems like this isn't used in the game.
        string description { get; set; }
    }
    public class GameInventory
    {
        private CC_Game game;
        private CC_Game.EngineType engine;
        private CC_SpriteUI sprite;
        private CC_SpriteUI spriteInstructions;
        private CC_SpriteUI spriteSelectedItem;

        public CC_SpriteUI girlOrder;

        private TextMeshProUGUI textElement;

        private bool adaptiveScale = false;

        private int originalHeight = 0;
        private int originalWidth = 0;

        private float scale = 1;

        private readonly float FONT_SCALE = 0.72f;

        private InventoryMode mode = InventoryMode.NORMAL;

        public List<string> items= new List<string>();
        public List<string> acts = new List<string>();
        public List<string> names = new List<string>();

        public List<GameInventoryItemData> itemData = new List<GameInventoryItemData>();
        private List<CC_SpriteUI> itemSprites = new List<CC_SpriteUI>();
        public GameObject baseObject;
       

        private bool isInventoryOpen = false;

        private UnityEvent onCloseCallback;

        public List<string> selection = new List<string>();
        public List<string> banned = new List<string>();

        public GameInventory(CC_Game game, CC_Game.EngineType engine)
        {
            this.game = game;
            this.engine = engine;
            this.sprite = new GameObject("inven_Sprite").AddComponent<CC_SpriteUI>();
            this.spriteInstructions = new GameObject("instru").AddComponent<CC_SpriteUI>();
            this.spriteSelectedItem = new GameObject("selec").AddComponent<CC_SpriteUI>();
            this.textElement = new GameObject("invetText").AddComponent<TextMeshProUGUI>();
            this.sprite.spriteButton.interactable = true;
            this.sprite.spriteButton.onClick.AddListener(delegate
            {
                //this.closeInventory();
            });

            this.adaptiveScale = true;

            
        }
        private void selectItem(string key, int index)
        {
            if (index > 16) return;


            List<(Vector2 center, Vector2 topLeft)> points = this.getItemLocationPoints();
            (Vector2 center, Vector2 topLeft) p = points[index];
            this.spriteSelectedItem.SetSpritePos(new Vector2(p.center.x - 1, p.center.y - 1));
            string desc = this.itemData.Find(data => data.key == key).description;
            this.textElement.text = desc;
        }
        private List<(Vector2 center, Vector2 topLeft)> getItemLocationPoints()
        {
            int originX = -Mathf.RoundToInt(originalWidth / 2) + 24;
            int originY = -Mathf.RoundToInt(originalHeight / 2) + 51;
            int squarePadding = 44;
            int squareWidth = 35;

            List<(Vector2 center, Vector2 topLeft)> points = new List<(Vector2 center, Vector2 topLeft)>();

            for (int index = 0; index < 6 * 3; index++)
            {
                int column = index % 6;
                int row = Mathf.FloorToInt(index / 6);
                Vector2 topLeft = new Vector2(originX + column * squarePadding, originY + row * squarePadding);
                Vector2 center = new Vector2(topLeft.x + Mathf.Round(squareWidth / 2f), topLeft.y + Mathf.Round(squareWidth / 2f));
                points.Add((center, topLeft));
            }

            // Remove the top left and top right inventory slots
            // because they are not shown on the graphic
            points.RemoveAt(0);
            points.RemoveAt(4);

            return points;
        }
        public void initItems(List<GameInventoryItemData> itemData)
        {
            this.itemData = itemData;
        }
        public bool isOpen()
        {
            return this.isInventoryOpen;
        }
        public void openInventory(string selectItem = "")
        {

            // Scooby games don't have a UI for the inventory
            if (this.engine == CC_Game.EngineType.Scooby)
            {
                return;
            }

            this.game.sound.once(this.game.sound.message);
            this.game.player.SetStatus(PlayerStatus.STOP);
            this.isInventoryOpen = true;
            this.sprite.Visible(true);

            this.setTextDimensions();

           List<string> items = this.getDisplayItems();

            if (items.Count == 0)
            {
                this.spriteSelectedItem.Visible(false);
                this.textElement.text = "No Items";
                this.clearItemSprites();
            }
            else
            {
                this.renderItems();
                if (selectItem != "")
                {
                    this.selectItem(selectItem, this.getItemIndex(selectItem));
                }
            }
        }
        public void addAct(string actKey)
        {
            this.acts.Add(actKey);
            this.updateVisIfWalking();
        }

        public bool removeAct(string actKey)
        {
            string index = this.acts.Find(i => i == actKey);
            if (index != "")
            {
                this.acts.Remove(index);
                this.updateVisIfWalking();
                return true;
            }
            return false;
        }


        public void addItem(string itemKey)
        {
            this.items.Add(itemKey);
            this.updateVisIfWalking();
        }

        public bool removeItem(string itemKey)
        {
            string index = this.items.Find(i => i == itemKey);
            if (index != "")
            {
                this.items.Remove(index);
                this.updateVisIfWalking();
                return true;
            }
            return false;
        }
        public bool has(string thing)
        {
            return (
              this.names.Contains(thing) || this.hasAct(thing) || this.hasItem(thing)
            );
        }
        private bool hasItem(string itemKey)
        {
            return items.FindAll(i => i == itemKey).Count > 0;
        }
        private bool hasAct(string actKey)
        {
            return this.acts.FindAll((i) => i == actKey).Count > 0;
        }
        private void clearItemSprites()
        {
            foreach (CC_SpriteUI sprite in itemSprites)
            {
                GameObject.Destroy(sprite.gameObject);

            }
            this.itemSprites.Clear();
        }
        private List<string> getDisplayItems()
        {
            return items.Where(i => !banned.Contains(i)).Take(16).ToList();
        }
        private int getItemIndex(string key)
        {
            List<string> items = this.getDisplayItems();
            return items.FindIndex((i) => i == key);
        }
        private void updateVisIfWalking()
        {
            if (this.game.player.status == PlayerStatus.MOVE)
            {
                this.game.updateAllVisibility();
            }
        }
        private void renderItems()
        {
            if (this.mode == InventoryMode.NORMAL)
            {
                this.spriteSelectedItem.spriteRenderer.enabled = true;
            }
            else
            {
                this.spriteSelectedItem.spriteRenderer.enabled = false;
            }
            this.clearItemSprites();

            if (this.items.Count > 16)
            {
                Debug.LogWarning("Warning! Items > 16");
            }
            List<string> items = this.getDisplayItems();
            List<(Vector2 center, Vector2 topLeft)> points = this.getItemLocationPoints();
            var mappedItems = items.FindAll(item => {
                int index = getItemIndex(item);
                Texture2D newTex = CC_Game.getMemberTexture(item + ".png");
                CC_SpriteUI itemSprite = new GameObject(item).AddComponent<CC_SpriteUI>();
                itemSprite.SetSprite(Sprite.Create(newTex, new UnityEngine.Rect(new Vector2(0,0), new Vector2(newTex.width, newTex.height)), Vector2.one * 0.5f));
                itemSprite.SetSpritePos(new Vector2(points[index].center.x, points[index].center.y));
                itemSprite.spriteRenderer.enabled = true;
                itemSprite.spriteButton.interactable = true;
                itemSprite.spriteButton.onClick.AddListener(delegate
                {
                    this.game.sound.SetSFXClip(this.game.sound.click);
                    EngineManager.instance.SFX.Play();
                    if (this.mode == InventoryMode.NORMAL)
                    {
                        this.selectItem(item, index);
                    }
                    else
                    {
                        if (this.selection.Contains(item))
                        {
                            this.selection.Remove(item);
                        }
                        else
                        {
                            if (this.selection.Count < 5)
                            {
                                this.selection.Add(item);
                            }
                        }

                        string description = this.selection.Find(itemDat => {
                            return this.itemData.Find(i => i.key == item).name != null;
                        });

                        this.textElement.text = description;
                    }
                });
                this.itemSprites.Add(itemSprite);
                this.sprite = (itemSprite);
                return this.itemData.Find(i => i.key == item).name != null;

            });
    
                // And release them when the inventory is opened next time.
                
            

            if (this.mode == InventoryMode.NORMAL)
            {
                this.selectItem(items[0], 0);
            }
            else
            {
                this.textElement.text = "";
            }
        }

        public void setMode(InventoryMode mode)
        {
            this.mode = mode;

            if (mode == InventoryMode.SELECT)
            {
                this.textElement.text = "";
                this.selection.Clear();
                this.spriteInstructions.Visible(false);
                this.spriteSelectedItem.Visible(false);
            }
            else
            {
                this.selection.Clear();
                this.spriteInstructions.Visible(true);
                this.spriteSelectedItem.Visible(true);
            }
        }
        public void init()
        {
            this.banned.Add("getanimal");
            Texture2D newTex = CC_Game.getMemberTexture("inventory.png", "inventory.visuals");
            this.sprite = new GameObject("inventory").AddComponent<CC_SpriteUI>();
            this.sprite.SetSprite((Sprite.Create(newTex, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(newTex.width, newTex.height)), Vector2.one * 0.5f)));
            this.sprite.Visible(false);

            this.originalHeight = (int)this.sprite.spriteData.rect.height;
            this.originalWidth = (int)this.sprite.spriteData.rect.width;
            Texture2D newTexSI = CC_Game.getMemberTexture("inventory.instruct.png", "inventory.visuals");
            this.spriteInstructions = new GameObject("instructions").AddComponent<CC_SpriteUI>();
            this.spriteInstructions.SetSprite(Sprite.Create(newTexSI, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(newTexSI.width, newTexSI.height)), Vector2.one * 0.5f));
            this.spriteInstructions.Visible(true);

            // this.spriteInstructions.buttonMode = true;
            // this.spriteInstructions.interactive = true;

            this.spriteInstructions.spriteButton.onClick.AddListener(delegate  {
                this.closeInventory();
            });

            Texture2D newTexSII = CC_Game.getMemberTexture("inventory.square.png", "inventory.visuals");
            this.spriteSelectedItem = new GameObject("selected").AddComponent<CC_SpriteUI>();
            this.spriteSelectedItem.SetSprite(Sprite.Create(newTexSII, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(newTexSII.width, newTexSII.height)), Vector2.one * 0.5f));
            this.spriteSelectedItem.Visible(false);

            if (EngineManager.instance.episode == "4")
            {
                Texture2D girlTex = CC_Game.getMemberTexture("end.girls.order.png", "end.sequence");
                this.girlOrder = new GameObject("girlOrder").AddComponent<CC_SpriteUI>();
                this.girlOrder.SetSprite(Sprite.Create(girlTex, new UnityEngine.Rect(new Vector2(0, 0), new Vector2(girlTex.width, girlTex.height)), Vector2.one * 0.5f));
                this.girlOrder.SetSpritePos(new Vector2(0, -160));
                this.girlOrder.Visible(false);
            }

           

            

            this.resize();
        }
        public void closeInventory()
        {
            if (this.engine == CC_Game.EngineType.Scooby)
            {
                return;
            }

            this.game.player.SetStatus(PlayerStatus.MOVE);
            this.isInventoryOpen = false;
            this.sprite.Visible(false);

            this.textElement.text =
              "I see you, poking around in the developer console";
            

            
                //this.onCloseCallback.Invoke();
                
            
        }
        
        private void setTextDimensions()
        {
            int width = 260;
            int height = 20;

            float boxWidth = width * this.scale;
            float boxHeight = height * this.scale;

            int halfWidth = (int)Mathf.Round(this.sprite.spriteData.rect.width / 2);
            int halfHeight = (int)Mathf.Round(this.sprite.spriteData.rect.height / 2);

            int l = 25;
            int t = 21;

            float leftAdjust = l * this.scale;
            float topAdjust = t * this.scale;

            float left = (int)this.sprite.transform.position.x - halfWidth + leftAdjust;
            float top = (int)this.sprite.transform.position.y - halfHeight + topAdjust;

         
        }

        public void resize()
        {
            Camera mainCam = Camera.main;
            float width = (Screen.width );
            float height = (Screen.height);
            float x = Mathf.Round(width / 2);
            float y = Mathf.Round(height / 2);
            this.sprite.SetSpritePos(new Vector2(x, y));
            this.spriteInstructions.SetSpritePos(new Vector2(0, 90));

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

            this.textElement.fontSize = 100 * this.scale * this.FONT_SCALE;
            this.setTextDimensions();
        }
    }
    
}
