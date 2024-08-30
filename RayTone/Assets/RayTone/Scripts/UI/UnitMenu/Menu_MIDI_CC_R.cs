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
    public class Menu_MIDI_CC_R : Menu_Unit
    {
        private MIDI_CC_R _MIDI_CC_R;

        // reference to input text
        [SerializeField] private TMP_InputField midiChannelInput;
        [SerializeField] private TMP_InputField ccNumberInput;
        [SerializeField] private UnityEngine.UI.Toggle midiLearnToggle;

        /////
        //START
        void Start()
        {
            _MIDI_CC_R = parentUnit.GetComponent<MIDI_CC_R>();

            // Register GUI
            midiChannelInput.SetTextWithoutNotify(_MIDI_CC_R.GetMIDIChannel().ToString());
            ccNumberInput.SetTextWithoutNotify(_MIDI_CC_R.GetCCNumber().ToString());
            midiChannelInput.onEndEdit.AddListener(delegate { OnMIDIChannelChanged(midiChannelInput.text); });
            ccNumberInput.onEndEdit.AddListener(delegate { OnCCNumberChanged(ccNumberInput.text); });
            midiLearnToggle.onValueChanged.AddListener(delegate { OnMIDILearn(midiLearnToggle.isOn); });
        }

        /////
        //UPDATE
        private void Update()
        {
            if(midiLearnToggle.isOn)
            {
                midiChannelInput.SetTextWithoutNotify(_MIDI_CC_R.GetMIDIChannel().ToString());
                ccNumberInput.SetTextWithoutNotify(_MIDI_CC_R.GetCCNumber().ToString());
            }
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            _MIDI_CC_R.SetMIDILearn(false);
        }

        // Called by MIDI channel text input
        public void OnMIDIChannelChanged(string val)
        {
            int val_int;
            if (int.TryParse(val, out val_int))
            {
                _MIDI_CC_R?.SetMIDIChannel(val_int);
            }
        }

        // Called by CC Number text input
        public void OnCCNumberChanged(string val)
        {
            int val_int;
            if (int.TryParse(val, out val_int))
            {
                _MIDI_CC_R?.SetCCNumber(val_int);
            }
        }

        // Called by MIDI learn button
        public void OnMIDILearn(bool status)
        {
            _MIDI_CC_R?.SetMIDILearn(status);
        }
    }
}
