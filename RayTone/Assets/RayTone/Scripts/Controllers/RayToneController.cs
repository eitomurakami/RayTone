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
using System.IO;
using SimpleFileBrowser;
using System.Linq;

namespace RayTone
{
    [System.Serializable]
    public struct UserConfig
    {
        public bool firstTime;
        public float volume;
        public int bpm;
        public bool fullScreen;
        public float resolutionDivider;
        public bool postProcessing;
        public float localGain;

        public UserConfig(bool firstTime_arg, float volume_arg, int bpm_arg, bool fullScreen_arg, float resolutionDivider_arg, bool postProcessing_arg, float localGain_arg)
        {
            firstTime = firstTime_arg;
            volume = volume_arg;
            bpm = bpm_arg;
            fullScreen = fullScreen_arg;
            resolutionDivider = resolutionDivider_arg;
            postProcessing = postProcessing_arg;
            localGain = localGain_arg;
        }
    }

    public class RayToneController : Singleton<RayToneController>
    {
        // prefab references
        [SerializeField] private Cable Cable_PF;
        private static Cable Cable_PF_static;
        [SerializeField] private GameObject SelectBox;

        // controller references
        private UnitController unitController;
        private PlayerController playerController;
        private MenuController menuController;
        private CameraController cameraController;
        
        // private variables
        private List<Unit> unitsSelected = new();
        private RayToneProject copiedProject = new();
        private List<RayToneCommand> commands = new();
        private int commandIndex = -1;
        private bool edit = false;
        private List<Vector3> dragOffset = new();
        private Vector3 dragFrom = default;
        private Vector3 selectboxAnchor = default;
        private UserConfig config = new(true, 1.0f, 120, true, 2.0f, true, 0.25f);

        // constant paths
        public static string BASE_DIR, CHUCK_DIR, SOURCE_DIR;

        /////
        //AWAKE
        protected override void Awake()
        {
            base.Awake();
            Cable_PF_static = Cable_PF;

            // Define paths
            BASE_DIR = Application.persistentDataPath;
            CHUCK_DIR = BASE_DIR + "/ChucK/";
            SOURCE_DIR = BASE_DIR + "/src/";

            // Copy assets
            Console.Log("Data path: " + BASE_DIR + "\n");
            RayToneUtil.CopyDirectory(Application.streamingAssetsPath, BASE_DIR, true);
        }

        /////
        //START
        private void Start()
        {
            // Get controller references
            unitController = UnitController.Instance;
            playerController = PlayerController.Instance;
            menuController = MenuController.Instance;
            cameraController = CameraController.Instance;

            Invoke(nameof(ApplyUserConfig), 1f);
            InvokeRepeating(nameof(AutoSaveProject), 300f, 300f);
        }

        // COMMANDS
        private void AddCommand(RayToneCommand command)
        {
            // If in the middle of undo/redo, delete future commands
            if(commandIndex != commands.Count - 1)
            {
                commands.RemoveRange(commandIndex + 1, commands.Count - commandIndex - 1);
            }
            commands.Add(command);
            commandIndex = commands.Count - 1;
        }
        /// <summary>
        /// Undo a command
        /// </summary>
        public void Undo()
        {
            if(commandIndex == -1)
            {
                return;
            }
            commands[commandIndex].Undo();
            commandIndex--;
        }
        /// <summary>
        /// Redo a command
        /// </summary>
        public void Redo()
        {
            if(commandIndex == commands.Count() - 1)
            {
                return;
            }
            commandIndex++;
            commands[commandIndex].Redo();
        }
        
        /// <summary>
        /// Store Spawn command
        /// </summary>
        public void StoreSpawnCommand(Unit[] units)
        {
            //print("Spawn");
            StartCoroutine(ExecuteStoreSpawnCommand(units));
        }
        private IEnumerator ExecuteStoreSpawnCommand(Unit[] units)
        {
            yield return new WaitForSeconds(0.1f);

            if (units.Length > 0)
            {
                RayToneProject project = unitController.GetProjectData(units.ToList<Unit>());
                SpawnCommand spawnCommand = new();
                spawnCommand.Init(unitController);
                spawnCommand.Store(project);
                AddCommand(spawnCommand);
            }
        }
            
