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
using System.IO;
using System;

namespace RayTone
{
    public class VoiceUnit : Unit
    {
        // Prefab ref
        [Header("VoiceUnit")]
        [SerializeField] private InputSocket Input_PF;
        [SerializeField] private OutputSocket Output_PF;
        [SerializeField] private TextMeshProUGUI voicetext;

        // public variables
        [System.NonSerialized] public string chuckName;
        [System.NonSerialized] public int inputsNum;
        [System.NonSerialized] public int inletsNum;
        [System.NonSerialized] public int valArrFirstIndex;
        [System.NonSerialized] public UnitController unitController;

        // sockets description
        private string[] inputsDesc = {};
        private string[] inletsDesc = {};

        // chuck code
        private string chuckCode;
        private string header;
        private int playCount = 0;
        private bool chuckReady = false;

        // spatialization status
        private bool spatializeLocal = true;

        // panning value for when spatialization is off
        private float panningValue = 0f;

        // local volume
        private float volumeLocal = 1f;

        // reference to components
        private AudioSource audiosource;

        // special case - use outlet
        private bool withOutlet = false;

        // automatic reload
        private bool reload = false;
        private float elapsedTime = 0;
        private float chuckRefreshRate = 2f;
        private System.DateTime lastEdit;

        /////
        //START
        void Start()
        {
            // Get reference to chuck and audio
            myChuck = GetComponent<ChuckSubInstance>();
            myChuck.SetID(GetID().ToString());
            audiosource = GetComponent<AudioSource>();

            // Set spatialization status
            myChuck.spatialize = IsSpatialized();
            audiosource.panStereo = panningValue;

            // Start subinstance
            myChuck.Init();

            // Initialize Voice Unit
            InitVoice();
        }

        /////
        //UPDATE
        void Update()
        {
            UpdateVolume();

            // If reload is enabled, check ChucK text at the refresh rate and compile if there are changes.
            if (!reload) return;

            elapsedTime += Time.deltaTime;
            if (elapsedTime < chuckRefreshRate) return;

            elapsedTime = 0;
            System.DateTime editCheck = File.GetLastWriteTime(RayToneController.CHUCK_DIR + chuckName + ".ck");
            if (lastEdit == editCheck) return;

            lastEdit = editCheck;
            InitVoice();
        }

        /////
        //LATE UPDATE: Wait for chuckReady cue
        private void LateUpdate()
        {
            if(chuckReady)
            {
                PlayChuck();
                chuckReady = false;
            }
        }

        /// <summary>
        /// Called by UnitController if audio buffer size changes
        /// </summary>
        public void OnAudioReset()
        {
            audiosource.Play();
        }

        /// <summary>
        /// Chained output
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            float outVal = 0f;

            if (withOutlet)
            {
                outVal = (float)UnitController.outletVals[GetID()]; // TODO: should there be a public getter?
            }
            StoreValue(outVal);
            return outVal;
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            if(up.metaFloat.ContainsKey("volume_local"))
            {
                SetLocalVolume(up.metaFloat["volume_local"]);
            }
            if (up.metaString.ContainsKey("spatialize"))
            {
                SetSpatialize(up.metaString["spatialize"] == "True");
            } 
            if (up.metaFloat.ContainsKey("panning"))
            {
                SetPanningValue(up.metaFloat["panning"]);
            }
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();

            up.metaString = new();
            up.metaString.Add("chuckName", chuckName);
            up.metaString.Add("file", GetFilePath());
            up.metaString.Add("spatialize", IsSpatialized().ToString());

            up.metaFloat = new();
            up.metaFloat.Add("volume_local", GetLocalVolume());
            up.metaFloat.Add("panning", GetPanningValue());

            return up;
        }

        /// <summary>
        /// Update volume by multiplying global and local volume
        /// </summary>
        public void UpdateVolume()
        {
            audiosource.volume = System.Convert.ToInt32(output.GetConnectionCount() == 0) * volumeLocal * UnitController.GetGlobalVolume();  //Add global volume
        }

        /// <summary>
        /// Set local volume
        /// </summary>
        /// <param name="val"></param>
        public void SetLocalVolume(float val)
        {
            volumeLocal = Mathf.Clamp(val, 0f, 1f);
        }

