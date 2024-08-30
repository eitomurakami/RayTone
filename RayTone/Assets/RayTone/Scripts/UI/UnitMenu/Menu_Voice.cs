/*----------------------------------------------------------------------------
*  RayTone: A Node-based Audiovisual Sequencing Environment
*      https://www.raytone.app/
*
*  Copyright 2024 Eito Murakami and John Burnett
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
-----------------------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace RayTone
{
    public class Menu_Voice : Menu_Unit
    {
        // reference to targeted voice
        private VoiceUnit voice;

        // reference to text and sliders
        [SerializeField] private UnityEngine.UI.Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private UnityEngine.UI.Toggle spatializeToggle;

        /////
        //START
        void Start()
        {
            voice = parentUnit.GetComponent<VoiceUnit>();

            if (voice != null)
            {
                volumeSlider.value = voice.GetLocalVolume();
                volumeText.text = "volume: " + Mathf.Floor(voice.GetLocalVolume() * 1000f) / 1000f;
                volumeSlider.onValueChanged.AddListener(delegate { OnVolumeChanged(volumeSlider.value); });

                spatializeToggle.isOn = voice.IsSpatialized();
                spatializeToggle.onValueChanged.AddListener(delegate { OnSpatialize(spatializeToggle.isOn); });
            }
        }

        // Called by slider1
        public void OnVolumeChanged(float val)
        {
            if (voice != null)
            {
                volumeText.text = "volume: " + Mathf.Floor(val * 1000f) / 1000f;
                voice.SetLocalVolume(val);
            }
        }

        // Called by toggle
        public void OnSpatialize(bool arg)
        {
            if (voice != null)
            {
                voice.SetSpatialize(arg);
            }
        }
    }
}
