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
    public class InletSocket : Socket
    {
        [System.NonSerialized] public Unit parentUnit;    // parent Unit
        [System.NonSerialized] public Unit connectedUnit;  // unit of connected outlet
        [System.NonSerialized] public OutletSocket connectedOutlet;    // connected outlet
        [System.NonSerialized] public Cable cable;
        [System.NonSerialized] public int inletIndex;
        [System.NonSerialized] public int valArrIndex;
        [System.NonSerialized] public bool isVoice = false;

        /////
        //UPDATE
        private void Update()
        {
            if (cable != null)
            {
                // Dynamically update cable end position
                cable.targetPosition = transform.position;
            }
        }

        /// <summary>
        /// sub update
        /// </summary>
        private void SubUpdate()
        {
            if (connectedUnit != null && isVoice)
            {
                // Read from recursive chained structures
                UnitController.valArr[valArrIndex] = connectedUnit.UpdateOutput();
                UnitController.trigArr[valArrIndex] = connectedUnit.UpdateTrigger();
                UnitController.statusArr[valArrIndex] = IsConnected() ? 1 : 0;
            }
        }

        /// <summary>
        /// Is Inlet connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            bool result = false;
            if (connectedOutlet != null)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Conenct to Inlet
        /// </summary>
        /// <param name="outlet_arg"></param>
        /// <param name="control_arg"></param>
        /// <param name="cable_arg"></param>
        public void Connect(OutletSocket outlet_arg, Unit unit_arg, Cable cable_arg)
        {
            connectedOutlet = outlet_arg;
            connectedUnit = unit_arg;
            cable = cable_arg;
            if (isVoice)
            {
                UnitController.OnSubUpdate += SubUpdate;
            }
        }

        /// <summary>
        /// Disconnect from Inlet
        /// </summary>
        public void Disconnect()
        {
            if (cable != null)
            {
                Destroy(cable.gameObject);
                cable = null;
            }
            connectedUnit = null;
            connectedOutlet = null;
            if (isVoice)
            {
                UnitController.valArr[valArrIndex] = 0f;
                UnitController.trigArr[valArrIndex] = 0;
                UnitController.statusArr[valArrIndex] = 0;
                UnitController.OnSubUpdate -= SubUpdate;
            }
        }
    }
}
