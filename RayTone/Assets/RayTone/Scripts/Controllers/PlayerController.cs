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
//using UnityEngine.InputSystem;

namespace RayTone
{
    public class PlayerController : Singleton<PlayerController>
    {
        // public GameObject reference
        [SerializeField] private HoverText HoverText_PF;

        // controller references
        private RayToneController raytoneController;
        private UnitController unitController;
        private MenuController menuController;
        private CameraController cameraController;

        // private variables
        private GameObject goClicked;
        private GameObject goReleased;
        private GameObject inletClicked;  // special case since Inlet is not a Unit. Used for double click operation.
        private GameObject inputClicked;
        private HoverText hovertext;
        private Cable cable;
        private bool drag, selectbox;
        private bool tempcable;
        private static bool inputEnabled = true;

        private float time = -100f;  // negative value to prevent triggering double click within the first 500ms

        /////
        //AWAKE
        protected override void Awake()
        {
            base.Awake();
        }

        /////
        //Start
        private void Start()
        {
            // Get controller references
            raytoneController = RayToneController.Instance;
            unitController = UnitController.Instance;
            menuController = MenuController.Instance;
            cameraController = CameraController.Instance;
        }

        /// <summary>
        /// Enable keyboard input inside PlayerController
        /// </summary>
        public void EnableInput()
        {
            inputEnabled = true;
        }
        /// <summary>
        /// Disable keyboard input inside PlayerController
        /// </summary>
        public void DisableInput()
        {
            inputEnabled = false;
        }
        /// <summary>
        /// Get keyboard input status
        /// </summary>
        /// <returns></returns>
        public bool IsInputEnabled()
        {
            return inputEnabled;
        }

        /// <summary>
        /// Get mouse position using ray
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMousePosition()
        {
            Vector3 pos = default;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layerMask = 1 << LayerMask.NameToLayer("Collider");

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
            {
                if (hit.transform != null)
                {
                    pos = hit.point;
                }
            }

            return pos;
        }

        /// <summary>
        /// Raycast
        /// </summary>
        /// <returns></returns>
        private GameObject Raycast()
        {
            GameObject go = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layerMask = LayerMask.GetMask("RayToneMain", "RayToneUI");

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
            {
                if (hit.transform != null)
                {
                    go = hit.transform.gameObject;
                }
            }

            return go;
        }

        /// <summary>
        /// Attempt Select Unit   //TODO(Should it take GameObject as an argument rather than use global var?)
        /// </summary>
        /// <param name="multiselection"></param>
        void OnClick(bool multiselection)
        {
            if (goClicked != null)
            {
                bool isUnit = goClicked.TryGetComponent<Unit>(out Unit clickedUnit);

                if (multiselection)
                {
                    if (isUnit)
                    {
                        if (clickedUnit.GetSelectionStatus())
                        {
                            raytoneController.DeselectUnit(clickedUnit);
                            drag = false;
                        }
                        else
                        {
                            raytoneController.SelectUnit(clickedUnit);
                            raytoneController.SetDragOffset(GetMousePosition());
                            drag = true;
                        }
                    }
                }
                else
                {
                    // Double Click
                    // inlet
                    if (Time.time - time < 0.5f && inletClicked == goClicked)
                    {
                        AttemptDisconnect(goClicked.GetComponent<InletSocket>());
                        return;
                    }
                    // input
                    if (Time.time - time < 0.5f && inputClicked == goClicked)
                    {
                        AttemptDisconnectSignal(goClicked.GetComponent<InputSocket>());
                        return;
                    }
                    // unit
                    if (Time.time - time < 0.5f && isUnit && clickedUnit.GetSelectionStatus())
                    {
                        raytoneController.EnterEdit();
                        return;
                    }

                    // Clicked on Unit
                    else if (isUnit)
                    {
                        if (!clickedUnit.GetSelectionStatus())
                        {
                            raytoneController.DeselectAllUnits();
                            raytoneController.SelectUnit(clickedUnit);
                        }
                        time = Time.time;  // Save time for double click operation
                        raytoneController.SetDragOffset(GetMousePosition());

                        drag = true;
                    }
                    // Clicked on Inlet (special case since Inlet is not a Unit)
                    else if (goClicked.TryGetComponent<InletSocket>(out InletSocket clickedInlet))
                    {
                        inletClicked = clickedInlet.gameObject;  // weird naming...
                        time = Time.time;  // Save time for double click operation
                        return;
                    }
                    // Clicked on Input (special case since Input is not a Unit)
                    else if (goClicked.TryGetComponent<InputSocket>(out InputSocket clickedInput))
                    {
                        inputClicked = clickedInput.gameObject;  //weird naming...
                        time = Time.time;  // Save time for double click operation
                        return;
                    }
                    // Clicked on Menu Softkeys
                    else if (goClicked.TryGetComponent<Menu_Softkeys1>(out Menu_Softkeys1 clickedMenu))
                    {
                        return;
                    }
                    // Clicked on something else
                    else
                    {
                        raytoneController.DeselectAllUnits();
                    }
                }
            }
            // Clicked on nothing
            else
            {
                if (!multiselection)
                {
                    raytoneController.DeselectAllUnits();
                }
                raytoneController.SetSelectBoxAnchor(GetMousePosition());
                selectbox = true;
            }
        }

