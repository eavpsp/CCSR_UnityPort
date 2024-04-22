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
            
        public CC_AudioFile walk = new CC_AudioFile();
        public CC_AudioFile boat = new CC_AudioFile();
        public CC_AudioFile bump = new CC_AudioFile();
        public CC_AudioFile push = new CC_AudioFile();
        public CC_AudioFile chimes = new CC_AudioFile();
        public CC_AudioFile message = new CC_AudioFile();
        public CC_AudioFile secret = new CC_AudioFile();
        public CC_AudioFile correct = new CC_AudioFile();
        public CC_AudioFile incorrect = new CC_AudioFile();
        public CC_AudioFile click = new CC_AudioFile();
        public CC_AudioFile win = new CC_AudioFile();
        public CC_AudioFile lose = new CC_AudioFile();

        // Episode 1:
        public CC_AudioFile pop = new CC_AudioFile();
        public CC_AudioFile water = new CC_AudioFile();
        public CC_AudioFile squeak = new CC_AudioFile();

        // Episode 2:
        public CC_AudioFile robot = new CC_AudioFile();
        public CC_AudioFile headPop = new CC_AudioFile();
        public CC_AudioFile headBounce = new CC_AudioFile();
        public CC_AudioFile alarm = new CC_AudioFile();

        // Episode 3:
        public CC_AudioFile rumble = new CC_AudioFile();
        public CC_AudioFile volcano = new CC_AudioFile();

        // Episode 4:
        public CC_AudioFile crowd = new CC_AudioFile();
        public CC_AudioFile disco = new CC_AudioFile();
        private CC_AudioFile theme = new CC_AudioFile();
        private CC_AudioFile theme1 = new CC_AudioFile();
        private CC_AudioFile theme2 = new CC_AudioFile();
        private int themeSelect = 1;
        private CC_AudioFile currentTheme = new CC_AudioFile();

        private CC_Game.EngineType engine;
        private string root;

        public Dictionary<string, CC_AudioFile> soundBank = new Dictionary<string, CC_AudioFile>();

        public GameSound(CC_Game.EngineType engine, string episode) 
        {
            //load in game audio as audio clips to be stored
            this.engine = engine;
            string root = Application.dataPath+"/game/" + episode +"/sound/";
            this.root = root;
            this.walk.soundClip = CC_Sound.LoadWavFile(root + "walk.wav", 0);
            this.walk.loop = true;
            soundBank.Add("walk", walk);
            this.push.soundClip = CC_Sound.LoadWavFile(root + "push.wav", 0);
            this.push.loop = true;
            soundBank.Add("push", push);
            this.boat.soundClip = CC_Sound.LoadWavFile(root + "boat.wav", 0);
            this.boat.loop = true;
            soundBank.Add("boat", boat);
            this.bump.soundClip = CC_Sound.LoadWavFile(root + "bump.wav", 0);
            soundBank.Add("bump", bump);
            this.chimes.soundClip = CC_Sound.LoadWavFile(root + "chimes.wav", 0);
            soundBank.Add("chimes", chimes);
            this.message.soundClip = CC_Sound.LoadWavFile(root + "message.wav", 0);
            soundBank.Add("message", message);
            this.secret.soundClip = CC_Sound.LoadWavFile(root + "discover.wav", 0);
            soundBank.Add("secret", secret);
            this.correct.soundClip = CC_Sound.LoadWavFile(root + "correct.wav", 0);
            soundBank.Add("correct", correct);
            this.incorrect.soundClip = CC_Sound.LoadWavFile(root + "incorrect.wav", 0);
            soundBank.Add("incorrect", incorrect);
            this.click.soundClip = CC_Sound.LoadWavFile(root + "click.wav", 0);
            soundBank.Add("click", click);
            this.win.soundClip = CC_Sound.LoadWavFile(root + "win.wav", 0);
            soundBank.Add("win", win);
            this.lose.soundClip = CC_Sound.LoadWavFile(root + "lose.wav", 0);
            soundBank.Add("lose", lose);
            this.theme.soundClip = CC_Sound.LoadWavFile(root + "theme.main.wav", 0);
            soundBank.Add("theme", theme);
            this.theme1.soundClip = CC_Sound.LoadWavFile(root + "theme.change.1.wav", 0);
            soundBank.Add("theme1", theme1);
            this.theme2.soundClip = CC_Sound.LoadWavFile(root + "theme.change.2.wav", 0);
            soundBank.Add("theme2", theme2);



            // Episode 1
            if (episode == "1")
            {
                this.pop.soundClip = CC_Sound.LoadWavFile(root + "pop.wav", 0);
                this.water.soundClip = CC_Sound.LoadWavFile(root + "water.wav", 0);
                this.water.loop = true;
                this.squeak.soundClip = CC_Sound.LoadWavFile(root + "squeak.wav", 0);
                this.squeak.loop = true;
                soundBank.Add("pop", pop);
                soundBank.Add("water", water);
                soundBank.Add("squeak", squeak);

            }

            // Episode 2
            if (episode == "2")
            {
                this.robot.soundClip = CC_Sound.LoadWavFile(root + "robot.wav", 0);
                this.robot.loop = true;
                this.headBounce.soundClip = CC_Sound.LoadWavFile(root + "headBounce.wav", 0);
                this.headPop.soundClip = CC_Sound.LoadWavFile(root + "headPop.wav", 0);
                this.alarm.soundClip = CC_Sound.LoadWavFile(root + "alarm.wav", 0);
                this.alarm.loop = true;
                soundBank.Add("robot", robot);
                soundBank.Add("headBounce", headBounce);
                soundBank.Add("headPop", headPop);
                soundBank.Add("alarm", alarm);
            }

            // Episode 3
            if (episode == "3")
            {
                this.rumble.soundClip = CC_Sound.LoadWavFile(root + "rumble.wav", 0);
                this.rumble.loop = true;
                this.volcano.soundClip = CC_Sound.LoadWavFile(root + "volcano.wav", 0);
                soundBank.Add("rumble", rumble);
                soundBank.Add("volcano", volcano);
            }

            // Episode 4
            if (episode == "4")
            {

                this.crowd.soundClip = CC_Sound.LoadWavFile(root + "crowd.wav", 0);
                this.crowd.loop = true;
                this.disco.soundClip = CC_Sound.LoadWavFile(root + "disco.wav", 0);
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
                    audioFile.soundClip = CC_Sound.LoadWavFile(root + sound + ".wav", 0);
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
            playTheme();
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
            EngineManager.instance.StartCoroutine(GetNextTheme());
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