        /// <summary>
        /// Get local volume
        /// </summary>
        /// <returns></returns>
        public float GetLocalVolume()
        {
            return volumeLocal;
        }

        /// <summary>
        /// Set spatialization status
        /// </summary>
        /// <param name="arg"></param>
        public void SetSpatialize(bool arg)
        {
            spatializeLocal = arg;
            if(myChuck != null)
            {
                myChuck.spatialize = arg;
            }
        }

        /// <summary>
        /// Get spatialization status
        /// </summary>
        /// <returns></returns>
        public bool IsSpatialized()
        {
            return spatializeLocal;
        }

        /// <summary>
        /// Set panning value
        /// </summary>
        /// <param name="val"></param>
        public void SetPanningValue(float val)
        {
            panningValue = val;
            if (audiosource != null)
            {
                audiosource.panStereo = panningValue;
            }
        }

        /// <summary>
        /// Get panning value
        /// </summary>
        /// <returns></returns>
        public float GetPanningValue()
        {
            return panningValue;
        }

        /// <summary>
        /// Cue chuck
        /// </summary>
        public void CueChuck()
        {
            CancelInvoke();
            chuckReady = true;
        }

        /// <summary>
        /// Reset file path
        /// </summary>
        /// <param name="filePath_arg"></param>
        public override void ReattachFilePath()
        {
            //Restart ChucK...
            CueChuck();
        }

        /// <summary>
        /// (Re)Initialize Voice Unit
        /// </summary>
        private void InitVoice()
        {
            // Read chuck code
            chuckCode = File.ReadAllText(RayToneController.CHUCK_DIR + chuckName + ".ck");

            // Read header
            header = File.ReadAllText(RayToneController.SOURCE_DIR + "voiceUnitHeader.ck");

            // Check if the voice requires automatic reload
            string reload_arg = RayToneUtil.FindStringArg(chuckCode, "RAYTONE_RELOAD(", ");");
            reload = (reload_arg == "true");
            // Store edit time
            if (reload)
            {
                lastEdit = File.GetLastWriteTime(RayToneController.CHUCK_DIR + chuckName + ".ck");
            }

            // Check if the voice requires an outlet and attach a control unit
            string outlet_arg = RayToneUtil.FindStringArg(chuckCode, "RAYTONE_DEFINE_OUTLET(", ");");
            withOutlet = (outlet_arg == "true");

            // Initialize inlets
            SpawnSockets();

            // Cue chuck file
            Invoke(nameof(CueChuck), 0.1f);
        }

