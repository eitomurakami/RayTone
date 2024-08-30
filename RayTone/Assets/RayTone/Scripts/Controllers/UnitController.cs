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
using System.Linq;
using UnityEngine;
using System.IO;

#if UNITY_WEBGL
using CK_INT = System.Int32;
using CK_UINT = System.UInt32;
#elif UNITY_ANDROID
using CK_INT = System.IntPtr;
using CK_UINT = System.UIntPtr;
#else
using CK_INT = System.Int64;
using CK_UINT = System.UInt64;
#endif
using CK_FLOAT = System.Double;

namespace RayTone
{
    [System.Serializable]
    public struct RayToneProject
    {
        [System.NonSerialized] public string directory;
        public UnitProperties[] unit_properties;
    }

    public enum ControlCategory
    {
        GUI,
        Math,
        Time,
        Utilities
    };

    public enum GraphicsCategory
    {
        Graphics
    };

    public class UnitController : Singleton<UnitController>
    {
        // prefab references
        [SerializeField] private VoiceUnit Voice_PF;
        [SerializeField] private ControlUnit[] Control_PF = new ControlUnit[10];
        [SerializeField] private GraphicsUnit[] Graphics_PF = new GraphicsUnit[10];
        [SerializeField] private ChuckMainInstance chuckMainInstance;

        // ChucK(voice) categories + items
        private string[] chuckCategories;
        private List<string> chuckFiles = new();
        private Dictionary<int, List<string>> chuckFilesByCategory = new();

        // Control categories + items
        private Dictionary<int, List<string>> controlByCategory = new();
        // Graphics categories + items
        private Dictionary<int, List<string>> graphicsByCategory = new();

        // max number of Inlets for Voice Units
        public const int INLET_NUM_MAX = 10;

        // public value arrays
        public static CK_FLOAT[] valArr = new CK_FLOAT[10000];
        public static CK_INT[] trigArr = new CK_INT[10000];
        public static CK_INT[] statusArr = new CK_INT[10000];
        public static CK_INT[] shredId = new CK_INT[1000];
        public static CK_FLOAT[] outletVals = new CK_FLOAT[1000];

        // controller reference
        private RayToneController raytoneController;

        // units arrays
        private bool[] controlsAvailability = new bool[1000];
        private bool[] voicesAvailability = new bool[1000];
        private bool[] graphicsAvailability = new bool[100];
        [System.NonSerialized] public Dictionary<int, ControlUnit> controls = new();
        [System.NonSerialized] public Dictionary<int, VoiceUnit> voices = new();
        [System.NonSerialized] public Dictionary<int, GraphicsUnit> graphics = new();
        [System.NonSerialized] public Dictionary<(UnitType, int), Unit> units = new();

        // global volume
        private static float volumeGlobal = 1f;

        // reference to audio source
        private AudioSource audioSource;
        // reference to ChuckSubInstance
        private ChuckSubInstance myChuck;
        // reference to ChuckEventListener
        private ChuckEventListener myEventListener;
        // Action delegate on Step
        public static System.Action OnStep;
        // Action delegate on sub update
        public static System.Action OnSubUpdate;

        // shredID callback from Chuck
        private Chuck.IntArrayCallback shredIdCallback;
        // outlet callback from chuck
        private Chuck.FloatArrayCallback outletCallback;

        /////
        //AWAKE
        protected override void Awake()
        {
            base.Awake();
        }

