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
    public class Toggle : ControlUnit
    {
        [SerializeField] private UnityEngine.UI.Toggle toggle;
        private int val = 0;

        /////
        //START
        override protected void Start()
        {
            base.Start();
            toggle.onValueChanged.AddListener(delegate { OnToggleChanged(toggle.isOn); });
        }

        // Chained output
        public override float UpdateOutput()
        {
            StoreValue(val);
            return val;
        }

        // Update trigger
        public override int UpdateTrigger()
        {
            return 0;
        }

        // Called by toggle
        public void OnToggleChanged(bool isOn)
        {
            if(isOn)
            {
                val = 1;
            }
            else
            {
                val = 0;
            }
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            val = up.metaInt["toggle"];

            if(val == 1)
            {
                toggle.SetIsOnWithoutNotify(true);
            }
            else
            {
                toggle.SetIsOnWithoutNotify(false);
            }
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();

            up.metaInt = new();
            up.metaInt.Add("toggle", val);
            return up;
        }
    }
}
