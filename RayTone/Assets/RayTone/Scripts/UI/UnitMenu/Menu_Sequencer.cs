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
    public class Menu_Sequencer : Menu_Unit
    {
        private Sequencer sequencer;

        // reference to text and sliders
        [SerializeField] private UnityEngine.UI.Slider stepSlider;
        [SerializeField] private TextMeshProUGUI stepText;
        [SerializeField] private UnityEngine.UI.Slider clockSlider;
        [SerializeField] private TMP_InputField clockInput;
        [SerializeField] private TextMeshProUGUI clockText;
        [SerializeField] private TMP_InputField minInput;
        [SerializeField] private TMP_InputField maxInput;
        [SerializeField] private TMP_InputField deltaInput;

        /////
        //START
        void Start()
        {
            sequencer = parentUnit.GetComponent<Sequencer>();

            // Register GUI
            stepSlider.value = sequencer.stepMax;
            stepText.text = "step: " + sequencer.stepMax;
            stepSlider.onValueChanged.AddListener(delegate { OnStepNumChanged(stepSlider.value); });

            clockSlider.SetValueWithoutNotify(Mathf.Log(sequencer.clockDiv, 2));
            //clockText.text = "clock: " + sequencer.clock_div;
            clockSlider.onValueChanged.AddListener(delegate { OnClockDivSliderChanged(clockSlider.value); });

            clockInput.SetTextWithoutNotify(sequencer.clockDiv.ToString());
            clockInput.onEndEdit.AddListener(delegate { OnClockDivTextChanged(clockInput.text); });
            clockInput.caretWidth = 0;

            minInput.text = sequencer.valMin.ToString();
            minInput.onEndEdit.AddListener(delegate { OnValMinChanged(minInput.text); });

            maxInput.text = sequencer.valMax.ToString();
            maxInput.onEndEdit.AddListener(delegate { OnValMaxChanged(maxInput.text); });

            deltaInput.text = sequencer.valDelta.ToString();
            deltaInput.onEndEdit.AddListener(delegate { OnValDeltaChanged(deltaInput.text); });
        }

        // Called by StepNum slider
        public void OnStepNumChanged(float val)
        {
            if (sequencer != null)
            {
                stepText.text = "step: " + val;
                sequencer.SpawnSteps((int)val);
                sequencer.UpdatePosition();
            }
        }

        // Called by ClockDiv slider
        public void OnClockDivSliderChanged(float val)
        {
            if (sequencer != null)
            {
                val = Mathf.Pow(2, val);
                clockInput.SetTextWithoutNotify(val.ToString());
                //clockText.text = "clock: " + val;
                sequencer.clockDiv = (int)val;
            }
        }

        // Called by ClockDiv text input
        public void OnClockDivTextChanged(string val)
        {
            if (sequencer != null && float.TryParse(val, out float val_float))
            {
                val_float = Mathf.Max(val_float, 1);
                sequencer.clockDiv = (int)val_float;
                clockSlider.SetValueWithoutNotify(Mathf.Log(sequencer.clockDiv, 2));
            }
        }

        // Called by ValMin text input
        public void OnValMinChanged(string val)
        {
            if (sequencer != null && float.TryParse(val, out float val_float))
            {
                //Update val_max if neccessary
                if (sequencer.valMax < val_float)
                {
                    maxInput.text = val_float.ToString();
                    sequencer.SetValMax(val_float);
                }

                sequencer.SetValMin(val_float);
            }
        }

        // Called by ValMax text input
        public void OnValMaxChanged(string val)
        {
            if (sequencer != null && float.TryParse(val, out float val_float))
            {
                //Update val_min if neccessary
                if (sequencer.valMin > val_float)
                {
                    minInput.text = val_float.ToString();
                    sequencer.SetValMin(val_float);
                }

                sequencer.SetValMax(val_float);
            }
        }

        // Called by Delta text input
        public void OnValDeltaChanged(string val)
        {
            if (sequencer != null && float.TryParse(val, out float val_float))
            {
                //Reset text to 1 if input is below 1
                if (val_float < 0.001)
                {
                    val_float = 0.001f;
                    deltaInput.text = "0.001";
                }

                sequencer.SetValDelta(val_float);
            }
        }

        // Close panel
        public void ClosePanel()
        {
            menuController.ExitUnitMenu(sequencer);
        } 
    }
}