        /// <summary>
        /// Run chuck file
        /// </summary>
        private void PlayChuck()
        {
            // Remove existing shred in this chuck subinstance
            if (playCount > 0)
            {
                unitController.RemoveShredWithVoiceID(GetID());
            }

            // Append header
            string chuckCodeMod = header + chuckCode;

            // Replace RAYTONE_OUTLET
            chuckCodeMod = chuckCodeMod.Replace("RAYTONE_OUTLET", "outletArr_chuck[raytone_id]");

            // Replace RAYTONE_OUTPUT 
            string gainMagic = "Gain raytone_gain_local_out1 => global Gain raytone_gain" + GetID().ToString() + " => Gain raytone_gain_local_out2";
            chuckCodeMod = chuckCodeMod.Replace("RAYTONE_OUTPUT", gainMagic);

            // Replace RAYTONE_INPUT
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].connectedVoiceIndex == -1)
                {
                    if (chuckCodeMod.Contains("raytone_gain_null"))
                    {
                        chuckCodeMod = chuckCodeMod.Replace("RAYTONE_INPUT(" + i.ToString() + ")", "raytone_gain_null");
                    }
                    chuckCodeMod = chuckCodeMod.Replace("RAYTONE_INPUT(" + i.ToString() + ")", "Gain raytone_gain_null");
                }
                else
                {
                    chuckCodeMod = chuckCodeMod.Replace("RAYTONE_INPUT(" + i.ToString() + ")", "global Gain raytone_gain" + inputs[i].connectedVoiceIndex.ToString() + " => Gain raytone_gain_local_in" + "_" + i.ToString()); //old - playCount.ToString()

                }
            }

            // Replace RAYTONE_INLET
            for (int i = 0; i < inlets.Length; i++)
            {
                chuckCodeMod = chuckCodeMod.Replace("RAYTONE_INLET(" + i.ToString() + ")", "valArr_chuck[raytone_index + " + i.ToString() + "]");
            }

            // Replace RAYTONE_TRIG
            for (int i = 0; i < inlets.Length; i++)
            {
                chuckCodeMod = chuckCodeMod.Replace("RAYTONE_TRIG(" + i.ToString() + ")", "trigArr_chuck[raytone_index + " + i.ToString() + "]");
            }

            // Replace RAYTONE_INLET_STATUS
            for (int i = 0; i < inlets.Length; i++)
            {
                chuckCodeMod = chuckCodeMod.Replace("RAYTONE_INLET_STATUS(" + i.ToString() + ")", "statusArr_chuck[raytone_index + " + i.ToString() + "]");
            }

            // Comment out RayTone macros
            if (chuckCodeMod.Contains("RAYTONE_DEFINE_INPUTS"))
            {
                chuckCodeMod = chuckCodeMod.Insert(chuckCodeMod.IndexOf("RAYTONE_DEFINE_INPUTS"), "//");
            }
            if(chuckCodeMod.Contains("RAYTONE_DEFINE_INLETS"))
            {
                chuckCodeMod = chuckCodeMod.Insert(chuckCodeMod.IndexOf("RAYTONE_DEFINE_INLETS"), "//");
            }
            if (chuckCodeMod.Contains("RAYTONE_DEFINE_OUTLET"))
            {
                chuckCodeMod = chuckCodeMod.Insert(chuckCodeMod.IndexOf("RAYTONE_DEFINE_OUTLET"), "//");
            }
            if (chuckCodeMod.Contains("RAYTONE_LOADFILE"))
            {
                chuckCodeMod = chuckCodeMod.Insert(chuckCodeMod.IndexOf("RAYTONE_LOADFILE"), "//");
            }
            if (chuckCodeMod.Contains("RAYTONE_RELOAD"))
            {
                chuckCodeMod = chuckCodeMod.Insert(chuckCodeMod.IndexOf("RAYTONE_RELOAD"), "//");
            }

            // workaround for adding arguments when using RunCode instead of RunFile
            chuckCodeMod = chuckCodeMod.Replace("me.arg(0)", '"' + GetID().ToString() + '"');
            chuckCodeMod = chuckCodeMod.Replace("me.arg(1)", '"' + valArrFirstIndex.ToString() + '"');
            chuckCodeMod = chuckCodeMod.Replace("me.arg(2)", '"' + GetFilePath() + '"');

            // Play Chuck file and pass id, index, and filedir
            myChuck.RunCode(chuckCodeMod);

            // Update text display
            voicetext.text = chuckName;
            if (chuckName.Contains("/"))
            {
                voicetext.text = chuckName.Split("/")[1];
            }
            if (!string.IsNullOrEmpty(GetFilePath()))
            {
                voicetext.text = Path.GetFileNameWithoutExtension(GetFilePath());
            }

            playCount++;
        }

        /// <summary>
        /// Spawn input
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private InputSocket SpawnInput(Vector3 position)
        {
            InputSocket newInput = Instantiate(Input_PF);
            newInput.transform.parent = this.transform;
            newInput.transform.localPosition = position;

            return newInput;
        }

        /// <summary>
        /// Spawn 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private OutputSocket SpawnOutput(Vector3 position)
        {
            OutputSocket newOutput = Instantiate(Output_PF);
            newOutput.transform.parent = this.transform;
            newOutput.transform.localPosition = position;

            return newOutput;
        }

        /// <summary>
        /// Read RAYTONE_INLETS macro and spawn inlets with description and indeces
        /// </summary>
        private void SpawnSockets()
        {
            // Read RAYTONE_DEFINE_INPUTS macro
            string inputs_args = RayToneUtil.FindStringArg(chuckCode, "RAYTONE_DEFINE_INPUTS(", ");");
            if(!string.IsNullOrWhiteSpace(inputs_args))
            {
                inputsDesc = inputs_args.Split(",");
            }
            else
            {
                Array.Resize(ref inputsDesc, 0);
            }
            for (int i = 0; i < inputsDesc.Length; i++)
            {
                inputsDesc[i] = RayToneUtil.FindStringArg(inputsDesc[i], char.ToString('"'), char.ToString('"'));
            }
            inputsNum = inputsDesc.Length;
            // If the number of inputs changes on reload: disconnect, destroy, and resize 
            if (inputs != null)
            {
                if (inputs.Length > inputsNum)
                {
                    for (int i = inputsNum; i < inputs.Length; i++)
                    {
                        if (inputs[i].IsConnected())
                        {
                            inputs[i].connectedOutput.Disconnect(inputs[i]);
                        }
                        Destroy(inputs[i].gameObject);
                    }
                }
                if (inputs.Length != inputsNum)  // Resize up or down
                {
                    Array.Resize(ref inputs, inputsNum);
                }
            }
            else
            {
                inputs = new InputSocket[inputsNum];
            }         

            // Read RAYTONE_DEFINE_INPUTS macro
            string inlets_args = RayToneUtil.FindStringArg(chuckCode, "RAYTONE_DEFINE_INLETS(", ");");
            if (!string.IsNullOrWhiteSpace(inlets_args))
            {
                inletsDesc = inlets_args.Split(",");
            }
            else
            {
                Array.Resize(ref inletsDesc, 0);
            }
            for (int i = 0; i < inletsDesc.Length; i++)
            {
                inletsDesc[i] = RayToneUtil.FindStringArg(inletsDesc[i], char.ToString('"'), char.ToString('"'));
            }
            // Clamp inlet num 
            inletsNum = Mathf.Min(inletsDesc.Length, UnitController.INLET_NUM_MAX);
            // If the number of inlets changes on reload: disconnect, destroy, and resize 
            if (inlets != null)
            {
                if (inlets.Length > inletsNum)
                {
                    for (int i = inletsNum; i < inlets.Length; i++)
                    {
                        if (inlets[i].IsConnected())
                        {
                            inlets[i].connectedOutlet.Disconnect(inlets[i]);
                        }
                        Destroy(inlets[i].gameObject);
                    }
                }
                if (inlets.Length != inletsNum)  // Resize up or down
                {
                    Array.Resize(ref inlets, inletsNum);
                }
            }
            else
            {
                inlets = new InletSocket[inletsNum];
            }

            // Spawn Inputs + Inlets + Output
            int j = 0;
            for (int i = 0; i < inputsNum + inletsNum + 1; i++)
            {
                if (i < inputsNum)
                {
                    InputSocket input = inputs[i];
                    if (inputs[i] == null)
                    {
                        input = SpawnInput(Vector3.zero);
                        inputs[i] = input;
                    }
                    input.transform.localPosition = new(0f, 0f, -2.25f * (i + 1.3f));
                    input.inputIndex = i;
                    input.parentVoice = this;
                    input.SetDescription(inputsDesc[i]);
                    
                }
                else if (i >= inputsNum && i < inputsNum + inletsNum)
                {
                    InletSocket inlet = inlets[j];
                    if (inlets[j] == null)
                    {
                        inlet = SpawnInlet(Vector3.zero);
                        inlets[j] = inlet;
                    }
                    inlet.transform.localPosition = new(0f, 0f, -2.25f * (i + 1.3f));
                    inlet.isVoice = true;
                    inlet.inletIndex = j;
                    inlet.valArrIndex = valArrFirstIndex + j;
                    inlet.parentUnit = this;
                    inlet.SetDescription(inletsDesc[j]);
                    j++;
                }
                else if (i >= inputsNum + inletsNum)
                {
                    if (output == null)
                    {
                        output = SpawnOutput(Vector3.zero);
                    }
                    output.transform.localPosition = new(0f, 0f, -2.25f * (i + 1.3f));
                    output.parentVoice = this;
                    output.SetDescription("signal out");
                }
            }
            if (withOutlet)
            {
                if (outlet == null)
                {
                    outlet = SpawnOutlet(Vector3.zero);
                }
                outlet.transform.localPosition = new(0f, 0f, -2.25f * ((inputsNum + inletsNum + 1) + 1.3f));
                outlet.parentUnit = this;
            }
            else
            {
                if (outlet != null)
                {
                    outlet.DisconnectAll();
                    Destroy(outlet.gameObject);
                }
            }
        }
    }
}   
