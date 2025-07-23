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
    public enum UnitType
    {
        Control,
        Voice,
        Graphics
    }

    [System.Serializable]
    public class UnitProperties
    {
        public UnitType type;
        [System.NonSerialized] public int id;
        [System.NonSerialized] public bool relUnit = false;
        public Vector3 location;
        public int[] inputs;
        public int[] inlets;
        public Dictionary<string, string> metaString;
        public Dictionary<string, int> metaInt;
        public Dictionary<string, float> metaFloat;

        // internal usage only
        public List<string> metaStringKeys;
        public List<string> metaStringVals;
        public List<string> metaIntKeys;
        public List<int> metaIntVals;
        public List<string> metaFloatKeys;
        public List<float> metaFloatVals;
    }

    public class Unit : Highlightable
    {
        [Header("Sockets")]
        // prefab references
        [SerializeField] private InletSocket Inlet_PF;
        [SerializeField] private OutletSocket Outlet_PF;

        // input & output references
        [System.NonSerialized] public InputSocket[] inputs;
        [System.NonSerialized] public OutputSocket output;

        // inlets array must be initialized by subclass
        public InletSocket[] inlets;
        // outlet must be initialized by subclass
        public OutletSocket outlet;

        [Header("Menu")]
        // whether the edit mode is allowed 
        public bool enableEdit = false;
        // whether to zoom in on EnterEdit
        public bool zoomOnEdit = true;
        // submenu Prehab
        public Menu_Unit UnitMenu_PF;
        // Whether to spawn submenu on EnterEdit
        public bool enterMenuOnEdit = true;

        // whether to force single instance
        [Header("Unit")]
        public bool forceSingleInstance = false;

        // reference to ChuckSubInstance and ChuckEventListener
        [System.NonSerialized] public ChuckSubInstance myChuck;
        protected ChuckEventListener myEventListener;

        // filepath for Graphics and Voice units
        private string filePath;

        // Unit type
        private UnitType type;
        // ID - not a global index - used separately for Control, Voice, and Graphics. To access a "global index", use units[type, id] in UnitController.
        private int id;
        // selection status
        private bool isSelected;
        
        // stored outlet value
        private float storedValue = 0;

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public virtual void ApplyUnitProperties(UnitProperties up)
        {
            // Override in each Unit subclass
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public virtual UnitProperties GetUnitProperties()
        {
            // Override in each Unit subclass
            UnitProperties up = new();
            return up;
        }

        /// <summary>
        /// Triggered on select
        /// </summary>
        public virtual void OnSelect()
        {
            // Override behavior OnSelect in each Unit subclass.
            Highlight();
        }
        /// <summary>
        /// Triggered on deselect
        /// </summary>
        public virtual void OnDeselect()
        {
            // Override behavior OnDeselect in each Unit subclass.
            Dehighlight();
        }

        /// <summary>
        /// Triggered on EnterEdit
        /// </summary>
        /// <returns></returns>
        public virtual void OnEnterEdit()
        {
            // Override behavior OnEnterEdit in each Unit subclass
        }
        /// <summary>
        /// Triggered on ExitEdit
        /// </summary>
        public virtual void OnExitEdit()
        {
            // Override behavior OnExitEdit in each Unit subclass
        }
        
        /// <summary>
        /// Triggered by EnterMenu
        /// </summary>
        public virtual void OnEnterMenu()
        {
            // Override behavior OnEnterMenu in each Unit subclass
        }
        /// <summary>
        /// Triggered by ExitMenu
        /// </summary>
        public virtual void OnExitMenu()
        {
            // Override behavior OnExitMenu in each Unit subclass
        }

        /// <summary>
        /// Triggered when clock steps are reset
        /// </summary>
        public virtual void OnResetStep()
        {
            // Override behavior OnResetStep in each Unit subclass
        }

        /// <summary>
        /// Set Unit type
        /// </summary>
        /// <param name="arg"></param>
        public void SetUnitType(UnitType arg)
        {
            type = arg;
        }
        /// <summary>
        /// Get Unit type
        /// </summary>
        /// <returns></returns>
        public UnitType GetUnitType()
        {
            return type;
        }
        /// <summary>
        /// Set ID
        /// </summary>
        /// <param name="arg"></param>
        public void SetID(int arg)
        {
            id = arg;
        }
        /// <summary>
        /// Get ID
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return id;
        }
        public (UnitType, int) GetTypeID()
        {
            return (GetUnitType(), GetID());
        }

        /// <summary>
        /// Selection status setter
        /// </summary>
        /// <returns></returns>
        public void SetSelectionStatus(bool arg)
        {
            isSelected = arg;
        }
        /// <summary>
        /// Selection status getter
        /// </summary>
        /// <returns></returns>
        public bool GetSelectionStatus()
        {
            return isSelected;
        }

        /// <summary>
        /// Set asset file path
        /// </summary>
        /// <param name="filePath_arg"></param>
        public void SetFilePath(string filePath_arg)
        {
            filePath = filePath_arg;
        }

        /// <summary>
        /// Get asset file path
        /// </summary>
        public string GetFilePath()
        {
            return filePath;
        }

        /// <summary>
        /// Reattach file path 
        /// </summary>
        public virtual void ReattachFilePath()
        {
            // Update behavior on reattaching FilePath in children units!
        }

        /// <summary>
        /// Start monitoring ChucK event "RAYTONE_TICK_CONTROL"
        /// </summary>
        protected void StartStepListener()
        {
            UnitController.OnStep += Step;
        }

        /// <summary>
        /// Stop monitoring ChucK event "RAYTONE_TICK_CONTROL"
        /// </summary>
        protected void StopStepListener()
        {
            UnitController.OnStep -= Step;
        }

        /// <summary>
        /// Start monitoring sub update
        /// </summary>
        protected void StartSubUpdateListener()
        {
            UnitController.OnSubUpdate += SubUpdate;
        }

        /// <summary>
        /// Stop monitoring sub update
        /// </summary>
        protected void StopSubUpdateListener()
        {
            UnitController.OnSubUpdate -= SubUpdate;
        }

        /// <summary>
        /// Move to the next step
        /// </summary>
        public virtual void Step()
        {
            // Implement behavior on Step for each Unit
        }

        /// <summary>
        /// On RayTone patch/canvas update
        /// </summary>
        public virtual void SubUpdate()
        {
            // Implement behavior on sub update for each Unit
        }

        /// <summary>
        /// Get inlet value
        /// </summary>
        /// <param name="inletIndex"></param>
        /// <returns></returns>
        protected float GetInletVal(int inletIndex)
        {
            float val = 0;

            if (inletIndex >= 0 && inletIndex < inlets.Length)
            {
                if (inlets != null && inlets[inletIndex] != null && inlets[inletIndex].connectedUnit != null)
                {
                    val = inlets[inletIndex].connectedUnit.UpdateOutput();
                }
            }

            return val;
        }

        /// <summary>
        /// Get inlet conenction status
        /// </summary>
        /// <param name="inletIndex"></param>
        /// <returns></returns>
        protected bool GetInletStatus(int inletIndex)
        {
            bool isConnected = false;

            if (inletIndex >= 0 && inletIndex < inlets.Length)
            {
                if (inlets != null && inlets[inletIndex] != null && inlets[inletIndex].IsConnected())
                {
                    isConnected = true;
                }
            }

            return isConnected;
        }

        /// <summary>
        /// Spawn inlet
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected InletSocket SpawnInlet(Vector3 position)
        {
            InletSocket newInlet = Instantiate(Inlet_PF);
            newInlet.transform.parent = this.transform;
            newInlet.transform.localPosition = position;

            return newInlet;
        }

        /// <summary>
        /// Spawn outlet
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected OutletSocket SpawnOutlet(Vector3 position)
        {
            OutletSocket newOutlet = Instantiate(Outlet_PF);
            newOutlet.transform.parent = this.transform;
            newOutlet.transform.localPosition = position;

            return newOutlet;
        }

        /// <summary>
        /// Store outlet value
        /// </summary>
        public void StoreValue(float val)
        {
            storedValue = val;
        }
        /// <summary>
        /// Get stored outlet value
        /// </summary>
        /// <returns></returns>
        public float GetStoredValue()
        {
            return storedValue;
        }

        /// <summary>
        /// Get chained output
        /// </summary>
        /// <returns></returns>
        public virtual float UpdateOutput()
        {
            // Implement recursive chained output structure for each Control Unit        
            return 0;
        }

        /// <summary>
        /// Get chained trigger
        /// </summary>
        /// <returns></returns>
        public virtual int UpdateTrigger()
        {
            int trigger = 0;

            if (GetStoredValue() == 0f)
            {
                return 0;
            }

            if (inlets != null)
            {
                for (int i = 0; i < inlets.Length; i++)
                {
                    if (inlets[i] != null && inlets[i].IsConnected())
                    {
                        if (inlets[i].connectedUnit.UpdateTrigger() > 0)
                        {
                            trigger = 1;
                        }
                    }
                }
            }

            return trigger;
        }

        /// <summary>
        /// Queue render frame
        /// </summary>
        /// <param name="inlet"></param>
        public virtual void QueueRenderFrame(InletSocket inlet)
        {
            // override in subclass
        }

        /// <summary>
        /// Send render frame request
        /// </summary>
        protected void NotifyQueueRenderFrame()
        {
            for (int i = 0; i < outlet.connectedUnits.Count; i++)
            {
                outlet.connectedUnits[i].QueueRenderFrame(outlet.connectedInlets[i]);
            }
        }
    }
}