        /// <summary>
        /// Store Destroy command
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        public void StoreDestroyCommand(Unit[] units)
        {
            //print("Destroy");

            if (units.Length > 0)
            {
                RayToneProject project = unitController.GetProjectData(units.ToList<Unit>(), true);
                DestroyCommand destroyCommand = new();
                destroyCommand.Init(unitController);
                destroyCommand.Store(project);
                AddCommand(destroyCommand);
            }
        }
        /// <summary>
        /// Store Connect command
        /// </summary>
        public void StoreConnectCommand((UnitType, int) fromTypeID, (UnitType, int) toTypeID, int inletIndex)
        {
            //print("Connect");
            ConnectCommand conenctCommand = new();
            conenctCommand.Init(unitController);
            conenctCommand.Store(fromTypeID, toTypeID, inletIndex);
            AddCommand(conenctCommand);
        }
        /// <summary>
        /// Store Connect Signal command
        /// </summary>
        public void StoreConnectSignalCommand((UnitType, int) fromTypeID, (UnitType, int) toTypeID, int inputIndex)
        {
            //print("ConnectSignal");
            ConnectSignalCommand conenctSignalCommand = new();
            conenctSignalCommand.Init(unitController);
            conenctSignalCommand.Store(fromTypeID, toTypeID, inputIndex);
            AddCommand(conenctSignalCommand);
        }
        /// <summary>
        /// Store Disconenct command
        /// </summary>
        public void StoreDisconnectCommand((UnitType, int) fromTypeID, (UnitType, int) toTypeID, int inletIndex)
        {
            //print("Disconnect");
            DisconnectCommand disconenctCommand = new();
            disconenctCommand.Init(unitController);
            disconenctCommand.Store(fromTypeID, toTypeID, inletIndex);
            AddCommand(disconenctCommand);
        }
        /// <summary>
        /// Store Disconnect Signal command
        /// </summary>
        public void StoreDisconnectSignalCommand((UnitType, int) fromTypeID, (UnitType, int) toTypeID, int inputIndex)
        {
            //print("DisconnectSignal");
            DisconnectSignalCommand disconenctSignalCommand = new();
            disconenctSignalCommand.Init(unitController);
            disconenctSignalCommand.Store(fromTypeID, toTypeID, inputIndex);
            AddCommand(disconenctSignalCommand);
        }
        /// <summary>
        /// Store Move command
        /// </summary>
        public void StoreMoveCommand(List<(UnitType, int)> typeIDs, Vector3 fromLocation = default, Vector3 toLocation = default)
        {
            if ((toLocation - fromLocation).magnitude > 0)
            {
                //print("Move");
                MoveCommand moveCommand = new();
                moveCommand.Init(unitController);
                moveCommand.Store(typeIDs, fromLocation, toLocation);
                AddCommand(moveCommand);
            }
        }

        /// <summary>
        /// Get stored user config without reading from file
        /// </summary>
        /// <returns></returns>
        public UserConfig GetStoredUserConfig()
        {
            return config;
        }

        /// <summary>
        /// Explicitly read user config from file
        /// </summary>
        /// <returns></returns>
        public UserConfig ReadUserConfig()
        {
            string path = SOURCE_DIR + "config.json";
            if (System.IO.File.Exists(path))
            {
                config = JsonUtility.FromJson<UserConfig>(System.IO.File.ReadAllText(path));
            }

            return config;
        }

        /// <summary>
        /// Overwrite user config file
        /// </summary>
        /// <param name="config"></param>
        public void WriteUserConfig(UserConfig config_arg)
        {
            config = config_arg;
            string path = SOURCE_DIR + "config.json";
            File.WriteAllText(path, JsonUtility.ToJson(config));
        }

        /// <summary>
        /// Read user config from file and apply properties
        /// </summary>
        private void ApplyUserConfig()
        {
            config = ReadUserConfig();
            UnitController.SetGlobalVolume(config.volume);
            Clock.SetBPM(config.bpm);
            CameraController.SetRenderScaleDivider(config.resolutionDivider);
            CameraController.SetPostProcessingStatus(config.postProcessing);
        }

