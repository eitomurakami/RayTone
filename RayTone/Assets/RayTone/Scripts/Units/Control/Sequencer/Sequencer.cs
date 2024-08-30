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
using System.Linq;
using UnityEngine;

namespace RayTone
{
    public class Sequencer : ControlUnit
    {
        // Prefab reference
        [SerializeField] private SequencerStep SequencerStep_PF;
        [SerializeField] private SequencerText SequencerText_PF;
        [SerializeField] private Renderer ring;

        // public variables
        [System.NonSerialized] public float valMin = 1f;
        [System.NonSerialized] public float valMax = 10f;
        [System.NonSerialized] public float valDelta = 1f;
        [System.NonSerialized] public int stepMax = 16;
        [System.NonSerialized] public int clockDiv = 1;
        [System.NonSerialized] public int valRange = 10;  // no input - derived parameter
        [System.NonSerialized] public int stepCurrent = 0;  // no input
        [System.NonSerialized] public int clockIndex = 0;  // no input
        [System.NonSerialized] public float[] vals = new float[16];

        // private variables
        private SequencerStep[] steps = new SequencerStep[16];
        private int stepSelected = 0;
        private SequencerText textStep = null;
        private SequencerText text = null;
        private bool stepUpdate = false;
        private float outVal = 0f;
        private int trigger = 0;
        private Material ringMaterial;

        ///// 
        //AWAKE
        void Awake()
        {
            outlet = SpawnOutlet(new Vector3(0f, 0f, 0f));
        }

        ///// 
        //START
        protected override void Start()
        {
            base.Start();
            ringMaterial = ring.material;
            SpawnSteps(stepMax);
            StartStepListener();
        }

        ///// 
        //UPDATE
        private void Update()
        {
            if (stepUpdate)
            {
                // Highlight current step
                for (int i = 0; i < stepMax; i++)
                {
                    if (steps[i] != null)
                    {
                        steps[i].GetMaterial().SetColor("_Color", new Color(93f / 255f, 88f / 255f, 191f / 255f) * 2.5f);
                    }
                }
                // ver.0.40 - because StepUpdate advances step_current in audio thread, the step to highlight is always step_current - 1
                int stepToHighlight = (stepCurrent - 1 + stepMax) % stepMax;
                if (steps[stepToHighlight] != null)
                {
                    steps[stepToHighlight].GetMaterial().SetColor("_Color", new Color(93f / 255f, 88f / 255f, 191f / 255f) * 4.5f);
                }

                stepUpdate = false;
            }
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            StopStepListener();
            Destroy(ringMaterial);
            ringMaterial = null;
        }

        /// <summary>
        /// Apply Unit Properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            SetValMin(up.metaFloat["val_min"]);
            SetValMax(up.metaFloat["val_max"]);
            SetValDelta(up.metaFloat["val_delta"]);
            stepMax = up.metaInt["step_max"];
            clockDiv = up.metaInt["clock_div"];

            for(int i = 0; i < vals.Length; i++)
            {
                vals[i] = up.metaFloat[i.ToString()];
            }
        }
        /// <summary>
        /// Get Unit Properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();

            up.metaFloat = new();
            up.metaFloat.Add("val_min", valMin);
            up.metaFloat.Add("val_max", valMax);
            up.metaFloat.Add("val_delta", valDelta);
            for (int i = 0; i < vals.Length; i++)
            {
                up.metaFloat.Add(i.ToString(), vals[i]);
            }

            up.metaInt = new();
            up.metaInt.Add("step_max", stepMax);
            up.metaInt.Add("clock_div", clockDiv);

