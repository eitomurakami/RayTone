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
    public class OutletSocket : Socket
    {
        // prefab reference
        [System.NonSerialized] public Cable Cable_PF;

        [System.NonSerialized] public Unit parentUnit; // parent Unit
        [System.NonSerialized] public List<Unit> connectedUnits = new();
        private List<InletSocket> connectedInlets = new();

        /// <summary>
        /// Connect to an Inlet
        /// </summary>
        /// <param name="inlet"></param>
        /// <param name="cable"></param>
        /// <returns></returns>
        public bool Connect(InletSocket inlet, Cable cable)
        {
            bool success = false;

            if (inlet != null && cable != null)
            {
                inlet.Connect(this, parentUnit, cable);
                connectedUnits.Add(inlet.parentUnit);
                connectedInlets.Add(inlet);

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Disconnect from a specific Inlet
        /// </summary>
        /// <param name="inlet"></param>
        /// <returns></returns>
        public bool Disconnect(InletSocket inlet)
        {
            bool success = false;

            if (inlet != null)
            {
                inlet.Disconnect();
                connectedUnits.Remove(inlet.parentUnit);
                connectedInlets.Remove(inlet);
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Disconnect all connections from this outlet
        /// </summary>
        public void DisconnectAll()
        {
            for (int i = connectedInlets.Count - 1; i >= 0; i--)
            {
                Disconnect(connectedInlets[i]);
            }
        }
    }
}
