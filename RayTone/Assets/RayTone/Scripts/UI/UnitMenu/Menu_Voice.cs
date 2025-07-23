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
        [SerializeField] private UnityEngine.UI.Slider panningSlider;
        [SerializeField] private TextMeshProUGUI panningText;
        [SerializeField] private UnityEngine.UI.Toggle spatializeToggle;

        /////
        //START
        void Start()
        {
            voice = parentUnit.GetComponent<VoiceUnit>();

            if (voice != null)
            {
                // Register UI
                volumeSlider.SetValueWithoutNotify(voice.GetLocalVolume());
                volumeText.text = "volume: " + Mathf.Floor(voice.GetLocalVolume() * 1000f) / 1000f;
                volumeSlider.onValueChanged.AddListener(delegate { OnVolumeChanged(volumeSlider.value); });

                panningSlider.SetValueWithoutNotify(voice.GetPanningValue());
                panningText.text = "panning: " + Mathf.Floor(panningSlider.value * 100f) / 100f;
                panningSlider.onValueChanged.AddListener(delegate { OnPanningChanged(panningSlider.value); });

                spatializeToggle.isOn = voice.IsSpatialized();
                panningSlider.interactable = !voice.IsSpatialized();  // panning disabled when spatialization is on
                panningText.color = voice.IsSpatialized() ? new Color(0.3f, 0.3f, 0.3f, 1.0f) : Color.white;
                spatializeToggle.onValueChanged.AddListener(delegate { OnSpatialize(spatializeToggle.isOn); });
            }
        }

        // Called by slider1
        private void OnVolumeChanged(float val)
        {
            volumeText.text = "volume: " + Mathf.Floor(val * 1000f) / 1000f;
            voice.SetLocalVolume(val);
        }

        // Called by panning slider
        private void OnPanningChanged(float val)
        {
            voice.SetPanningValue(val);
            panningText.text = "panning: " + Mathf.Floor(val * 100f) / 100f;
        }

        // Called by toggle
        private void OnSpatialize(bool arg)
        {
            voice.SetSpatialize(arg);
            panningSlider.interactable = !arg;  // panning disabled when spatialization is on
            panningText.color = arg ? new Color(0.3f, 0.3f, 0.3f, 1.0f) : Color.white;
        }
    }
}
