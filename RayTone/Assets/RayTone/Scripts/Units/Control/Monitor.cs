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

namespace RayTone
{
    public class Monitor : ControlUnit
    {
        public TextMeshProUGUI text;
        private float outVal = 0f;

        /////
        //START
        override protected void Start()
        {
            base.Start();
        }

        /////
        //UPDATE
        private void Update()
        {
            text.text = (Mathf.Round(GetInletVal(0) * 100000f) / 100000f).ToString();
        }

        // Chained output
        public override float UpdateOutput()
        {
            outVal = GetInletVal(0);

            StoreValue(outVal);
            return outVal;
        }

        /// <summary>
        /// Queue render frame
        /// </summary>
        /// <param name="inlet"></param>
        public override void QueueRenderFrame(InletSocket inlet)
        {
            NotifyQueueRenderFrame();
        }
    }
}
