using System.Collections.Generic;
using UnityEngine;

namespace Sound
{
    public class SoundBox : MonoBehaviour
    {
        [SerializeField] private List<AudioSource> sounds = new List<AudioSource>();

        public void SoundPlay(int index)
        {
            if(sounds.Count > index)
                sounds[index].Play();
        }

        public void SoundStop(int index)
        {
            if(sounds.Count > index)
                sounds[index].Stop();
        }
    }
}