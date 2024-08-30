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
    public class Clock : MonoBehaviour
    {
        private static int bpm = 120;
        private static float clock = 0f;

        // reference to ChuckMainInstance
        [SerializeField] private ChuckMainInstance chuckMainInstance;
        private static ChuckMainInstance chuckMainInstance_static;

        /////
        //AWAKE
        private void Awake()
        {
            chuckMainInstance_static = chuckMainInstance;
        }

        /////
        //START
        void Start()
        { 
            Run();
            UpdateClockDelay();
        }

        /// <summary>
        /// Set clock
        /// </summary>
        /// <param name="bpm_arg"></param>
        public static void SetBPM(int bpm_arg)
        {
            bpm = (int)Mathf.Clamp(bpm_arg, 30, 250);
            clock = 60f / bpm / 4f * 1000f;
            chuckMainInstance_static.SetFloat("RAYTONE_BPM", bpm);
            chuckMainInstance_static.SetFloat("clock", clock);
        }

        /// <summary>
        /// Get clock
        /// </summary>
        /// <returns></returns>
        public static float GetBPM()
        {
            return bpm;
        }

        /// <summary>
        /// Update ChucK clock delay based on the buffer length
        /// </summary>
        public static void UpdateClockDelay()
        {
            AudioSettings.GetDSPBufferSize(out int audioBufferLength, out int audioBufferNum);
            int clock_delay = (int)(((float)audioBufferLength / (float)AudioSettings.outputSampleRate) * 1000 + 1);
            chuckMainInstance_static.SetFloat("clock_delay", clock_delay);
        }

        /// <summary>
        /// Run
        /// </summary>
        public void Run()
        {
            chuckMainInstance_static.RunFile("src/clock.ck", true);
            SetBPM(bpm);
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            //
        }
    }
}