        /// <summary>
        /// Attempt Disconnect
        /// </summary>
        /// <param name="inlet"></param>
        void AttemptDisconnect(InletSocket inlet)
        {
            // Check if Inlet is currently conencted
            if (inlet.IsConnected())
            {
                // Disconnect!
                raytoneController.StoreDisconnectCommand(inlet.connectedOutlet.parentUnit.GetTypeID(), inlet.parentUnit.GetTypeID(), inlet.inletIndex);
                inlet.connectedOutlet.Disconnect(inlet);
            }
        }

        /// <summary>
        /// Attempt Disconnect signal sockets
        /// </summary>
        /// <param name="inlet"></param>
        void AttemptDisconnectSignal(InputSocket input)
        {
            // Check if Input is currently conencted
            if (input.IsConnected())
            {
                // Disconnect!
                raytoneController.StoreDisconnectSignalCommand(input.connectedOutput.parentVoice.GetTypeID(), input.parentVoice.GetTypeID(), input.inputIndex);
                input.connectedOutput.Disconnect(input);
            }
        }

        /// <summary>
        /// Attempt Connect
        /// </summary>
        void AttemptConnect()
        {
            // Check if clicked object was Outlet
            if (goClicked != null && goClicked.TryGetComponent<OutletSocket>(out OutletSocket clickedOutlet))
            {
                // Check if released object is Inlet
                if (goReleased != null && goReleased.TryGetComponent<InletSocket>(out InletSocket releasedInlet))
                {
                    // Remove any existing connection from this inlet
                    AttemptDisconnect(releasedInlet);

                    // Connect!
                    if (clickedOutlet.Connect(releasedInlet, cable))
                    {
                        tempcable = false;
                        cable = null;
                        raytoneController.StoreConnectCommand(clickedOutlet.parentUnit.GetTypeID(), releasedInlet.parentUnit.GetTypeID(), releasedInlet.inletIndex);
                    }
                    else
                    {
                        DestroyTempCable();
                    }
                }
                else
                {
                    DestroyTempCable();
                }
            }
        }

        /// <summary>
        /// Attempt Connect
        /// </summary>
        void AttemptConnectSignal()
        {
            // Check if clicked object was Output
            if (goClicked != null && goClicked.TryGetComponent<OutputSocket>(out OutputSocket clickedOutput))
            {
                // Check if released object is Input
                if (goReleased != null && goReleased.TryGetComponent<InputSocket>(out InputSocket releasedInput))
                {
                    // Remove any existing connection from this input
                    AttemptDisconnectSignal(releasedInput);

                    // Connect!
                    if (clickedOutput.Connect(releasedInput, cable))
                    {
                        tempcable = false;
                        cable = null;

                        raytoneController.StoreConnectSignalCommand(clickedOutput.parentVoice.GetTypeID(), releasedInput.parentVoice.GetTypeID(), releasedInput.inputIndex);
                    }
                    else
                    {
                        DestroyTempCable();
                    }
                }
                else
                {
                    DestroyTempCable();
                }
            }
        }