        /////
        //START
        void Start()
        {
            // Get controller reference
            raytoneController = RayToneController.Instance;

            // Initialize ChucK
            audioSource = GetComponent<AudioSource>();
            myChuck = GetComponent<ChuckSubInstance>();
            chuckMainInstance.RunFile("src/chuckGlobals", true);
            shredIdCallback = GetShredID;
            outletCallback = GetOutlet;

            // Start listening for RAYTONE_TICK_CONTROL
            myEventListener = gameObject.GetComponent<ChuckEventListener>();
            Invoke(nameof(StartStepListener), 1f);

            // Parse Control categories
            for (int i = 0; i < System.Enum.GetNames(typeof(ControlCategory)).Length; i++)
            {
                controlByCategory.Add(i, new List<string>());
            }
            for (int i = 0; i < Control_PF.Length; i++)
            {
                if(Control_PF[i] != null)
                {
                    int category = (int)Control_PF[i].controlCategory;
                    controlByCategory[category].Add(Control_PF[i].GetType().ToString());
                    controlByCategory[category].Sort();
                }
            }
            // Parse Graphics categories
            for (int i = 0; i < System.Enum.GetNames(typeof(GraphicsCategory)).Length; i++)
            {
                graphicsByCategory.Add(i, new List<string>());
            }
            for (int i = 0; i < Graphics_PF.Length; i++)
            {
                if (Graphics_PF[i] != null)
                {
                    int category = (int)Graphics_PF[i].graphicsCategory;
                    graphicsByCategory[category].Add(Graphics_PF[i].GetType().ToString());
                    graphicsByCategory[category].Sort();
                }
            }
            LoadChuckFiles();
        }

        /////
        //UPDATE
        void Update()
        {
            AssignShredIDCallback();
            AssignOutletCallback();
        }

        /////
        //OnAudioFilterRead
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!myChuck || !myEventListener) return;

            myEventListener.SubUpdate();
            OnSubUpdate?.Invoke();

