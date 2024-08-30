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
    public class KeyInput : ControlUnit
    {
        private string key = "a";
        private bool isContinuous = true;
        private bool status = false;
        private bool statusQueue = false;
        private float outVal = 0f;

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
            if (isContinuous)
            {
                status = Input.GetKey(key);
            }
            else
            {
                statusQueue = statusQueue || Input.GetKeyDown(key);
            }
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            StopStepListener();
        }

        /// <summary>
        /// Get current key
        /// </summary>
        /// <returns></returns>
        public string GetKey()
        {
            return key;
        }

        /// <summary>
        /// Set key
        /// </summary>
        /// <param name="val"></param>
        public void SetKey(string val)
        {
            key = val;
        }

        /// <summary>
        /// Get continuous status
        /// </summary>
        /// <returns></returns>
        public bool GetContinuous()
        {
            return isContinuous;
        }

        /// <summary>
        /// Set continuous status
        /// </summary>
        /// <param name="status"></param>
        public void SetContinuous(bool status)
        {
            isContinuous = status;
        }

        // Step
        public override void Step()
        {
            if (isContinuous) return; // queue not relevant if continuous

            // Apply queue to status
            if (statusQueue)
            {
                status = true;
            }
            else
            {
                status = false;
            }
            statusQueue = false;
        }

        // Chained output
        public override float UpdateOutput()
        {
            outVal = System.Convert.ToSingle(status);
            StoreValue(outVal);
            return outVal;
        }

        // trigger output
        public override int UpdateTrigger()
        {
            return (int)GetStoredValue();
        }

        /// <summary>
        /// Apply Unit Properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            SetKey(up.metaString["key"]);
            SetContinuous(up.metaString["continuous"] == "True");
        }

        /// <summary>
        /// Get Unit Properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();

            up.metaString = new();
            up.metaString.Add("key", GetKey());
            up.metaString.Add("continuous", GetContinuous().ToString());

            return up;
        }
    }
}