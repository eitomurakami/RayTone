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
    public class Average : ControlUnit
    {
        private float outVal = 0f;
        private float outPrevious = 0f;
        private float timePrevious = 0f;

        /////
        //UPDATE
        private void Update()
        {
            if (!GetInletStatus(0))
            {
                outVal = 0f;
                outPrevious = 0f;
                timePrevious = Time.time;
                return;
            }

            float inVal = GetInletVal(0);
            float time_average = 500;
            if (GetInletStatus(1))
            {
                time_average = Mathf.Max(GetInletVal(1), 1);
            }

            float weight = 2 / ((time_average / ((Time.time - timePrevious) * 1000f)) + 1);

            outVal = outPrevious * (1 - weight) + inVal * weight;
            outPrevious = outVal;
            timePrevious = Time.time;
        }

        // Chained output
        public override float UpdateOutput()
        {
            StoreValue(outVal);
            return outVal;
        }
    }
}
