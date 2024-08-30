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
    public class Menu_OSC_R : Menu_Unit
    {
        private OSC_R osc_r;

        // reference to input text
        [SerializeField] private TMP_InputField oscAddressInput;
        [SerializeField] private TMP_InputField portInput;

        /////
        //START
        void Start()
        {
            osc_r = parentUnit.GetComponent<OSC_R>();

            // Register GUI
            oscAddressInput.SetTextWithoutNotify(osc_r.GetOSCAddress());
            oscAddressInput.onEndEdit.AddListener(delegate { OnOSCAddressChanged(oscAddressInput.text); });

            portInput.SetTextWithoutNotify(osc_r.GetPort().ToString());
            portInput.onEndEdit.AddListener(delegate { OnPortChanged(portInput.text); });
        }

        // Called by OSCAddress text input
        public void OnOSCAddressChanged(string arg)
        {
            osc_r?.SetOSCAddress(arg);
        }

        // Called by Port text input
        public void OnPortChanged(string val)
        {
            float val_float;
            if (float.TryParse(val, out val_float))
            {
                osc_r?.SetPort((int)val_float);
            }
        }
    }
}
