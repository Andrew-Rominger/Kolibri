using System;
using System.Collections.Generic;

namespace AudioMod
{
    [Serializable]
    public class ConfigFile
    {
        public float Volume;
        public float HoldMusicFadeTime;
        public float TakeMusicFadeTime;
        public string TakeSongFileName;
        public string HoldSongFileName;
        public override string ToString()
        {
            return
                $"Volume: {Volume}\nHoldMusicFadeTime: {HoldMusicFadeTime}\nTakeMusicFadeTime: {TakeMusicFadeTime}\nTakeSongFileName: {TakeSongFileName}\nHoldSongFileName: {HoldSongFileName}";
        }
    }
}