        /// <summary>
        /// Get edit status
        /// </summary>
        /// <returns></returns>
        public bool GetEditStatus()
        {
            return edit;
        }

        /// <summary>
        /// Get Unit Selection Length
        /// </summary>
        /// <returns></returns>
        public int GetUnitSelectionLength()
        {
            return unitsSelected.Count;
        }

        /// <summary>
        /// Get First Selected Unit
        /// </summary>
        /// <returns></returns>
        public Unit GetFirstSelectedUnit()
        {
            if (GetUnitSelectionLength() == 0)
            {
                return null;
            }

            return unitsSelected[0];
        }

        /// <summary>
        /// Set Drag Offset
        /// </summary>
        /// <param name="mousePos"></param>
        public void SetDragOffset(Vector3 mousePos = default)
        {
            dragOffset.Clear();
            for (int i = 0; i < unitsSelected.Count; i++)
            {
                if(i == 0)
                {
                    dragFrom = GetFirstSelectedUnit().transform.position;
                }
                Vector3 offset = unitsSelected[i].transform.position - mousePos;
                dragOffset.Add(offset);
            }
        }

        /// <summary>
        /// Drag units
        /// </summary>
        /// <param name="mousePos"></param>
        public void Drag(Vector3 mousePos = default)
        {
            for (int i = 0; i < unitsSelected.Count; i++)
            {
                unitsSelected[i].transform.position = mousePos + dragOffset[i];
            }
        }

        /// <summary>
        /// End drag operation
        /// </summary>
        public void EndDrag()
        {
            if(unitsSelected.Count > 0)
            {
                List<(UnitType, int)> typeIDs = new();
                Vector3 dragTo = default;
                for (int i = 0; i < unitsSelected.Count; i++)
                {
                    if (i == 0)
                    {
                        dragTo = GetFirstSelectedUnit().transform.position;
                    }
                    typeIDs.Add(unitsSelected[i].GetTypeID());
                }
                StoreMoveCommand(typeIDs, dragFrom, dragTo);
            }
        }

        /// <summary>
        /// Set the start position of select box
        /// </summary>
        /// <param name="mousePos"></param>
        public void SetSelectBoxAnchor(Vector3 mousePos = default)
        {
            selectboxAnchor = mousePos;
        }

        /// <summary>
        /// Update select box scale and select units inside
        /// </summary>
        /// <param name="mousePos"></param>
        public void UpdateSelectBox(Vector3 mousePos = default)
        {
            SelectBox.SetActive(true);

            float width = (mousePos.x - selectboxAnchor.x) * 0.1f;
            float height = (mousePos.z - selectboxAnchor.z) * 0.1f;
            SelectBox.transform.localScale = new Vector3(width, 1f, height);
            SelectBox.transform.localPosition = new Vector3(width * 5f, 0.1f, height * 5f) + selectboxAnchor;
        } 

        /// <summary>
        /// Disable select box
        /// </summary>
        public void EndSelectBox()
        {
            SelectBox.SetActive(false);
        }

        /// <summary>
        /// Static function to spawn a cable
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Cable SpawnCable(Transform transform, Vector3 pos = default)
        {
            Cable cable = Instantiate(Cable_PF_static);
            cable.targetPosition = pos;
            cable.transform.parent = transform;
            cable.transform.localPosition = new Vector3(0f, 0f, 0f);

            return cable;
        }

        /// <summary>
        /// Split meta dictionaries into keys + vals lists
        /// </summary>
        /// <param name="up"></param>
        private void SplitMeta(ref UnitProperties up)
        {
            if(up.metaString != null)
            {
                up.metaStringKeys = up.metaString.Keys.ToList();
                up.metaStringVals = up.metaString.Values.ToList();
            }
            if (up.metaInt != null)
            {
                up.metaIntKeys = up.metaInt.Keys.ToList();
                up.metaIntVals = up.metaInt.Values.ToList();
            }
            if (up.metaFloat != null)
            {
                up.metaFloatKeys = up.metaFloat.Keys.ToList();
                up.metaFloatVals = up.metaFloat.Values.ToList();
            } 
        }

