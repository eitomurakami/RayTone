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

namespace RayTone
{
    public class Elapsed : ControlUnit
    {
        private float outVal = 0f;
        private float timeInit = 0f;
        private bool timeInitUpdate = false;
        private bool stepReady = false;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            StartStepListener();
        }

        /////
        //UPDATE
        private void Update()
        {
            // Reset init time
            if (timeInitUpdate)
            {
                timeInit = Time.time;
                timeInitUpdate = false;
            }

            outVal = Time.time - timeInit;
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            StopStepListener();
        }

        // Step
        public override void Step()
        {
            stepReady = true;
        }

        // Chained output
        public override float UpdateOutput()
        {
            if (stepReady)
            {
                if (GetInletStatus(0))
                {
                    GetInletVal(0);  // Force recursive inlet update

                    // Update init time on trigger
                    if (inlets[0].connectedUnit.UpdateTrigger() == 1)
                    {
                        timeInitUpdate = true;
                    }
                }
                stepReady = false;
            }

            StoreValue(outVal);
            return outVal;
        }
    }
}