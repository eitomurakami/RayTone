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

namespace RayTone
{
    public class Menu_Webcam : Menu_Unit
    {
        // reference to UI
        [SerializeField] private TMP_Dropdown devicesDropdown;

        private Webcam webcam;

        /////
        //START
        void Start()
        {
            webcam = parentUnit.GetComponent<Webcam>();

            // Initialize options with a list of available webcam devices
            List<TMP_Dropdown.OptionData> devices = new();
            foreach (WebCamDevice device in WebCamTexture.devices)
            {
                devices.Add(new TMP_Dropdown.OptionData(device.name));
            }
            devicesDropdown.AddOptions(devices);

            // Register GUI
            devicesDropdown.SetValueWithoutNotify(webcam.GetWebcamIndex());
            devicesDropdown.onValueChanged.AddListener(delegate { OnDeviceChanged(devicesDropdown.value); });

        }

        // Called by dropdown
        private void OnDeviceChanged(int val)
        {
            webcam.StartWebcam(val);
        }
    }
}