        /// <summary>
        /// Construct meta dictionaries from keys + vals lists
        /// </summary>
        /// <param name="up"></param>
        private void MergeMeta(ref UnitProperties up)
        {
            if (up.metaStringKeys != null)
            {
                up.metaString = new Dictionary<string, string>();

                for (int i = 0; i < up.metaStringKeys.Count; i++)
                {
                    up.metaString.Add(up.metaStringKeys[i], up.metaStringVals[i]);
                }
            }
            if (up.metaIntKeys != null)
            {
                up.metaInt = new Dictionary<string, int>();

                for (int i = 0; i < up.metaIntKeys.Count; i++)
                {
                    up.metaInt.Add(up.metaIntKeys[i], up.metaIntVals[i]);
                }
            }
            if (up.metaFloatKeys != null)
            {
                up.metaFloat = new Dictionary<string, float>();

                for (int i = 0; i < up.metaFloatKeys.Count; i++)
                {
                    up.metaFloat.Add(up.metaFloatKeys[i], up.metaFloatVals[i]);
                }
            }
        }


        /// <summary>
        /// Save project
        /// </summary>
        public void SaveProject()
        {
            // Set browser status to true
            menuController.SetBrowserStatus(true);

            // Set filters
            FileBrowser.SetFilters(false, new FileBrowser.Filter("RayTone Project Files", ".rt"));
            // Set default filter
            FileBrowser.SetDefaultFilter(".rt");

            // Show browser
            FileBrowser.ShowSaveDialog((paths) =>
            {
                List<Unit> unitsTemp = new();

                // Select all units and copy lists
                SelectAllUnits();
                foreach (Unit unit in unitsSelected)
                {
                    unitsTemp.Add(unit);
                }
                RayToneProject project = unitController.GetProjectData(unitsSelected);
                DeselectAllUnits();

                // Create Project directory
                string projectDir = Path.ChangeExtension(paths[0], null) + "_RayToneProject";
                // Overwrite project directory if a user decided to replace an existing .rt file.
                if (File.Exists(paths[0]))
                {
                    projectDir = Path.GetDirectoryName(paths[0]);
                }
                projectDir = projectDir.Replace('\\', '/');
                string projectFile = Path.GetFileName(paths[0]);
                string assetsDir = projectDir + "/Assets";
                Directory.CreateDirectory(projectDir);
                Directory.CreateDirectory(assetsDir);

                // Copy all assets to new directory
                for (int i = 0; i < project.unit_properties.Length; i++)
                {
                    if (project.unit_properties[i].metaString != null && project.unit_properties[i].metaString.ContainsKey("file"))
                    {
                        string path = project.unit_properties[i].metaString["file"];

                        if (!string.IsNullOrEmpty(path))
                        {
                            if(!File.Exists(assetsDir + "/" + Path.GetFileName(path)))
                            {
                                // Copy!
                                File.Copy(path, assetsDir + "/" + Path.GetFileName(path));

                                // WARNING: UPDATING ASSET REFERENCE!
                                unitsTemp[i].SetFilePath(assetsDir + "/" + Path.GetFileName(path));
                                unitsTemp[i].ReattachFilePath();
                            }
                            project.unit_properties[i].metaString["file"] = Path.GetFileName(path);
                        }
                    }
                    SplitMeta(ref project.unit_properties[i]);
                }

                string json = JsonUtility.ToJson(project);
                File.WriteAllText(projectDir + "/" + projectFile, json);

                if(File.Exists(projectDir + "/" + projectFile))
                {
                    Console.Log("Successfully saved " + projectFile + ". \nWarning: All units have updated asset references to " + assetsDir + ".\n", true);
                }

                // Set browser status to false on success
                menuController.SetBrowserStatus(false);
            },
            () =>
            {
                // Set browser status to false on cancel
                menuController.SetBrowserStatus(false);
            }
            , FileBrowser.PickMode.Files, false, null, null, "Save RayTone Project File", "Select");
        }

