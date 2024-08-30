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
    public class MIDI_CC_S : ControlUnit
    {
        private MIDIController midiController;

        private int midiChannel = 1;
        private int ccNumber = 60;

        /////
        //START
        protected override void Start()
        {
            base.Start();

            midiController = MIDIController.Instance;
        }

        /// <summary>
        /// Override custom update
        /// </summary>
        private void Update()
        {
            if (GetInletStatus(0))
            {
                for (int i = 0; i < midiController.GetMIDIOutPorts().Count; i++)
                {
                    midiController.GetMIDIOutPorts()[i].SendControlChange((midiChannel - 1), ccNumber, (int)GetInletVal(0));  // RT-MIDI uses 0-indexed channel
                }
            }
        }

        /// <summary>
        /// Set MIDI channel
        /// </summary>
        /// <param name="channel"></param>
        public void SetMIDIChannel(int channel)
        {
            midiChannel = channel;
        }

        /// <summary>
        /// Get MIDI Channel
        /// </summary>
        /// <returns></returns>
        public int GetMIDIChannel()
        {
            return midiChannel;
        }

        /// <summary>
        /// Set CC number
        /// </summary>
        /// <param name="number"></param>
        public void SetCCNumber(int number)
        {
            ccNumber = number;
        }

        /// <summary>
        /// Get CC number
        /// </summary>
        /// <returns></returns>
        public int GetCCNumber()
        {
            return ccNumber;
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            if (up.metaInt.ContainsKey("midi_channel"))
            {
                midiChannel = up.metaInt["midi_channel"];
            }
            if (up.metaInt.ContainsKey("cc_number"))
            {
                ccNumber = up.metaInt["cc_number"];
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
            up.metaInt.Add("midi_channel", midiChannel);
            up.metaInt.Add("cc_number", ccNumber);

            return up;
        }
    }
}
