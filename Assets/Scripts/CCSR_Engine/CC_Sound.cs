using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class CC_AudioFile
{
    public bool loop = false;
    public AudioClip soundClip;
}
public class CC_Sound 
{
    
    
    private static AudioClip LoadWavFile(string filePath, int offsetSamples)
    {
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist: " + filePath);
            return null;
        }

        // Read the bytes from the WAV file
        byte[] wavData = File.ReadAllBytes(filePath);

      

        // Check if AudioClip creation was successful
        
            // Use the AudioClip as needed (e.g., play it)
            //AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            //audioSource.clip = audioClip;
            //audioSource.Play();
            int headerSize = 44;
            int sampleCount = wavData.Length - headerSize;
            float[] floatData = new float[sampleCount / 2];
            for (int i = 0; i < sampleCount / 2; i++)
            {
                floatData[i] = (float)System.BitConverter.ToInt16(wavData, i * 2 + headerSize + offsetSamples * 2) / 32768.0f;
            }
            AudioClip audioClip = AudioClip.Create(filePath, sampleCount / 2, 1, 44100, false);
            audioClip.SetData(floatData, 0);
            return audioClip;
        
       
    }
    public AudioClip ToAudioClip(byte[] wavData, int offsetSamples, string name = "wav")
    {
        // Convert WAV data to Unity audio clip
        int headerSize = 44;
        int sampleCount = wavData.Length - headerSize;
        float[] floatData = new float[sampleCount / 2];
        for (int i = 0; i < sampleCount / 2; i++)
        {
            floatData[i] = (float)System.BitConverter.ToInt16(wavData, i * 2 + headerSize + offsetSamples * 2) / 32768.0f;
        }
        AudioClip audioClip = AudioClip.Create(name, sampleCount / 2, 1, 44100, false);
        audioClip.SetData(floatData, 0);
        return audioClip;
    }
    public class GameSound
    {
            
        public CC_AudioFile walk;
        public CC_AudioFile boat;
        public CC_AudioFile bump;
        public CC_AudioFile push;
        public CC_AudioFile chimes;
        public CC_AudioFile message;
        public CC_AudioFile secret;
        public CC_AudioFile correct;
        public CC_AudioFile incorrect;
        public CC_AudioFile click;
        public CC_AudioFile win;
        public CC_AudioFile lose;

        // Episode 1:
        public CC_AudioFile pop;
        public CC_AudioFile water;
        public CC_AudioFile squeak;

        // Episode 2:
        public CC_AudioFile robot;
        public CC_AudioFile headPop;
        public CC_AudioFile headBounce;
        public CC_AudioFile alarm;

        // Episode 3:
        public CC_AudioFile rumble;
        public CC_AudioFile volcano;

        // Episode 4:
        public CC_AudioFile crowd;
        public CC_AudioFile disco;

        private CC_AudioFile theme;
        private CC_AudioFile theme1;
        private CC_AudioFile theme2;
        private int themeSelect = 1;
        private CC_AudioFile currentTheme;

        private CC_Game.EngineType engine;
        private string root;

        public Dictionary<string, CC_AudioFile> soundBank = new Dictionary<string, CC_AudioFile>();

        public GameSound(CC_Game.EngineType engine, string episode) 
        {
            //load in game audio as audio clips to be stored
            this.engine = engine;
            string root = Application.dataPath+"/sounds/";
            this.root = root;
            this.walk.soundClip = CC_Sound.LoadWavFile("walk.wav", 0);
            this.walk.loop = true;
            soundBank.Add("walk", walk);
            this.push.soundClip = CC_Sound.LoadWavFile("push.wav", 0);
            this.push.loop = true;
            soundBank.Add("push", push);
            this.boat.soundClip = CC_Sound.LoadWavFile("boat.wav", 0);
            this.boat.loop = true;
            soundBank.Add("boat", boat);
            this.bump.soundClip = CC_Sound.LoadWavFile("bump.wav", 0);
            soundBank.Add("bump", bump);
            this.chimes.soundClip = CC_Sound.LoadWavFile("chimes.wav", 0);
            soundBank.Add("chimes", chimes);
            this.message.soundClip = CC_Sound.LoadWavFile("message.wav", 0);
            soundBank.Add("message", message);
            this.secret.soundClip = CC_Sound.LoadWavFile("discover.wav", 0);
            soundBank.Add("secret", secret);
            this.correct.soundClip = CC_Sound.LoadWavFile("correct.wav", 0);
            soundBank.Add("correct", correct);
            this.incorrect.soundClip = CC_Sound.LoadWavFile("incorrect.wav", 0);
            soundBank.Add("incorrect", incorrect);
            this.click.soundClip = CC_Sound.LoadWavFile("click.wav", 0);
            soundBank.Add("click", click);
            this.win.soundClip = CC_Sound.LoadWavFile("win.wav", 0);
            soundBank.Add("win", win);
            this.lose.soundClip = CC_Sound.LoadWavFile("lose.wav", 0);
            soundBank.Add("lose", lose);
            this.theme.soundClip = CC_Sound.LoadWavFile("theme.main.wav", 0);
            soundBank.Add("theme", theme);
            this.theme1.soundClip = CC_Sound.LoadWavFile("theme.change.1.wav", 0);
            soundBank.Add("theme1", theme1);
            this.theme2.soundClip = CC_Sound.LoadWavFile("theme.change.2.wav", 0);
            soundBank.Add("theme2", theme2);



            // Episode 1
            if (episode == "1")
            {
                this.pop.soundClip = CC_Sound.LoadWavFile("pop.wav", 0);
                this.water.soundClip = CC_Sound.LoadWavFile("water.wav", 0);
                this.water.loop = true;
                this.squeak.soundClip = CC_Sound.LoadWavFile("squeak.wav", 0);
                this.squeak.loop = true;
                soundBank.Add("pop", pop);
                soundBank.Add("water", water);
                soundBank.Add("squeak", squeak);

            }

            // Episode 2
            if (episode == "2")
            {
                this.robot.soundClip = CC_Sound.LoadWavFile("robot.wav", 0);
                this.robot.loop = true;
                this.headBounce.soundClip = CC_Sound.LoadWavFile("headBounce.wav", 0);
                this.headPop.soundClip = CC_Sound.LoadWavFile("headPop.wav", 0);
                this.alarm.soundClip = CC_Sound.LoadWavFile("alarm.wav", 0);
                this.alarm.loop = true;
                soundBank.Add("robot", robot);
                soundBank.Add("headBounce", headBounce);
                soundBank.Add("headPop", headPop);
                soundBank.Add("alarm", alarm);
            }

            // Episode 3
            if (episode == "3")
            {
                this.rumble.soundClip = CC_Sound.LoadWavFile("rumble.wav", 0);
                this.rumble.loop = true;
                this.volcano.soundClip = CC_Sound.LoadWavFile("volcano.wav", 0);
                soundBank.Add("rumble", rumble);
                soundBank.Add("volcano", volcano);
            }

            // Episode 4
            if (episode == "4")
            {

                this.crowd.soundClip = CC_Sound.LoadWavFile("crowd.wav", 0);
                this.crowd.loop = true;
                this.disco.soundClip = CC_Sound.LoadWavFile("disco.wav", 0);
                this.disco.loop = true;
                soundBank.Add("crowd", crowd);
                soundBank.Add("disco", disco);
            }

            // ghetto as hell but who cares
            if (episode.ToLower().Contains("scooby"))
            {
                string[] sounds = new string[] {
              "bloop", "bump", "bunch_o_bats",
              "chimes", "discover", "door_creak",
              "ghost_02", "howl", "message",
              "music_haunted_loop_01",
              "music_haunted_loop_02",
              "scooby dooby doo",
              "pipe.organ",
              "push", "ruh_oh"
            };

                string[] loops = new string[] { "bunch_o_bats"};

          foreach(string sound in sounds) 
             {
               
                    CC_AudioFile audioFile = new CC_AudioFile();
                    audioFile.soundClip = CC_Sound.LoadWavFile(sound + ".wav", 0);
                    audioFile.loop = loops.Contains(sound);
                    soundBank.Add(sound, audioFile);
            }

            // hack in the theme music
            this.theme = this.soundBank["music_haunted_loop_01"];
            this.theme1 = this.soundBank["music_haunted_loop_02"];
        }

        this.currentTheme = this.theme;

        this.initTheme();
    }

        public void dynamicSoundOnce(string sound) 
        {
            CC_AudioFile howl = this.soundBank[sound];
            if (howl != null)
            {
                if (!EngineManager.instance.SFX.isPlaying)
                {
                    SetSFXClip(howl);
                    EngineManager.instance.SFX.Play();
                }
            }
        }

        public void once(CC_AudioFile sound) 
        {
            if (!EngineManager.instance.SFX.isPlaying)
            {
                SetSFXClip(sound);
                EngineManager.instance.SFX.Play();
            }
        }

        public void setVolumeTheme(float level) {
        
        }

        public void setVolumeMaster(float level) {
    
       
        }
        public IEnumerator GetNextTheme() 
        {
            while (EngineManager.instance.BGM.isPlaying)
            {
                yield return null;

            }
            themeSelect++;
            if (themeSelect > 3)
            {
                currentTheme = theme;
                themeSelect = 0;
            }
            else
            {
                if (themeSelect == 1)
                {
                    currentTheme = theme1;
                }
                else if(themeSelect == 2)
                {
                    currentTheme = theme2;

                }
            }
            initTheme();
        }

        private void initTheme()
        {
            this.currentTheme = this.theme;
            SetBGMClip(currentTheme);
        
            if (this.engine == CC_Game.EngineType.CCSR)
            {
                EngineManager.instance.StartCoroutine(GetNextTheme());
            }
            else if (this.engine == CC_Game.EngineType.Scooby)
            {
                EngineManager.instance.StartCoroutine(GetNextTheme());

            }
        }

        public void SetBGMClip(CC_AudioFile file)
        {
            EngineManager.instance.BGM.clip = file.soundClip;
            EngineManager.instance.BGM.loop = file.loop;
        }
        public void SetSFXClip(CC_AudioFile file)
        {
            EngineManager.instance.SFX.clip = file.soundClip;
            EngineManager.instance.SFX.loop = file.loop;
        }
        public void playTheme()
        {
                SetBGMClip(this.currentTheme);
                EngineManager.instance.BGM.Play();
        }

        public bool isThemePlaying()
        {
            return (
               EngineManager.instance.BGM.clip == this.theme.soundClip || EngineManager.instance.BGM.clip == this.theme1.soundClip || EngineManager.instance.BGM.clip == this.theme2.soundClip
            );
        }

        public void pauseTheme()
        {
            EngineManager.instance.BGM.Pause();
        }
    }

}