        /// <summary>
        /// Save project without updating asset references.
        /// This function has duplicated code with SaveProject and SelectAllUnits but is seperated due to its unique behavior.
        /// </summary>
        public void AutoSaveProject()
        {
            // Get references to all units
            List<Unit> unitsTemp = new();
            foreach (Unit unit in unitController.controls.Values)
            {
                unitsTemp.Add(unit);
            }
            foreach (Unit unit in unitController.voices.Values)
            {
                unitsTemp.Add(unit);
            }
            foreach (Unit unit in unitController.graphics.Values)
            {
                unitsTemp.Add(unit);
            }

            // Collect project data
            RayToneProject project = unitController.GetProjectData(unitsTemp);

            // Define Project directory
            string projectDir = BASE_DIR + "/AutoSave/AutoSave_RayToneProject";
            projectDir = projectDir.Replace('\\', '/');
            string projectFile = "AutoSave.rt";
            string assetsDir = projectDir + "/Assets";
            List<string> currentAssets = new();

            // Create directories if necessary
            if (!Directory.Exists(projectDir)) Directory.CreateDirectory(projectDir);
            if (!Directory.Exists(assetsDir)) Directory.CreateDirectory(assetsDir);

            // Copy all assets to new directory
            for (int i = 0; i < project.unit_properties.Length; i++)
            {
                if (project.unit_properties[i].metaString != null && project.unit_properties[i].metaString.ContainsKey("file"))
                {
                    string path = project.unit_properties[i].metaString["file"];

                    if (!string.IsNullOrEmpty(path))
                    {
                        // Copy!
                        File.Copy(path, assetsDir + "/" + Path.GetFileName(path), true);
                        project.unit_properties[i].metaString["file"] = Path.GetFileName(path);
                        currentAssets.Add(Path.GetFileName(path));
                    }
                }
                SplitMeta(ref project.unit_properties[i]);
            }

            string json = JsonUtility.ToJson(project);
            File.WriteAllText(projectDir + "/" + projectFile, json);

            // Delete assets that are not in the newly auto-saved project so that the Assets folder does not pile up
            List<string> existingAssets = Directory.GetFiles(assetsDir).ToList();
            foreach (string existingAsset in existingAssets)
            {
                if (!currentAssets.Contains(Path.GetFileName(existingAsset)))
                {
                    File.Delete(existingAsset);
                }
            }

            // print("Autosave: " + Time.time);
        }

        /// <summary>
        /// Load project
        /// </summary>
        public void LoadProject()
        {
            // Set browser status to true
            menuController.SetBrowserStatus(true);

            // Set filters
            FileBrowser.SetFilters(false, new FileBrowser.Filter("RayTone Project Files", ".rt"));
            // Set default filter
            FileBrowser.SetDefaultFilter(".rt");

            // Show browser
            FileBrowser.ShowLoadDialog((paths) =>
            {
                string json = System.IO.File.ReadAllText(paths[0]);
                RayToneProject project = JsonUtility.FromJson<RayToneProject>(json);

                for(int i = 0; i < project.unit_properties.Length; i++)
                {
                    MergeMeta(ref project.unit_properties[i]);
                }

                project.directory = Path.GetDirectoryName(paths[0]);
                Unit[] units = unitController.AddProject(project);
                StoreSpawnCommand(units);

                // Set browser status to false on success
                menuController.SetBrowserStatus(false);
            },
            () =>
            {
                // Set browser status to false on cancel
                menuController.SetBrowserStatus(false);
            }
            , FileBrowser.PickMode.Files, false, null, null, "Load RayTone Project File", "Select");
        }

        /// <summary>
        /// Add unit to selection list
        /// </summary>
        /// <param name="unit"></param>
        public void SelectUnit(Unit unit)
        {
            if (unit != null && !unit.GetSelectionStatus())
            {
                unitsSelected.Add(unit);
                unit.OnSelect();
                unit.SetSelectionStatus(true);
            }
        }

        /// <summary>
        /// Select all units
        /// </summary>
        public void SelectAllUnits()
        {
            foreach (Unit unit in unitController.controls.Values)
            {
                SelectUnit(unit);
            }
            foreach (Unit unit in unitController.voices.Values)
            {
                SelectUnit(unit);
            }
            foreach (Unit unit in unitController.graphics.Values)
            {
                SelectUnit(unit);
            }
        }

