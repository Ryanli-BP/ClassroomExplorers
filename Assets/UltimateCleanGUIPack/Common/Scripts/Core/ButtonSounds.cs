// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// This component goes together with a button object and contains
    /// the audio clips to play when the player rolls over and presses it.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ButtonSounds : MonoBehaviour
    {
        public AudioClip pressedSound;
        public AudioClip rolloverSound;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayPressedSound()
        {
            audioSource.clip = pressedSound;
            audioSource.Play();
        }

        public void PlayRolloverSound()
        {
            audioSource.clip = rolloverSound;
            audioSource.Play();
        }
    }
}