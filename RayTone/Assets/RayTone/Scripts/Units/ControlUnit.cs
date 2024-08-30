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
    public class ControlUnit : Unit
    {
        // inlets description
        [SerializeField] private string[] inletsDescription;
        [SerializeField] private Vector3[] inletsOrientation;

        // category
        public ControlCategory controlCategory;

        /////
        //START
        protected virtual void Start()
        {
            if (outlet)
            {
                outlet.parentUnit = this;
            }

            // Assign inlet description
            if (inlets != null)
            {
                for (int i = 0; i < inlets.Length; i++)
                {
                    inlets[i].parentUnit = this;
                    inlets[i].inletIndex = i;

                    if (i < inletsDescription.Length)
                    {
                        inlets[i].SetDescription(inletsDescription[i]);
                        inlets[i].SetOrientation(inletsOrientation[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            //
        }
        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();
            return up;
        }
    }
}