        /// <summary>
        /// Spawn temp cable
        /// </summary>
        void AttemptSpawnTempCable()
        {
            if (goClicked != null)
            {
                if (goClicked.TryGetComponent<OutletSocket>(out OutletSocket outlet) || goClicked.TryGetComponent<OutputSocket>(out OutputSocket output))
                {
                    cable = RayToneController.SpawnCable(goClicked.transform, GetMousePosition());
                    tempcable = true;
                }
            }
        }

        /// <summary>
        /// Destroy temp cable
        /// </summary>
        void DestroyTempCable()
        {
            if (cable != null)
            {
                Destroy(cable.gameObject);
                cable = null;
            }
            tempcable = false;
        }

        /// <summary>
        /// AttemptSpawnHoverText
        /// </summary>
        void AttemptSpawnHoverText()
        {
            GameObject hovered = Raycast();
            if (hovered != null && hovered.TryGetComponent<Socket>(out Socket hoveredSocket))
            {
                if (hovertext == null)
                {
                    hovertext = Instantiate(HoverText_PF);
                    hovertext.transform.parent = hovered.transform;
                    hovertext.transform.localPosition = hoveredSocket.GetOrientation();
                    hovertext.SetText(hoveredSocket.GetDescription());
                }
            }
            else
            {
                if (hovertext != null)
                {
                    Destroy(hovertext.gameObject);
                    hovertext = null;
                }
            }
        }


        // EVENTS
        // S
        public void OnS()
        {
            // Exit edit if in edit
            if (raytoneController.GetEditStatus())
            {
                raytoneController.ExitEdit();
            }
            menuController.ToggleSettingsMenu();
        }

        // C
        public void OnC()
        {
            // Exit edit if in edit
            if (raytoneController.GetEditStatus())
            {
                raytoneController.ExitEdit();
            }
            menuController.ToggleControlMenu();
        }

        // V
        public void OnV()
        {
            // Exit edit if in edit
            if (raytoneController.GetEditStatus())
            {
                raytoneController.ExitEdit();
            }
            menuController.ToggleVoiceMenu();
        }

        // G
        public void OnG()
        {
            // Exit edit if in edit
            if (raytoneController.GetEditStatus())
            {
                raytoneController.ExitEdit();
            }
            menuController.ToggleGraphicsMenu();
        }

        // E
        public void OnE()
        {
            // Enter/Exit Edit Mode if not using one of the global menu panels
            if (menuController.GetMenuStatus() == MenuStatus.None || menuController.GetMenuStatus() == MenuStatus.UnitMenu)
            {
                if (!raytoneController.GetEditStatus())
                {
                    menuController.CloseMenu();
                    raytoneController.EnterEdit();
                    drag = false;
                }
                else
                {
                    raytoneController.ExitEdit();
                }
            }
        }

        // M
        public void OnM()
        {
            raytoneController.ToggleUnitMenu();
        }

        // Tilde
        public void OnTilde()
        {
            Console.ToggleConsoleVisibility();
        }

        // Up
        public void OnUp()
        {
            // Increment Sequencer Step value
            if (raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None && raytoneController.GetFirstSelectedUnit().TryGetComponent<Sequencer>(out Sequencer sequencerRef))
            {
                int delta = 1;
                if (Input.GetKey("left shift") || Input.GetKey("right shift"))
                {
                    delta = 10;
                }
                if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl") || Input.GetKey("left cmd") || Input.GetKey("right cmd"))
                {
                    sequencerRef.IncDecStepsVal(delta);
                }
                else
                {
                    sequencerRef.IncDecStepVal(delta);
                }
            }
        }

        // Down
        public void OnDown()
        {
            // Decrement Sequencer Step value
            if (raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None && raytoneController.GetFirstSelectedUnit().TryGetComponent<Sequencer>(out Sequencer sequencerRef))
            {
                int delta = -1;
                if (Input.GetKey("left shift") || Input.GetKey("right shift"))
                {
                    delta = -10;
                }
                if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl") || Input.GetKey("left cmd") || Input.GetKey("right cmd"))
                {
                    sequencerRef.IncDecStepsVal(delta);
                }
                else
                {
                    sequencerRef.IncDecStepVal(delta);
                }
            }
        }

        // Left
        public void OnLeft()
        {
            // Move to previous Sequencer Step
            if (raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None && raytoneController.GetFirstSelectedUnit().TryGetComponent<Sequencer>(out Sequencer sequencerRef))
            {
                sequencerRef.SelectPrevStep();
            }
        }

