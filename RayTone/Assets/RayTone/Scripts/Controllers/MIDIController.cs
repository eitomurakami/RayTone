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
using RtMidi.LowLevel;

namespace RayTone
{
    public class MIDIController : Singleton<MIDIController>
    {
        // MIDI
        private MidiProbe midiInProbe;
        private MidiProbe midiOutProbe;
        private List<MidiInPort> midiInPorts = new();
        private List<MidiOutPort> midiOutPorts = new();

        /////
        //START
        private void Start()
        {
            // Initialize MIDI probe
            midiInProbe = new MidiProbe(MidiProbe.Mode.In);
            midiOutProbe = new MidiProbe(MidiProbe.Mode.Out);
            RescanMIDIPorts();
        }

        /////
        //UPDATE
        private void Update()
        {
            // Process queued messages in the opened ports.
            foreach (MidiInPort port in midiInPorts)
            {
                port?.ProcessMessages();
            }
        }

        /////
        //ON-DESTROY
        void OnDestroy()
        {
            midiInProbe?.Dispose();
            midiOutProbe?.Dispose();
            DisposeMIDIPorts();
        }

        /// <summary>
        /// Scan MIDI ports
        /// </summary>
        public void RescanMIDIPorts()
        {
            DisposeMIDIPorts();

            for (int i = 0; i < midiInProbe.PortCount; i++)
            {
                string name = midiInProbe.GetPortName(i);
                Console.Log("Detected MIDI-In device: " + name.Substring(0, name.Length - 2) + ".\n");

                midiInPorts.Add(new MidiInPort(i));
            }

            for (int i = 0; i < midiOutProbe.PortCount; i++)
            {
                string name = midiOutProbe.GetPortName(i);
                Console.Log("Detected MIDI-Out device: " + name.Substring(0, name.Length - 2) + ".\n");

                midiOutPorts.Add(new MidiOutPort(i));
            }
        }

        /// <summary>
        /// Get MIDI in ports
        /// </summary>
        /// <returns></returns>
        public List<MidiInPort> GetMIDIInPorts()
        {
            return midiInPorts;
        }

        /// <summary>
        /// Get MIDI out ports 
        /// </summary>
        /// <returns></returns>
        public List<MidiOutPort> GetMIDIOutPorts()
        {
            return midiOutPorts;
        }

        /// <summary>
        /// Discard existing MIDI Ports
        /// </summary>
        private void DisposeMIDIPorts()
        {
            foreach (MidiInPort inPort in midiInPorts)
            {
                inPort?.Dispose();
            }
            midiInPorts.Clear();

            foreach (MidiOutPort outPort in midiOutPorts)
            {
                outPort?.Dispose();
            }
            midiOutPorts.Clear();
        }
    }
}
