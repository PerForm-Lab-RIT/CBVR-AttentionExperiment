using System;
using UnityEngine;

namespace Trial_Manager
{
    public class SoundPlayer : MonoBehaviour
    {
        [Serializable]
        private struct SoundEffects
        {
            public AudioClip experimentStart;
            public AudioClip success;
            public AudioClip failure;
        }
        [SerializeField] private AudioSource soundPlayer;
        [SerializeField] private SoundEffects sfx;

        public void PlayStartSound()
        {
            soundPlayer.PlayOneShot(sfx.experimentStart);
        }

        public void PlayWinSound()
        {
            soundPlayer.PlayOneShot(sfx.success);
        }

        public void PlayLoseSound()
        {
            soundPlayer.PlayOneShot(sfx.failure);
        }
    }
}