        /// <summary>
        /// Remove Unit selection
        /// </summary>
        /// <param name="unit"></param>
        public void DeselectUnit(Unit unit)
        {
            if (unit != null)
            {
                unitsSelected.Remove(unit);
                unit.OnDeselect();
                unit.SetSelectionStatus(false);
            }
        }

        /// <summary>
        /// Remove all units from selection
        /// </summary>
        public void DeselectAllUnits()
        {
            foreach (Unit unit in unitsSelected)
            {
                if (unit != null)
                {
                    unit.OnDeselect();
                    unit.SetSelectionStatus(false);
                }
            }

            unitsSelected.Clear();
        }

        /// <summary>
        /// Destroy selected units
        /// </summary>
        public void DestroySelectedUnits()
        {
            StoreDestroyCommand(unitsSelected.ToArray());

            foreach (Unit unit in unitsSelected)
            {
                // Check if selected object is Voice
                if (unit.TryGetComponent<VoiceUnit>(out VoiceUnit clickedVoice))
                {
                    // Destroy Voice
                    unitController.DestroyVoice(clickedVoice);
                }

                // Check if selected object is Control
                if (unit.TryGetComponent<ControlUnit>(out ControlUnit clickedControl))
                {
                    // Destroy Control
                    unitController.DestroyControl(clickedControl);
                }

                // Check if selected object is Graphics
                if (unit.TryGetComponent<GraphicsUnit>(out GraphicsUnit clickedGraphics))
                {
                    // Destroy Control
                    unitController.DestroyGraphics(clickedGraphics);
                }
            }

            unitsSelected.Clear();
        }

        /// <summary>
        /// Copy selected units
        /// </summary>
        public void CopyUnits()
        {
            copiedProject = unitController.GetProjectData(unitsSelected);
        }

        /// <summary>
        /// Paste copied units
        /// </summary>
        public void PasteUnits(bool atMouse)
        {
            Vector3 offset = new(Random.Range(3f, 8f), 0f, Random.Range(-3f, 3f));

            if (atMouse)
            {
                if(copiedProject.unit_properties != null && copiedProject.unit_properties.Length > 0)
                {
                    offset -= copiedProject.unit_properties[0].location;
                    offset += playerController.GetMousePosition();
                }
            }

            Unit[] units = unitController.AddProject(copiedProject, offset);
            StoreSpawnCommand(units);
        }

        /// <summary>
        /// Duplicate selected units
        /// </summary>
        public void DuplicateUnits()
        {
            CopyUnits();
            PasteUnits(false);
        }

        /// <summary>
        /// Enter Edit Mode
        /// </summary>
        public void EnterEdit()
        {
            if (!edit && GetFirstSelectedUnit() != null && GetFirstSelectedUnit().enableEdit)
            {
                GetFirstSelectedUnit().OnEnterEdit();

                if (GetFirstSelectedUnit().zoomOnEdit)
                {
                    cameraController.ZoomIn(GetFirstSelectedUnit().gameObject);
                    edit = true;
                }

                if (GetFirstSelectedUnit().enterMenuOnEdit)
                {
                    menuController.EnterUnitMenu(GetFirstSelectedUnit());
                }
            }
        }

        /// <summary>
        /// Exit Edit Mode
        /// </summary>
        public void ExitEdit()
        {
            if (edit && GetFirstSelectedUnit() != null)
            {
                GetFirstSelectedUnit().OnExitEdit();
                menuController.CloseMenu();
                if (GetFirstSelectedUnit().zoomOnEdit)
                {
                    cameraController.ZoomOut();
                }
                edit = false;
            }
        }

        /// <summary>
        /// Toggle Unit submenu if EnterMenuOnEdit is set to false
        /// </summary>
        public void ToggleUnitMenu()
        {
            if (!GetEditStatus() || GetFirstSelectedUnit().enterMenuOnEdit) return;

            if (menuController.GetMenuStatus() == MenuStatus.None)
            {
                menuController.EnterUnitMenu(GetFirstSelectedUnit());
            }
            else if (menuController.GetMenuStatus() == MenuStatus.UnitMenu)
            {
                menuController.ExitUnitMenu(GetFirstSelectedUnit());
            }
        }
    }
}