            // Pass ValArr and TrigArr to Chuck global arrays
            myChuck.SetFloatArray("valArr_chuck", valArr);
            myChuck.SetIntArray("trigArr_chuck", trigArr);
            myChuck.SetIntArray("statusArr_chuck", statusArr);
        }

        /// <summary>
        /// Start listening for ChucK event
        /// </summary>
        private void StartStepListener()
        {
            chuckMainInstance.SetFloat("RAYTONE_LOCAL_GAIN", raytoneController.ReadUserConfig().localGain);
            myEventListener.ListenForEvent(myChuck, "RAYTONE_TICK_CONTROL", Step);
            Clock.UpdateClockDelay();
        }

        /// <summary>
        /// Called by RAYTONE_TICK_CONTROL
        /// </summary>
        private void Step()
        {
            OnStep?.Invoke();
        }

        /// <summary>
        /// Get audio buffer length
        /// </summary>
        /// <returns></returns>
        public int GetAudioBufferLength()
        {
            AudioSettings.GetDSPBufferSize(out int audioBufferLength, out int audioBufferNum);
            return audioBufferLength;
        }

        /// <summary>
        /// Set new audio buffer length
        /// </summary>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        public int SetAudioBufferLength(int bufferLength)
        {
            AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
            audioConfig.dspBufferSize = bufferLength;
            AudioSettings.Reset(audioConfig);
            AudioSettings.GetDSPBufferSize(out int audioBufferLength, out int audioBufferNum);
            Console.Log("Set audio buffer length: " + audioBufferLength);

            Clock.UpdateClockDelay();

            //Restart audio sources
            audioSource.Play();
            myChuck.chuckMainInstance.OnAudioReset();
            foreach (VoiceUnit voice in voices.Values)
            {
                voice.OnAudioReset();
            }

            return audioBufferLength;
        }

        /// <summary>
        /// Get all available Chuck files
        /// </summary>
        public void LoadChuckFiles()
        {
            chuckFilesByCategory.Clear();

            chuckCategories = Directory.GetDirectories(RayToneController.CHUCK_DIR);
            System.Array.Sort(chuckCategories);
            for(int i = 0; i < chuckCategories.Length; i++)
            {
                List<string> files = Directory.GetFiles(chuckCategories[i], "*.ck").ToList();
                chuckCategories[i] = chuckCategories[i].Replace(RayToneController.CHUCK_DIR, "");

                for (int j = 0; j < files.Count; j++)
                {
                    files[j] = chuckCategories[i] + "/" + Path.GetFileNameWithoutExtension(files[j]);
                    chuckFiles.Add(files[j]);
                }
                files.Sort();
                chuckFilesByCategory.Add(i, files);
            }
        }

        //categories + items public getters
        public string[] GetChuckCategories()
        {
            return chuckCategories;
        }
        public Dictionary<int, List<string>> GetChuckItemsByCategory()
        {
            return chuckFilesByCategory;
        }
        public Dictionary<int, List<string>> GetControlItemsByCategory()
        {
            return controlByCategory;
        }
        public Dictionary<int, List<string>> GetGraphicsItemsByCategory()
        {
            return graphicsByCategory;
        }

        /// <summary>
        /// Get index of Chuck file by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private int GetChuckFileIndex(string name)
        {
            int index = -1;

            for (int i = 0; i < chuckFiles.Count; i++)
            {
                if (name == chuckFiles[i])
                {
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// Check whether Voice Unit requires file loading
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CheckVoiceFileRequirement(string chuckName)
        {
            bool result = false;

            string chuckCode = File.ReadAllText(RayToneController.CHUCK_DIR + chuckName + ".ck");
            string arg = RayToneUtil.FindStringArg(chuckCode, "RAYTONE_LOADFILE(", ");");

            if (arg == "true")
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Get Graphics Unit required file extensions
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public string[] GetGraphicsRequiredExtensions(string className)
        {
            string[] extensions = { };

            // TODO: very redundant with SpawnGraphics
            for (int i = 0; i < Graphics_PF.Length; i++)
            {
                if (Graphics_PF[i] != null && Graphics_PF[i].GetType().ToString() == className)
                {
                    extensions = Graphics_PF[i].extensions;
                }
            }

            return extensions;
        }

        /// <summary>
        /// Assign outlet callback from ChucK
        /// </summary>
        private void AssignOutletCallback()
        {
            myChuck.GetFloatArray("outletArr_chuck", outletCallback);
        }
        /// <summary>
        /// Get outlet array from ChucK
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="num"></param>
#if UNITY_IOS && !UNITY_EDITOR
[AOT.MonoPInvokeCallback(typeof(Chuck.FloatArrayCallback))]
#endif
        private static void GetOutlet(CK_FLOAT[] vals, CK_UINT num)
        {
            outletVals = vals;
        }

        /// <summary>
        /// Assign shred ID callback
        /// </summary>
        private void AssignShredIDCallback()
        {
            myChuck.GetIntArray("shredId_chuck", shredIdCallback);
        }
        /// <summary>
        /// Get shred ID from Chuck
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="i"></param>
#if UNITY_IOS && !UNITY_EDITOR
[AOT.MonoPInvokeCallback(typeof(Chuck.IntArrayCallback))]
#endif
        private static void GetShredID(CK_INT[] ids, CK_UINT num)
        {
            shredId = ids;
        }

        /// <summary>
        /// Remove shred from ChucK
        /// </summary>
        /// <param name="id"></param>
        public void RemoveShred(int id)
        {
            chuckMainInstance.RunCode($@"Machine.remove({id}); Machine.remove(me.id());");
        }

        /// <summary>
        /// Remove shred from ChucK using voice ID
        /// </summary>
        /// <param name="voiceId"></param>
        public void RemoveShredWithVoiceID(int voiceId)
        {
            RemoveShred((int)shredId[voiceId]);
        }

        /// <summary>
        /// Set global volume
        /// </summary>
        /// <param name="val"></param>
        public static void SetGlobalVolume(float val)
        {
            volumeGlobal = Mathf.Clamp(val, 0f, 1f);
        }
        /// <summary>
        /// Get global volume
        /// </summary>
        /// <returns></returns>
        public static float GetGlobalVolume()
        {
            return volumeGlobal;
        }

        /// <summary>
        /// Spawn control unit
        /// </summary>
        /// <param name="className"></param>
        /// <param name="pos"></param>
        /// <param name="overwriteId"></param>
        /// <returns></returns>
        public ControlUnit SpawnControl(string className, Vector3 pos = default, int overwriteId = -1)
        {
            ControlUnit control = null;
            int controlPrefabIndex = -1;

            // Look for Prefab reference whose script matches that of target
            for (int i = 0; i < Control_PF.Length; i++)
            {
                if (Control_PF[i] != null && Control_PF[i].GetType().ToString() == className)
                {
                    controlPrefabIndex = i;
                    break;
                }
            }
            if(controlPrefabIndex == -1)
            {
                return null;
            }

            // Check if only a single instance is allowed
            if (Control_PF[controlPrefabIndex].forceSingleInstance)
            {
                foreach (ControlUnit existingControlUnit in controls.Values)
                {
                    if (existingControlUnit.GetType().ToString() == className)
                    {
                        Console.Log("Error: Only a single instance of " + existingControlUnit.GetType().Name + " unit is allowed.", true);
                        return null;
                    }
                }
            }

            int id = overwriteId;
            if(overwriteId == -1)
            {
                // Check for next available controls index
                for (id = 0; id < controlsAvailability.Length; id++)
                {
                    if (!controlsAvailability[id])
                    {
                        break;
                    }
                }
            }

            // Spawn - assign type and ID
            control = Instantiate(Control_PF[controlPrefabIndex], pos, Quaternion.identity);
            control.SetID(id);
            control.SetUnitType(UnitType.Control);

            // Highlight
            raytoneController.SelectUnit(control);

            // Store
            controls.Add(id, control);
            units.Add((UnitType.Control, id), control);
            controlsAvailability[id] = true;

            return control;
        }

        /// <summary>
        /// Spawn Voice Unit
        /// </summary>
        /// <param name="chuckName"></param>
        /// <param name="file"></param>
        /// <param name="pos"></param>
        /// <param name="overwriteId"></param>
        /// <returns></returns>
        public VoiceUnit SpawnVoice(string chuckName, string file = "", Vector3 pos = default, int overwriteId = -1)
        {
            VoiceUnit voice = null;
            int chuckIndex = GetChuckFileIndex(chuckName);

            // Check if chuck file exists
            if (chuckIndex == -1)
            {
                Console.Log("Voice Unit: Failed to load " + chuckName + ".", true);
                return voice;
            }

            int i = overwriteId;
            if (overwriteId == -1)
            {
                // Check for next available voices index
                for (i = 0; i < voicesAvailability.Length; i++)
                {
                    if (!voicesAvailability[i])
                    {
                        break;
                    }
                }
            }

            // Spawn - assign type and ID
            // TODO - Create Init() and make these variables private in VoiceUnit?
            voice = Instantiate(Voice_PF, pos, Quaternion.identity);
            voice.SetID(i);
            voice.SetUnitType(UnitType.Voice);
            voice.valArrFirstIndex = i * INLET_NUM_MAX;
            voice.chuckName = chuckName;
            voice.SetFilePath(file);
            voice.unitController = this;

            // Highlight
            raytoneController.SelectUnit(voice);

            // Store
            voices.Add(i, voice);
            units.Add((UnitType.Voice, i), voice);
            voicesAvailability[i] = true;

            return voice;
        }

        /// <summary>
        /// Spawn graphics unit
        /// </summary>
        /// <param name="className"></param>
        /// <param name="filePath"></param>
        /// <param name="pos"></param>
        /// <param name="overwriteId"></param>
        /// <returns></returns>
        public GraphicsUnit SpawnGraphics(string className, string filePath, Vector3 pos = default, int overwriteId = -1)
        {
            GraphicsUnit graphic = null;
            int graphicsPrefabIndex = -1;

            // Look for Prefab reference whose script matches that of target
            for (int i = 0; i < Graphics_PF.Length; i++)
            {
                if (Graphics_PF[i] != null && Graphics_PF[i].GetType().ToString() == className)
                {
                    graphicsPrefabIndex = i;
                }
            }
            if(graphicsPrefabIndex == -1)
            {
                return null;
            }

            // Check if only a single instance is allowed
            if (Graphics_PF[graphicsPrefabIndex].forceSingleInstance)
            {
                foreach (GraphicsUnit existingGraphicsUnit in graphics.Values)
                {
                    if (existingGraphicsUnit.GetType().ToString() == className)
                    {
                        Console.Log("Error: Only a single instance of " + existingGraphicsUnit.GetType().Name + " unit is allowed.", true);
                        return null;
                    }
                } 
            }

            int id = overwriteId;
            if(overwriteId == -1)
            {
                // Check for next available controls index
                for (id = 0; id < graphicsAvailability.Length; id++)
                {
                    if (!graphicsAvailability[id])
                    {
                        break;
                    }
                }
            }

            // Spawn - assign type and ID
            graphic = Instantiate(Graphics_PF[graphicsPrefabIndex], pos, Quaternion.identity);
            graphic.SetID(id);
            graphic.SetUnitType(UnitType.Graphics);
            graphic.SetFilePath(filePath);

            // Highlight
            raytoneController.SelectUnit(graphic);

            // Store
            graphics.Add(id, graphic);
            units.Add((UnitType.Graphics, id), graphic);
            graphicsAvailability[id] = true;

            return graphic;
        }

        /// <summary>
        /// Destroy control unit
        /// </summary>
        /// <param name="control"></param>
        public void DestroyControl(ControlUnit control)
        {
            if (control != null)
            {
                // Disconnect all inlets
                if (control.inlets != null)
                {
                    for (int i = 0; i < control.inlets.Length; i++)
                    {
                        if (control.inlets[i] != null && control.inlets[i].IsConnected())
                        {
                            control.inlets[i].connectedOutlet.Disconnect(control.inlets[i]);
                        }
                    }
                }

                // Disconnenct all connections from this Control Unit
                if (control.outlet != null) control.outlet.DisconnectAll();

                // Remove from List
                controls.Remove(control.GetID());
                units.Remove((UnitType.Control, control.GetID()));

                // Update availability array
                controlsAvailability[control.GetID()] = false;

                // Destroy
                if (control.myChuck != null)
                {
                    control.myChuck.SetRunning(false);
                    Destroy(control.myChuck);
                    control.myChuck = null;
                }
                Destroy(control.gameObject);
                control = null;
            }
        }

        /// <summary>
        /// Destroy voice unit
        /// </summary>
        /// <param name="voice"></param>
        public void DestroyVoice(VoiceUnit voice)
        {
            if (voice != null)
            {
                // Disconnect inlet connections
                for (int i = 0; i < voice.inletsNum; i++)
                {
                    if (voice.inlets[i].IsConnected())
                    {
                        voice.inlets[i].connectedOutlet.Disconnect(voice.inlets[i]);
                    }
                }
                // Disconnenct all connections from this Voice Unit
                if (voice.output != null) voice.output.DisconnectAll();
                if (voice.outlet != null) voice.outlet.DisconnectAll();

                // Disconnect input connections
                for (int j = 0; j < voice.inputsNum; j++)
                {
                    if(voice.inputs[j].IsConnected())
                    {
                        voice.inputs[j].connectedOutput.Disconnect(voice.inputs[j]);
                    }
                }

                // Remove voice shred from Chuck
                RemoveShred((int)shredId[voice.GetID()]);

                // Remove voice from List
                voices.Remove(voice.GetID());
                units.Remove((UnitType.Voice, voice.GetID()));

                // Update availability array
                voicesAvailability[voice.GetID()] = false;

                // Destroy
                if (voice.myChuck != null)
                {
                    voice.myChuck.SetRunning(false);
                    Destroy(voice.myChuck);
                    voice.myChuck = null;
                }
                Destroy(voice.gameObject);
                voice = null;
            }
        }

        /// <summary>
        /// Destroy graphics unit
        /// </summary>
        /// <param name="control"></param>
        public void DestroyGraphics(GraphicsUnit graphic)
        {
            if (graphic != null)
            {
                // Disconnect all inlets
                if (graphic.inlets != null)
                {
                    for (int i = 0; i < graphic.inlets.Length; i++)
                    {
                        if (graphic.inlets[i] != null && graphic.inlets[i].IsConnected())
                        {
                            graphic.inlets[i].connectedOutlet.Disconnect(graphic.inlets[i]);
                        }
                    }
                }

                // Disconnenct all connections from this Voice Unit
                if (graphic.outlet != null) graphic.outlet.DisconnectAll();

                // Remove from List
                graphics.Remove(graphic.GetID());
                units.Remove((UnitType.Graphics, graphic.GetID()));

                // Update availability array
                graphicsAvailability[graphic.GetID()] = false;

                // Destroy
                if(graphic.myChuck)
                {
                    graphic.myChuck.SetRunning(false);
                    Destroy(graphic.myChuck);
                    graphic.myChuck = null;
                }
                Destroy(graphic.gameObject);
                graphic = null;
            }
        }

        /// <summary>
        /// Get Full Unit Properties
        /// (Unit.GetUnitProperties only returns params known to a child Unit. This function fills the rest, such as class_index)
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public UnitProperties GetFullUnitProperties(Unit unit)
        {
            UnitProperties up = new();

            if (unit != null)
            {
                up = unit.GetUnitProperties();
                up.id = unit.GetID();
                up.location = unit.transform.position;

                switch (unit.GetUnitType())
                {
                    // Control
                    case UnitType.Control:
                        up.type = UnitType.Control;

                        if (up.metaString == null)
                        {
                            up.metaString = new Dictionary<string, string>();
                        }
                        up.metaString.Add("className", unit.GetType().ToString());
                        break;
                    // Voice
                    case UnitType.Voice:
                        up.type = UnitType.Voice;
                        break;
                    // Graphics
                    case UnitType.Graphics:
                        up.type = UnitType.Graphics;

                        if (up.metaString == null)
                        {
                            up.metaString = new Dictionary<string, string>();
                        }
                        up.metaString.Add("className", unit.GetType().ToString());
                        break;
                }
            }
            return up;
        }

        /// <summary>
        /// Get project data
        /// </summary>
        /// <param name="selectedUnits"></param>
        /// <param name="searchRelUnits"></param>
        /// <returns></returns>
        public RayToneProject GetProjectData(List<Unit> selectedUnits, bool searchRelUnits = false)
        {
            // Initialize Struct
            RayToneProject project = new();
            List<Unit> unitsTemp = new();
            List<UnitProperties> unit_properties = new();

            // Copy lists
            foreach(Unit unit in selectedUnits)
            {
                if (unit != null)
                {
                    unitsTemp.Add(unit);
                }
            }

            // Get Unit Properties
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                if (selectedUnits[i] != null)
                {
                    unit_properties.Add(GetFullUnitProperties(selectedUnits[i]));
                }
            }

            // Look for "RelUnits" - defined as Units that were not explicitly selected by user but connected to selected Units
            if(searchRelUnits)
            {
                Unit connectedUnit;
                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    // inlet
                    if(selectedUnits[i].inlets != null)
                    {
                        for (int j = 0; j < selectedUnits[i].inlets.Length; j++)
                        {
                            connectedUnit = selectedUnits[i].inlets[j].connectedUnit;
                            if (connectedUnit != null && !unitsTemp.Contains(connectedUnit))
                            {
                                UnitProperties up = new();
                                up.type = connectedUnit.GetUnitType();
                                up.id = connectedUnit.GetID();
                                up.relUnit = true;
                                unit_properties.Add(up);

                                unitsTemp.Add(connectedUnit);
                            }
                        }
                    }
                    // input
                    if(selectedUnits[i].inputs != null)
                    {
                        for (int j = 0; j < selectedUnits[i].inputs.Length; j++)
                        {
                            connectedUnit = selectedUnits[i].inputs[j].connectedVoice;
                            if (connectedUnit != null && !unitsTemp.Contains(connectedUnit))
                            {
                                UnitProperties up = new();
                                up.type = connectedUnit.GetUnitType();
                                up.id = connectedUnit.GetID();
                                up.relUnit = true;
                                unit_properties.Add(up);

                                unitsTemp.Add(connectedUnit);
                            }
                        }
                    }
                    
                    // outlet
                    if(selectedUnits[i].outlet != null)
                    {
                        for (int j = 0; j < selectedUnits[i].outlet.connectedUnits.Count; j++)
                        {
                            connectedUnit = selectedUnits[i].outlet.connectedUnits[j];
                            if (connectedUnit != null && !unitsTemp.Contains(connectedUnit))
                            {
                                UnitProperties up = new();
                                up.type = connectedUnit.GetUnitType();
                                up.id = connectedUnit.GetID();
                                up.relUnit = true;
                                unit_properties.Add(up);

                                unitsTemp.Add(connectedUnit);
                            }
                        }
                    }
                    // output
                    if(selectedUnits[i].output != null)
                    {
                        for (int j = 0; j < selectedUnits[i].output.connectedUnits.Count; j++)
                        {
                            connectedUnit = selectedUnits[i].output.connectedUnits[j];
                            if (connectedUnit != null && !unitsTemp.Contains(connectedUnit))
                            {
                                UnitProperties up = new();
                                up.type = connectedUnit.GetUnitType();
                                up.id = connectedUnit.GetID();
                                up.relUnit = true;
                                unit_properties.Add(up);

                                unitsTemp.Add(connectedUnit);
                            }
                        }
                    }
                }
            }

            // Get Input connection data
            for (int i = 0; i < unitsTemp.Count; i++)
            {
                if(unitsTemp[i].inputs != null)
                {
                    int inputsLength = unitsTemp[i].inputs.Length;
                    unit_properties[i].inputs = new int[inputsLength];

                    for (int j = 0; j < inputsLength; j++)
                    {
                        for (int k = 0; k < unitsTemp.Count; k++)
                        {
                            unit_properties[i].inputs[j] = -1;  //null int

                            if (unitsTemp[i].inputs[j].IsConnected() && unitsTemp[i].inputs[j].connectedOutput == unitsTemp[k].output)
                            {
                                unit_properties[i].inputs[j] = k;
                                break;
                            }
                        }
                    }
                }

                // Get Inlet connection
                if (unitsTemp[i].inlets != null)
                {
                    int inletsLength = unitsTemp[i].inlets.Length;
                    unit_properties[i].inlets = new int[inletsLength];

                    for (int j = 0; j < inletsLength; j++)
                    {
                        for (int k = 0; k < unitsTemp.Count; k++)
                        {
                            unit_properties[i].inlets[j] = -1;  //null int

                            if (unitsTemp[i].inlets[j].IsConnected() && unitsTemp[i].inlets[j].connectedOutlet == unitsTemp[k].outlet)
                            {
                                unit_properties[i].inlets[j] = k;
                                break;
                            }
                        }
                    }
                }
            }
            project.unit_properties = unit_properties.ToArray();
            return project;
        }

        /// <summary>
        /// Spawn Unit From Struct
        /// </summary>
        /// <param name="up"></param>
        /// <param name="offset"></param>
        /// <param name="projectDir"></param>
        /// <returns></returns>
        private Unit SpawnUnitFromStruct(UnitProperties up, Vector3 offset = default, string projectDir = "", bool overwriteID = false)
        {
            Unit newunit = null;

            string dir = "";
            // does not prepend a directory path to voice/graphics_properties.file unless project.directory is defined.
            if (!string.IsNullOrEmpty(projectDir))
            {
                dir = projectDir + "/Assets/";
                dir = dir.Replace('\\', '/');
            }

            int idTemp = -1;
            if (overwriteID)
            {
                idTemp = up.id;
            }
            switch (up.type)
            {
                // control
                case UnitType.Control:
                    newunit = SpawnControl(up.metaString["className"], up.location + offset, idTemp);
                    break;
                // voice
                case UnitType.Voice:
                    if (string.IsNullOrEmpty((string)up.metaString["file"]))
                    {
                        dir = "";
                    }
                    newunit = SpawnVoice((string)up.metaString["chuckName"], dir + (string)up.metaString["file"], up.location + offset, idTemp);
                    break;
                // graphics
                case UnitType.Graphics:
                    if (string.IsNullOrEmpty((string)up.metaString["file"]))
                    {
                        dir = "";
                    }
                    newunit = SpawnGraphics(up.metaString["className"], dir + (string)up.metaString["file"], up.location + offset, idTemp);
                    break;
            }

            if (newunit != null)
            {
                newunit.ApplyUnitProperties(up);
            }

            return newunit;
        }

        /// <summary>
        /// Coroutine - connect units after spawning from struct
        /// </summary>
        /// <param name="project"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        IEnumerator ConnectUnitsFromStruct(RayToneProject project, Unit[] units)
        {
            yield return new WaitForSeconds(0.0f);

            // Conenct Units
            for (int i = 0; i < units.Length; i++)
            {
                if(project.unit_properties[i].inputs != null)
                {
                    // inputs
                    for (int j = 0; j < project.unit_properties[i].inputs.Length; j++)
                    {
                        int index = project.unit_properties[i].inputs[j];

                        // Check for valid connection, connected unit, connected output, current unit?
                        if (index != -1 && units[index] != null && units[index].output != null && units[i] != null)
                        {
                            units[index].output.Connect(units[i].inputs[j], RayToneController.SpawnCable(units[index].output.transform));
                        }
                    }
                }
                
                if(project.unit_properties[i].inlets != null)
                {
                    // inlets
                    for (int j = 0; j < project.unit_properties[i].inlets.Length; j++)
                    {
                        int index = project.unit_properties[i].inlets[j];

                        // Check for valid connection, connected unit, connected outlet, current unit?
                        if (index != -1 && units[index] != null && units[index].outlet != null && units[i] != null)
                        {
                            units[index].outlet.Connect(units[i].inlets[j], RayToneController.SpawnCable(units[index].outlet.transform));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// AddProject from struct
        /// </summary>
        /// <param name="project"></param>
        /// <param name="offset"></param>
        public Unit[] AddProject(RayToneProject project, Vector3 offset = default, bool overwriteID = false)
        {
            raytoneController.DeselectAllUnits();

            // Skip if units are empty
            if(project.unit_properties == null)
            {
                return null;
            }

            int unitsLength = project.unit_properties.Length;
            Unit[] unitsTemp = new Unit[unitsLength];

            // Spawn Units
            for (int i = 0; i < unitsLength; i++)
            {
                if(project.unit_properties[i].relUnit)
                {
                    unitsTemp[i] = units[(project.unit_properties[i].type, project.unit_properties[i].id)];
                }
                else
                {
                    unitsTemp[i] = SpawnUnitFromStruct(project.unit_properties[i], offset, project.directory, overwriteID);
                } 
            }
            StartCoroutine(ConnectUnitsFromStruct(project, unitsTemp));

            return unitsTemp;
        }

        /// <summary>
        /// ResetStep
        /// </summary>
        public void ResetStep()
        {
            foreach (Unit unitTemp in units.Values)
            {
                unitTemp.OnResetStep();
            }
        }
    }
}
