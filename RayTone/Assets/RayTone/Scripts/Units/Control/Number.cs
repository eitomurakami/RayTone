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
    public class Number : ControlUnit
    {
        [SerializeField] private TMP_InputField input;

        private string currentNumber = "0";

        /////
        //AWAKE
        private void Awake()
        {
            input.caretWidth = 0;
        }

        /////
        //START
        protected override void Start()
        {
            base.Start();
            input.onEndEdit.AddListener(delegate { GetCurrentText(); });
        }

        /// <summary>
        /// Get latest text
        /// </summary>
        /// <param name="text"></param>
        private void GetCurrentText()
        {
            currentNumber = input.text;
        }

        /// <summary>
        /// Override chained output
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            float output;
            float.TryParse(currentNumber, out output);

            StoreValue(output);
            return output;
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            input.text = up.metaString["number"];
            GetCurrentText();
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();

            up.metaString = new();
            up.metaString.Add("number", input.text);
            return up;
        }
    }
}
