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
    public class OutputSocket : Socket
    {
        // prefab reference
        [System.NonSerialized] public Cable Cable_PF;
        
        [System.NonSerialized] public VoiceUnit parentVoice;    // reference to parent voice
        [System.NonSerialized] public List<Unit> connectedUnits = new();
        private List<InputSocket> connectedInputs = new(); 

        private int connectionCount = 0;

        /// <summary>
        /// Connect to an Input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cable"></param>
        /// <returns></returns>
        public bool Connect(InputSocket input, Cable cable)
        {
            bool success = false;

            if (input != null && cable != null && input.parentVoice.GetID() != parentVoice.GetID())
            {
                input.Connect(this, parentVoice.GetID(), cable);
                connectedUnits.Add(input.parentVoice);
                connectedInputs.Add(input);
                connectionCount++;

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Disconnect from a specific Input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool Disconnect(InputSocket input)
        {
            bool success = false;

            if (input != null)
            {
                input.Disconnect();
                connectedUnits.Remove(input.parentVoice);
                connectedInputs.Remove(input);
                connectionCount--;

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Disconnect all connections from this output
        /// </summary>
        public void DisconnectAll()
        {
            for (int i = connectedInputs.Count - 1; i >= 0; i--)
            {
                Disconnect(connectedInputs[i]);
            }
        }

        /// <summary>
        /// Get connection status
        /// </summary>
        /// <returns></returns>
        public int GetConnectionCount()
        {
            return connectionCount;
        }
    }
}
