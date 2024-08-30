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
    public class Trigger : ControlUnit
    {
        [SerializeField] private Button button;
        private bool triggerReady = false;
        private int trigger = 0;

        /////
        //START
        override protected void Start()
        {
            base.Start();
            button.onClick.AddListener(delegate { triggerReady = true; });
            StartStepListener();
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            StopStepListener();
        }

        // Chained output
        public override float UpdateOutput()
        {
            return trigger;
        }

        // Update trigger
        public override int UpdateTrigger()
        {
            return trigger;
        }

        // Reset trigger on clock
        public override void Step()
        {
            if(triggerReady)
            {
                trigger = 1;
            }
            else
            {
                trigger = 0;
            }
            triggerReady = false;
        }
    }
}