        // Right
        public void OnRight()
        {
            // Move to next Sequencer Step
            if (raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None && raytoneController.GetFirstSelectedUnit().TryGetComponent<Sequencer>(out Sequencer sequencerRef))
            {
                sequencerRef.SelectNextStep();
            }
        }

        // Delete
        public void OnDelete()
        {
            if (!raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None)
            {
                // Check if clicked object is Inlet
                if (goClicked != null && goClicked.TryGetComponent<InletSocket>(out InletSocket clickedInlet))
                {
                    // Attempt Disconnect
                    AttemptDisconnect(clickedInlet);
                }
                // Check if clicked object is Input
                if (goClicked != null && goClicked.TryGetComponent<InputSocket>(out InputSocket clickedInput))
                {
                    // Attempt Disconnect
                    AttemptDisconnectSignal(clickedInput);
                }

                // Delete Selected Units
                raytoneController.DestroySelectedUnits();
            }
        }

        // Escape
        public void OnEscape()
        {
            // Exit edit mode
            if (raytoneController.GetEditStatus())
            {
                raytoneController.ExitEdit();
            }

            // Deselect Units
            if (menuController.GetMenuStatus() == MenuStatus.None)
            {
                if (drag)
                {
                    raytoneController.EndDrag();
                }
                raytoneController.DeselectAllUnits();
            }
        }

        // Randomize (Ctrl+R - special case)
        public void OnRandomize()
        {
            // Randomize all Sequencer Step values
            if (raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None && raytoneController.GetFirstSelectedUnit().TryGetComponent<Sequencer>(out Sequencer sequencerRef))
            {
                sequencerRef.RandomizeStepsVal();
            }
        }

        // Duplicate (Ctrl+D - special case)
        public void OnDuplicate()
        {
            // Duplicate Unit
            if (!raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None)
            {
                raytoneController.DuplicateUnits();
            }
        }

