using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CC_Scene
{

    public class MoveAnimation
    {
        public CC_SpriteGame sprite;
        public CC_Types.Pos from, to;
        public float startFrame, endFrame;
    }

    public class FrameCallback
    {
        public float frame;
        public UnityEvent callback;
    }

    public class GameScene
    {
        protected CC_Game game;

        protected List<MoveAnimation> moveAnims = new List<MoveAnimation>();
        protected List<FrameCallback> frameCallbacks = new List<FrameCallback>();

        protected float currentFrame = 0;

        public readonly float frameRate = 1000 / 12;
        public float lastUpdate = 0;

        protected bool playing = false;

        public GameObject container = new GameObject("game_scene_container");

        public GameScene(CC_Game game)
        {
            this.game = game;
            this.container.SetActive(false);
            EngineManager.instance.tickEvent.AddListener(delegate {
                this.tick(DateTime.Now.Millisecond);
                });
            this.resize();
        }

        public void resize()
        {
            
            float w = (Screen.width);
            float h = (Screen.height);
            this.container.transform.localScale = new Vector2(this.game.camera.scaleX, this.game.camera.scaleY);
            float x = (w - 416 * this.container.transform.localScale.x) / 2;
            float y = (h - 320 * this.container.transform.localScale.y) / 2;
            this.container.transform.position = new Vector2(x, y);
            //this.container.position.set(0, 0);
        }

        public bool isPlaying()
        {
            return this.playing;
        }

        public void stopPlaying()
        {
            this.playing = false;
        }

        public void tick(float timeNow)
        {
            List<MoveAnimation> anims = this.moveAnims.FindAll(
            (anim) =>
            this.currentFrame >= anim.startFrame &&
            this.currentFrame < anim.endFrame
            );

            anims.ForEach((anim) =>
            {
                float now = timeNow - this.lastUpdate;
                float p = now / this.frameRate;
                float animDeltaX = anim.to.x - anim.from.x;
                float animDeltaY = anim.to.y - anim.from.y;
                float totalFrames = anim.endFrame - anim.startFrame;
                float distX = animDeltaX / totalFrames;
                float distY = animDeltaY / totalFrames;
                float origX = anim.from.x + distX * (this.currentFrame - anim.startFrame);
                float origY = anim.from.y + distY * (this.currentFrame - anim.startFrame);
                float dx = distX * p;
                float dy = distY * p;
                float x = origX + dx;
                float y = origY + dy;
                anim.sprite.SetSpritePos(new Vector2(x, y));
            });
        }

        public void nextFrame(float timeNow)
        {
            this.lastUpdate = timeNow;
            this.currentFrame++;

            // set any animated stuff to their current position
            List<MoveAnimation> anims = this.moveAnims.FindAll(
            (anim) =>
            this.currentFrame >= anim.startFrame &&
            this.currentFrame <= anim.endFrame
            );

            anims.ForEach((anim) => {
                float p =
                    (this.currentFrame - anim.startFrame) /
                    (anim.endFrame - anim.startFrame);
                float dx = (anim.to.x - anim.from.x) * p;
                float dy = (anim.to.y - anim.from.y) * p;
                float x = anim.from.x + dx;
                float y = anim.from.y + dy;
                anim.sprite.SetSpritePos(new Vector2(x, y));
            });

            // Call any callbacks for this specific frame
            this.frameCallbacks
            .FindAll((cb) => cb.frame == this.currentFrame)
            .ForEach((cb) => {
                cb.callback.Invoke();
            });

            this.onFrame();
        }

        public virtual void init(){ }
        public virtual void play(){ }
        public virtual void exit(){ }
        public virtual void onFrame(){ }
    }
}
