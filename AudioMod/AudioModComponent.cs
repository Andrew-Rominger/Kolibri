using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FistVR;
using FMOD.Studio;
using Kolibri;
using UnityEngine;

namespace AudioMod
{
    public class AudioMod
    {
        [InjectMethod(typeof(FistVR.TAH_Manager), "BeginGame")]
        public static void BeginGameInject(TAH_Manager manager)
        {
            try
            {
                Assembly.Load("System.Windows.Forms");
                
                var audioMod = manager.gameObject.AddComponent<AudioModComponent>();
                manager.gameObject.AddComponent<DoubleAudioSource>();
                var holdImporter = manager.gameObject.AddComponent<BassImporter>();
                var takeImporter = manager.gameObject.AddComponent<BassImporter>();

                var configFile = new FileInfo("Mods\\AudioMod\\AudioModConfig.json");
                
                var configFileText = File.ReadAllText(configFile.FullName);
                File.WriteAllText("Out.txt", configFileText);
                var config = JsonUtility.FromJson<ConfigFile>(configFileText);
                File.WriteAllText("Out2.txt", config.ToString());
                audioMod.Config = config;

                audioMod.Manager = manager;
                audioMod.HoldAudioClip = holdImporter.ImportFile(audioMod.HoldSongFile.FullName);
                audioMod.TakeAudioClip = takeImporter.ImportFile(audioMod.TakeSongFile.FullName);
                audioMod.SilenceDefaultMusic();
            }
            catch (Exception e)
            {
                File.WriteAllText("Exception.txt", e.Message + "\n");
                File.AppendAllText("Exception.txt", e.StackTrace);
                Application.Quit();
            }
        }

        [InjectMethod(typeof(FVRFMODController), "SwitchTo")]
        public static void StartCustomMusic(int musicIndex, float timeDelayStart, bool shouldStop, bool shouldDeadStop)
        {
            try
            {
                var audioModComponent = GM.TAHMaster.GetComponent<AudioModComponent>();
                switch (musicIndex)
                {
                    case 1:
                        audioModComponent.PlayHoldMusic();
                        break;
                    case 0:
                        audioModComponent.PlayTakeMusic();
                        break;
                }
                audioModComponent.SilenceDefaultMusic();
            }
            catch (Exception e)
            {
                File.WriteAllText("Exception.txt", e.Message + "\n");
                File.AppendAllText("Exception.txt", e.StackTrace);
                Application.Quit();
            }
        }

        class AudioModComponent : MonoBehaviour
        {
            private ConfigFile _config;

            public ConfigFile Config
            {
                get => _config;
                set
                {
                    _config = value;
                    TakeSongFile = new FileInfo($"Mods\\AudioMod\\{_config.TakeSongFileName}");
                    HoldSongFile = new FileInfo($"Mods\\AudioMod\\{_config.HoldSongFileName}");
                   
                }

            }
            public FileInfo TakeSongFile { get; private set; }
            public FileInfo HoldSongFile { get; private set; }
            public TAH_Manager Manager { get; set; }
            public AudioClip TakeAudioClip { get; set; }
            public AudioClip HoldAudioClip { get; set; }

            public void SilenceDefaultMusic()
            {
                Manager.FMODController.GetField<Bus>("MasterBus").setMute(true);
            }

            public void PlayHoldMusic()
            {
                var musicSource = Manager.gameObject.GetComponent<DoubleAudioSource>();
                musicSource.CrossFade(HoldAudioClip, Config.Volume, Config.TakeMusicFadeTime);
            }
            public void PlayTakeMusic()
            {
                var musicSource = Manager.gameObject.GetComponent<DoubleAudioSource>();
                musicSource.CrossFade(TakeAudioClip, Config.Volume, Config.TakeMusicFadeTime);
            }
        }
    }
}