        /////
        //UPDATE  TODO: Remove redundant code in this function
        void Update()
        {
            // No canvas operations if browser is open or user input is disabled
            if(menuController.GetBrowserStatus() || !IsInputEnabled()) return;

            // Space key -> Reset Step
            if (Input.GetKeyDown("space"))
            {
                unitController.ResetStep();
            }

            if (cameraController.GetVisibility())
            {
                // ToggleVoiceMenu
                if (Input.GetKeyDown("v"))
                {
                    if (!Input.GetKey("left ctrl") && !Input.GetKey("right ctrl") && !Input.GetKey("left cmd") && !Input.GetKey("right cmd"))
                    {
                        OnV();
                    }
                }

                // ToggleControlMenu
                if (Input.GetKeyDown("c"))
                {
                    if (!Input.GetKey("left ctrl") && !Input.GetKey("right ctrl") && !Input.GetKey("left cmd") && !Input.GetKey("right cmd"))
                    {
                        OnC();
                    }  
                }

                // ToggleGraphicsMenu
                if (Input.GetKeyDown("g"))
                {
                    OnG();
                }

                // Enter/Exit Edit
                if (Input.GetKeyDown("e"))
                {
                    OnE();
                }

                // Toggle Sequencer Menu
                if (Input.GetKeyDown("m"))
                {
                    OnM();
                }

                // Up
                if (Input.GetKeyDown("up"))
                {
                    OnUp();
                }
                // Down
                else if (Input.GetKeyDown("down"))
                {
                    OnDown();
                }
                // Left
                else if (Input.GetKeyDown("left"))
                {
                    OnLeft();
                }
                // Right
                if (Input.GetKeyDown("right"))
                {
                    OnRight();
                }
                // Tilde - console
                if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
                {
                    OnTilde();
                }
                // Escape
                if (Input.GetKeyDown("escape"))
                {
                    OnEscape();
                }

                // Canvas operation
                if (!raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None)
                {
                    // Spawn number
                    if (Input.GetKeyDown("n"))
                    {
                        raytoneController.DeselectAllUnits();
                        Vector3 offset = new Vector3(Random.Range(0f, 3f), 0f, Random.Range(-1f, 1f));
                        Unit unitTemp = unitController.SpawnControl("RayTone.Number", GetMousePosition() + offset);
                        raytoneController.StoreSpawnCommand(new Unit[] { unitTemp });
                    }

                    // Delete
                    if (Input.GetKeyDown("delete"))
                    {
                        OnDelete();
                    }
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    if (Input.GetKeyDown(KeyCode.Backspace))
                    {
                        OnDelete();
                    }
#endif
                }

                // CTRL
                if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl") || Input.GetKey("left cmd") || Input.GetKey("right cmd"))
                {
                    // Randomize
                    if (Input.GetKeyDown("r"))
                    {
                        OnRandomize();
                    }

                    // Canvas operation (Ctrl)
                    if(!raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None)
                    {
                        // Select All
                        if (Input.GetKeyDown("a"))
                        {
                            raytoneController.SelectAllUnits();
                        }

                        // Copy
                        if (Input.GetKeyDown("c"))
                        {
                            raytoneController.CopyUnits();
                        }
                        // Cut
                        if (Input.GetKeyDown("x"))
                        {
                            raytoneController.CopyUnits();
                            raytoneController.DestroySelectedUnits();
                        }
                        // Paste
                        if (Input.GetKeyDown("v"))
                        {
                            raytoneController.PasteUnits(true);
                        }
                        // Duplicate
                        if (Input.GetKeyDown("d"))
                        {
                            OnDuplicate();  //TODO: is the dedicated function too redundant?
                        }

                        // Undo
                        if (Input.GetKeyDown("z"))
                        {
                            raytoneController.Undo();
                        }
                        // Redo
                        if (Input.GetKeyDown("y"))
                        {
                            raytoneController.Redo();
                        }

                        // Save Project
                        if (Input.GetKeyDown("s"))
                        {
                            raytoneController.SaveProject();
                        }
                        // Load Project
                        if (Input.GetKeyDown("p") || Input.GetKeyDown("q"))
                        {
                            raytoneController.LoadProject();
                        }
                        // Load Library
                        if (Input.GetKeyDown("l"))
                        {
                            /*
                            menuController.CloseMenu();
                            unitController.LoadChuckFiles();
                            */
                        }
                    }
                }
            }
            else
            {
                // Exit "performance" view
                if (Input.GetKeyDown("escape"))
                {
                    Cursor.lockState = CursorLockMode.None;
                    cameraController.ToggleVisibility(true);
                }
            }

            // PERFORMANCE VISIBILITY TOGGLE
            if (Input.GetKeyDown("t"))
            {
                if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl") || Input.GetKey("left cmd") || Input.GetKey("right cmd"))
                {
                    if(!raytoneController.GetEditStatus())
                    {
                        cameraController.ToggleVisibility(!cameraController.GetVisibility());
                    }
                }
            }

            // MOUSE COMMANDS
            if (!raytoneController.GetEditStatus() && menuController.GetMenuStatus() == MenuStatus.None && cameraController.GetVisibility())
            {
                // Hover text
                AttemptSpawnHoverText();

                // Click
                if (Input.GetMouseButtonDown(0))
                {
                    goClicked = Raycast();
                    OnClick(Input.GetKey("left shift") || Input.GetKey("right shift") || Input.GetKey("left ctrl") || Input.GetKey("right ctrl") || Input.GetKey("left cmd") || Input.GetKey("right cmd"));
                    AttemptSpawnTempCable();
                }

                // Release
                if (Input.GetMouseButtonUp(0))
                {
                    goReleased = Raycast();
                    AttemptConnect();
                    AttemptConnectSignal();
                }

                // Drag
                if (!raytoneController.GetEditStatus())
                {
                    if (drag)
                    {
                        raytoneController.Drag(GetMousePosition());

                        if (Input.GetMouseButtonUp(0))
                        {
                            raytoneController.EndDrag();
                            drag = false;
                        }
                    }

                    if (selectbox)
                    {
                        raytoneController.UpdateSelectBox(GetMousePosition());

                        if (Input.GetMouseButtonUp(0))
                        {
                            raytoneController.EndSelectBox();
                            selectbox = false;
                        }
                    }
                }

                // Drag temp cable
                if (tempcable == true)
                {
                    cable.targetPosition = GetMousePosition();
                }
            }
        }
    }
}
