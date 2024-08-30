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
    public class Menu_KeyInput : Menu_Unit
    {
        private KeyInput _KeyInput;

        // reference to input text
        [SerializeField] private TMP_InputField keyInput;
        [SerializeField] private UnityEngine.UI.Toggle continuousToggle;

        /////
        //START
        void Start()
        {
            _KeyInput = parentUnit.GetComponent<KeyInput>();

            // Register GUI
            keyInput.SetTextWithoutNotify(_KeyInput.GetKey());
            keyInput.onEndEdit.AddListener(delegate { OnKeyChanged(keyInput.text); });
            continuousToggle.SetIsOnWithoutNotify(_KeyInput.GetContinuous());
            continuousToggle.onValueChanged.AddListener(delegate { OnContinuousChanged(continuousToggle.isOn); });
        }

        // Called by key text input
        public void OnKeyChanged(string val)
        {
            _KeyInput.SetKey(val);
        }

        // Called by continuous toggle
        public void OnContinuousChanged(bool status)
        {
            _KeyInput.SetContinuous(status);
        }
    }
}