            return up;
        }

        /// <summary>
        /// Spawn steps
        /// </summary>
        /// <param name="stepNum"></param>
        public void SpawnSteps(int stepNum)
        {
            // Activate ring if step_num <= 2
            if (stepNum <= 2)
            {
                ring.enabled = true;
            }
            else
            {
                ring.enabled = false;
            }

            if (stepNum < 1)
            {
                stepNum = 1;
            }
            else if (stepNum > 16)
            {
                stepNum = 16;
            }
            stepMax = stepNum;

            for (int i = 0; i < 16; i++)
            {
                if (steps[i] != null)
                {
                    Destroy(steps[i].gameObject);
                }

                if (i < stepMax)
                {
                    SequencerStep go = Instantiate(SequencerStep_PF);
                    go.transform.parent = this.transform;
                    go.transform.localPosition = new(4 * Mathf.Sin(2 * Mathf.PI / stepMax * i), 0f, 4 * Mathf.Cos(2 * Mathf.PI / stepMax * i));
                    steps[i] = go;
                }
            }

            stepSelected = 0;
        }

        /// <summary>
        /// Destroy text
        /// </summary>
        private void DestroyText()
        {
            if (text != null)
            {
                Destroy(text.gameObject);
                text = null;
            }
        }

        /// <summary>
        /// Display text
        /// </summary>
        /// <param name="text_arg"></param>
        private void DisplayText(string text_arg)
        {
            DestroyStepText();
            text = Instantiate(SequencerText_PF);
            text.transform.parent = transform;
            text.transform.localPosition = new(0f, 1.25f, 0f);

            text.SetText(text_arg);
        }

        /// <summary>
        /// Destroy step text
        /// </summary>
        private void DestroyStepText()
        {
            if (textStep != null)
            {
                Destroy(textStep.gameObject);
                textStep = null;
            }
        }

        /// <summary>
        /// Display step text
        /// </summary>
        private void DisplayStepText()
        {
            DestroyStepText();
            textStep = Instantiate(SequencerText_PF);
            textStep.transform.parent = steps[stepSelected].transform;
            textStep.transform.localPosition = new(0f, 1.25f, 0f);

            // Compute value and set text
            float computedvalue;
            if (vals[stepSelected] == 0)
            {
                computedvalue = 0;
            }
            else
            {
                computedvalue = (vals[stepSelected] - 1) * valDelta + valMin;
            }
            textStep.SetText(computedvalue.ToString());
        }

        /// <summary>
        /// Highlight steps
        /// </summary>
        public void HighlightSteps()
        {
            for (int i = 0; i < stepMax; i++)
            {
                if (steps[i] != null)
                {
                    steps[i].Highlight();
                }
            }

            // Highlight ring
            if (ring.enabled == true)
            {
                ringMaterial.SetColor("_Color", new Color(93f / 255f, 88f / 255f, 191f / 255f) * 3.5f);
            }
        }

        /// <summary>
        /// Dehighlight steps
        /// </summary>
        public void DehighlightSteps()
        {
            for (int i = 0; i < stepMax; i++)
            {
                if (steps[i] != null)
                {
                    steps[i].Dehighlight();
                }
            }

            // De-highlight ring
            if (ring.enabled == true)
            {
                ringMaterial.SetColor("_Color", new Color(93f / 255f, 88f / 255f, 191f / 255f) * 2f);
            }

            // Destroy any Sequencer text
            DestroyStepText();
        }

        /// <summary>
        /// Select step
        /// </summary>
        /// <param name="index"></param>
        public void SelectStep(int index)
        {
            if (index >= 0 && index < stepMax && steps[index] != null)
            {
                DehighlightSteps();
                steps[index].Highlight();
                stepSelected = index;
            }

            DisplayStepText();
        }

        /// <summary>
        /// Select next step
        /// </summary>
        public void SelectNextStep()
        {
            stepSelected = (stepSelected + 1) % stepMax;
            SelectStep(stepSelected);
        }

        /// <summary>
        /// Select previous step
        /// </summary>
        public void SelectPrevStep()
        {
            stepSelected = (stepSelected - 1 + stepMax) % stepMax;
            SelectStep(stepSelected);
        }

        /// <summary>
        /// Set step value
        /// </summary>
        /// <param name="val"></param>
        public void SetStepVal(float val)
        {
            if (steps[stepSelected] != null)
            {
                SequencerStep stepRef = steps[stepSelected];
                vals[stepSelected] = Mathf.Min(Mathf.Max(val, 0f), valRange);
                stepRef.transform.localPosition = new(stepRef.transform.localPosition.x, (vals[stepSelected] / valRange) * 3f, stepRef.transform.localPosition.z);

                DisplayStepText();
            }
        }

        /// <summary>
        /// Increment / Decrement step value
        /// </summary>
        /// <param name="delta"></param>
        public void IncDecStepVal(int delta)
        {
            if (steps[stepSelected] != null)
            {
                //SequencerStep step_ref = steps[step_selected];
                SetStepVal(Mathf.Min(Mathf.Max((vals[stepSelected] + delta), 0f), valRange));
            }
        }

        /// <summary>
        /// Increment / Decrement all steps values
        /// </summary>
        /// <param name="delta"></param>
        public void IncDecStepsVal(int delta)
        {
            for (int i = 0; i < stepMax; i++)
            {
                stepSelected = i;
                IncDecStepVal(delta);
            }

            SelectStep(0);
        }

        /// <summary>
        /// Randomize all steps values
        /// </summary>
        public void RandomizeStepsVal()
        {
            for (int i = 0; i < stepMax; i++)
            {
                stepSelected = i;
                SetStepVal(Random.Range(0, valRange + 1));
            }

            DisplayText("Randomized All Steps!");

            SelectStep(0);
        }

        /// <summary>
        /// Update all steps position
        /// </summary>
        public void UpdatePosition()
        {
            for (int i = 0; i < stepMax; i++)
            {
                stepSelected = i;
                IncDecStepVal(0);
            }

            stepSelected = 0;
            DestroyStepText();
        }

        /// <summary>
        /// Reset all steps position
        /// </summary>
        public void ResetPosition()
        {
            for (int i = 0; i < stepMax; i++)
            {
                if (steps[i] != null)
                {
                    steps[i].transform.localPosition = new(steps[i].transform.localPosition.x, 0f, steps[i].transform.localPosition.z);
                }
            }
            DestroyStepText();
            DestroyText();
        }

        /// <summary>
        /// Calculate value range from min, max, and delta
        /// </summary>
        public void CalculateValRange()
        {
            // Avoid divide by 0
            if (valDelta != 0)
            {
                valRange = (int)Mathf.Floor((valMax - valMin) / valDelta) + 1;
            }
            else
            {
                valRange = 1;
            }

            // Re-clamp Step Values
            UpdatePosition();
        }

        /// <summary>
        /// Set minimum value
        /// </summary>
        /// <param name="val"></param>
        public void SetValMin(float val)
        {
            // Not checking if valMax < valMin because it is handled by Menu_Sequencer
            valMin = val;
            CalculateValRange();
        }

        /// <summary>
        /// Set maximum value
        /// </summary>
        /// <param name="val"></param>
        public void SetValMax(float val)
        {
            // Not checking if valMin > valMax because it is handled by Menu_Sequencer
            valMax = val;
            CalculateValRange();
        }

        /// <summary>
        /// Set value delta
        /// </summary>
        /// <param name="delta"></param>
        public void SetValDelta(float delta)
        {
            if (delta < 0.001)
            {
                delta = 0.001f;
            }

            valDelta = delta;
            CalculateValRange();
        }

        /// <summary>
        /// Move to next step
        /// </summary>
        public override void Step()
        {
            // Wrap index again in case stepMax has been changed
            stepCurrent %= stepMax;
            clockIndex %= clockDiv;

            if (clockIndex == 0)
            {
                // Toggle graphics update
                stepUpdate = true;

                if (vals[stepCurrent] == 0)
                {
                    outVal = 0;
                }
                else
                {
                    outVal = (vals[stepCurrent] - 1) * valDelta + valMin;
                }

                if (outVal != 0)
                {
                    trigger = 1;
                }
                else
                {
                    trigger = 0;
                }

                // Advance step index
                stepCurrent = (stepCurrent + 1) % stepMax;
            }
            else
            {
                trigger = 0;
            }

            clockIndex = (clockIndex + 1) % clockDiv;
        }

        /// <summary>
        /// Return outlet value
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            return outVal;
        }

        /// <summary>
        /// Return trigger
        /// </summary>
        /// <returns></returns>
        public override int UpdateTrigger()
        {
            return trigger;
        }

        /// <summary>
        /// Override OnSelect
        /// </summary>
        public override void OnSelect()
        {
            Invoke("HighlightSteps", 0.01f);    // why invoke?
        }
        /// <summary>
        /// Override OnDeselect
        /// </summary>
        public override void OnDeselect()
        {
            CancelInvoke("HighlightSteps");
            DehighlightSteps();
        }

        /// <summary>
        /// Override OnEnterEdit
        /// </summary>
        /// <returns></returns>
        public override void OnEnterEdit()
        {
            UpdatePosition();
            SelectStep(0);
        }
        /// <summary>
        /// Override OnExitEdit
        /// </summary>
        public override void OnExitEdit()
        {
            ResetPosition();
            HighlightSteps();
        }

        /// <summary>
        /// Override OnExitMenu
        /// </summary>
        public override void OnExitMenu()
        {
            SelectStep(0);
        }

        /// <summary>
        /// Override OnResetStep
        /// </summary>
        public override void OnResetStep()
        {
            stepCurrent = 0;
            clockIndex = 0;
        }
    }
}
