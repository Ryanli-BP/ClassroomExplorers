// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using UnityEngine.UI;

namespace UltimateClean
{
    /// <summary>
    /// This class handles updating the music UI widgets depending on the player's selection.
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        private Slider m_musicSlider;
        private GameObject m_musicButton;

        private void Start()
        {
            m_musicSlider = GetComponent<Slider>();
            m_musicSlider.value = PlayerPrefs.GetInt("music_on");
            m_musicButton = GameObject.Find("MusicButton/Button");
        }

        public void SwitchMusic()
        {
            var backgroundAudioSource = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
            backgroundAudioSource.volume = m_musicSlider.value;
            PlayerPrefs.SetInt("music_on", (int)m_musicSlider.value);
            if (m_musicButton != null)
                m_musicButton.GetComponent<MusicButton>().ToggleSprite();
        }
    }
}
