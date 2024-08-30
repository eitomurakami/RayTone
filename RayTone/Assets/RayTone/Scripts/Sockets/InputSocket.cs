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
    public class InputSocket : Socket
    {
        [System.NonSerialized] public VoiceUnit parentVoice;  // parent VoiceUnit
        [System.NonSerialized] public VoiceUnit connectedVoice; // VoiceUnit of connected output
        [System.NonSerialized] public int connectedVoiceIndex = -1;   // index of connected VoiceUnit
        [System.NonSerialized] public OutputSocket connectedOutput;  // connected output
        [System.NonSerialized] public int inputIndex;
        [System.NonSerialized] public Cable cable;

        private void Awake()
        {
            connectedVoiceIndex = -1;    // why do i need this?
        }

        /// <summary>
        /// Is Input connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            bool result = false;
            if (connectedOutput != null)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Connect to Input
        /// </summary>
        public void Connect(OutputSocket output_arg, int voiceIndex_arg, Cable cable_arg)
        {
            connectedOutput = output_arg;
            connectedVoiceIndex = voiceIndex_arg;
            cable = cable_arg;
            connectedVoice = connectedOutput.parentVoice;

            parentVoice.CueChuck();
        }

        /// <summary>
        /// Disconnect from Input
        /// </summary>
        public void Disconnect()
        {
            if(cable != null)
            {
                Destroy(cable.gameObject);
                cable = null;
            }
            connectedVoiceIndex = -1;
            connectedOutput = null;
            connectedVoice = null;

            parentVoice.CueChuck();
        }

        /////
        //Update
        void Update()
        {
            if (cable != null)
            {
                // Dynamically update cable end position
                cable.targetPosition = transform.position;
            }
        }
    }
}
