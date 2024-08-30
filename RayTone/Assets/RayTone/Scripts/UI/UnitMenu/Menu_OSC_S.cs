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
    public class Menu_OSC_S : Menu_Unit
    {
        private OSC_S osc_s;

        // reference to input text
        [SerializeField] private TMP_InputField ipAddressInput;
        [SerializeField] private TMP_InputField oscAddressInput;
        [SerializeField] private TMP_InputField portInput;

        /////
        //START
        void Start()
        {
            osc_s = parentUnit.GetComponent<OSC_S>();

            //Register GUI
            ipAddressInput.SetTextWithoutNotify(osc_s.GetIPAddress());
            ipAddressInput.onEndEdit.AddListener(delegate { OnIPAddressChanged(ipAddressInput.text); });

            oscAddressInput.SetTextWithoutNotify(osc_s.GetOSCAddress());
            oscAddressInput.onEndEdit.AddListener(delegate { OnOSCAddressChanged(oscAddressInput.text); });

            portInput.SetTextWithoutNotify(osc_s.GetPort().ToString());
            portInput.onEndEdit.AddListener(delegate { OnPortChanged(portInput.text); });
        }

        // Called by IPAddress text input
        public void OnIPAddressChanged(string arg)
        {
            osc_s?.SetIPAddress(arg);
        }

        // Called by OSCAddress text input
        public void OnOSCAddressChanged(string arg)
        {
            osc_s?.SetOSCAddress(arg);
        }

        // Called by Port text input
        public void OnPortChanged(string val)
        {
            float val_float;
            if (float.TryParse(val, out val_float))
            {
                osc_s?.SetPort((int)val_float);
            }
        }
    }
}
