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
using SimpleFileBrowser;

namespace RayTone
{
    public enum MenuStatus
    {
        None,
        Settings,
        Control,
        Voice,
        Graphics,
        UnitMenu
    }

    public class MenuController : Singleton<MenuController>
    {
        // prefab references
        [SerializeField] private Menu_Global Menu_Global_PF;
        [SerializeField] private Menu_Settings Menu_Settings_PF;
        [SerializeField] private Menu_Sequencer Menu_Sequencer_PF;
        [SerializeField] private Menu_Voice Menu_Voice_PF;
        [SerializeField] private Menu_Softkeys1 Menu_Softkeys1_PF;
        [SerializeField] private SelectTextButton SelectTextButton_PF;
        [SerializeField] private Splash Splash_PF;

        // version text
        [SerializeField] private string version;
        
        // controller references
        private RayToneController raytoneController;
        private UnitController unitController;
        private PlayerController playerController;  
        private CameraController cameraController;

        // private references
        private Menu_Global menu_global = null;
        private Menu_Settings menu_settings = null;
        private Menu_Unit menu_unit = null;
        private Menu_Softkeys1 menu_softkeys1 = null;

        // menu status
        private MenuStatus menuStatus = MenuStatus.None;
        private bool browserStatus = false; 

        /////
        //AWAKE
        protected override void Awake()
        {
            base.Awake();
        }
        
        /////
        //Start
        void Start()
        {
            // Get controller references
            raytoneController = RayToneController.Instance;
            unitController = UnitController.Instance;
            playerController = PlayerController.Instance;
            cameraController = CameraController.Instance;

            // Display Splash with version text 
            Console.Log("RayTone " + version + "\n");
            Splash splash = Instantiate(Splash_PF);
            splash.SetVersionText(version);
            if (raytoneController.ReadUserConfig().firstTime)
            {
                splash.DisplayIntro();
            }

            // Spawn Softkeys
            menu_softkeys1 = Instantiate(Menu_Softkeys1_PF);

            // Add streaming asseets to FileBrowser
            FileBrowser.AddQuickLink("RayTone Source", RayToneController.BASE_DIR);

            // Add tutorials to FileBrowser
            FileBrowser.AddQuickLink("Tutorials", RayToneController.BASE_DIR + "/Tutorials/");
        }

        /////
        //UPDATE
        void Update()
        {
            // Attach menu global to camera
            if (menu_global != null)
            {
                menu_global.transform.parent = cameraController.transform;
                menu_global.transform.localRotation = Quaternion.Euler(0, 0, 0);
                menu_global.transform.localPosition = new Vector3(0f, 0f, 9f);
            }

            // Set Softkeys at right corner
            Vector3 rightcorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane + 9f));
            Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane + 9f));
            Vector3 direction = (center - rightcorner).normalized;
            menu_softkeys1.transform.position = rightcorner + direction * 0.5f;
            menu_softkeys1.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(-90, 0, 0);
        }

        /// <summary>
        /// Get menu status
        /// </summary>
        /// <returns></returns>
        public MenuStatus GetMenuStatus()
        {
            return menuStatus;
        }

        /// <summary>
        /// Set browser status
        /// </summary>
        /// <param name="status"></param>
        public void SetBrowserStatus(bool status)
        {
            browserStatus = status;
        }
        /// <summary>
        /// Get browser status
        /// </summary>
        /// <returns></returns>
        public bool GetBrowserStatus()
        {
            return browserStatus;
        }

        /// <summary>
        /// Open file browser to spawn voice with file
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        public void CallSpawnVoiceWithFile(string chuckName, Vector3 pos)
        {
            // Set browser status to true
            SetBrowserStatus(true);

            // Set filters
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Audio Files", ".wav"));
            // Set default filter
            FileBrowser.SetDefaultFilter(".wav");

            // Show browser
            FileBrowser.ShowLoadDialog((paths) =>
            {
                Unit unitTemp = unitController.SpawnVoice(chuckName, paths[0].Replace("\\", "/"), pos);
                if (unitTemp)
                {
                    raytoneController.StoreSpawnCommand(new Unit[] { unitTemp });
                }

                // Set browser status to false on success
                SetBrowserStatus(false);
            },
            () =>
            {
                // Set browser status to false on cancel
                SetBrowserStatus(false);
            }
            , FileBrowser.PickMode.Files, false, null, null, "Load Audio File", "Select");
        }

        /// <summary>
        /// Open file browser to spawn graphics with file
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        public void CallSpawnGraphicsWithFile(string className, Vector3 pos, string[] extensions)
        {
            if (extensions.Length == 0)
            {
                unitController.SpawnGraphics(className, "", pos);
                return;
            }

            // Set browser status to true
            SetBrowserStatus(true);

            // Set filters
            string fileType = string.Join(" ", extensions);
            FileBrowser.SetFilters(false, new FileBrowser.Filter(fileType, extensions));

            // Show browser
            FileBrowser.ShowLoadDialog((paths) =>
            {
                Unit unitTemp = unitController.SpawnGraphics(className, paths[0].Replace("\\", "/"), pos);
                if (unitTemp)
                {
                    raytoneController.StoreSpawnCommand(new Unit[] { unitTemp });
                }

                // Set browser status to false on success
                SetBrowserStatus(false);
            },
            () =>
            {
                // Set browser status to false on cancel
                SetBrowserStatus(false);
            }
            , FileBrowser.PickMode.Files, false, null, null, "Load Assets", "Select");
        }

        /// <summary>
        /// Triggered on item click
        /// </summary>
        /// <param name="index"></param>
        public void OnSelectTextButtonClick(string className)
        {
            Vector3 randomoffset = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
            Vector3 pos = playerController.GetMousePosition() + randomoffset;

            // Deselect all before spawning a new unit
            raytoneController.DeselectAllUnits();

            Unit unitTemp;
            switch (menuStatus)
            {
                case MenuStatus.Control:
                    unitTemp = unitController.SpawnControl(className, pos);
                    if (unitTemp)
                    {
                        raytoneController.StoreSpawnCommand(new Unit[] { unitTemp });
                    }
                    break;
                case MenuStatus.Voice:
                    // Check if Voice requires file loading
                    if (unitController.CheckVoiceFileRequirement(className))
                    {
                        CallSpawnVoiceWithFile(className, pos);  // Open file browser and load file path
                    }
                    else
                    {
                        unitTemp = unitController.SpawnVoice(className, "", pos);
                        if (unitTemp)
                        {
                            raytoneController.StoreSpawnCommand(new Unit[] { unitTemp });
                        }
                    }
                    break;
                case MenuStatus.Graphics:
                    CallSpawnGraphicsWithFile(className, pos, unitController.GetGraphicsRequiredExtensions(className));
                    break;
            }

            CloseMenu();
        }

        /// <summary>
        /// Close all menu
        /// </summary>
        public void CloseMenu()
        {
            if (menu_global != null)
            {
                Destroy(menu_global.gameObject);
                menu_global = null;
            }

            if(menu_unit != null)
            {
                Destroy(menu_unit.gameObject);
                menu_unit = null;
            }

            menuStatus = MenuStatus.None;
        }

        /// <summary>
        /// Open/Close Settings Menu
        /// </summary>
        public void ToggleSettingsMenu()
        {
            // Open
            if (menuStatus != MenuStatus.Settings)
            {
                CloseMenu();
                menu_settings = Instantiate(Menu_Settings_PF);
                menuStatus = MenuStatus.Settings;

                menu_global = menu_settings;    // reducing code by making menu_settings a child of menu_global
            }
            // Close
            else
            {
                CloseMenu();
            }
        }

        /// <summary>
        /// Open/Close Control Selection Menu
        /// </summary>
        public void ToggleControlMenu()
        {
            // Open
            if (menuStatus != MenuStatus.Control)
            {
                CloseMenu();
                menu_global = Instantiate(Menu_Global_PF);
                
                menu_global.SetCategories(System.Enum.GetNames(typeof(ControlCategory)));
                menu_global.SetItems(unitController.GetControlItemsByCategory());
                menuStatus = MenuStatus.Control;
            }
            // Close
            else
            {
                CloseMenu();
            }
        }

        /// <summary>
        /// Open/Close Voice Selection Menu
        /// </summary>
        public void ToggleVoiceMenu()
        {
            // Open
            if (menuStatus != MenuStatus.Voice)
            {
                CloseMenu();
                menu_global = Instantiate(Menu_Global_PF);

                menu_global.SetCategories(unitController.GetChuckCategories());
                menu_global.SetItems(unitController.GetChuckItemsByCategory());

                menuStatus = MenuStatus.Voice;
            }
            // Close
            else
            {
                CloseMenu();
            }
        }

        /// <summary>
        /// Open/Close Graphics Selection Menu
        /// </summary>
        public void ToggleGraphicsMenu()
        {
            // Open
            if (menuStatus != MenuStatus.Graphics)
            {
                CloseMenu();
                menu_global = Instantiate(Menu_Global_PF);

                menu_global.SetCategories(System.Enum.GetNames(typeof(GraphicsCategory)));
                menu_global.SetItems(unitController.GetGraphicsItemsByCategory());
                
                menuStatus = MenuStatus.Graphics;
            }
            // Close
            else
            {
                CloseMenu();
            }
        }

        /// <summary>
        /// Open Unit submenu
        /// </summary>
        /// <param name="unitTemp"></param>
        public void EnterUnitMenu(Unit unitTemp)
        {
            if (unitTemp == null) return;

            if (unitTemp.UnitMenu_PF)
            {
                // Spawn menu prefab and notify unit
                unitTemp.OnEnterMenu();
                menu_unit = Instantiate(unitTemp.UnitMenu_PF);

                // Attach to camera
                menu_unit.transform.parent = cameraController.transform;
                menu_unit.transform.localRotation = Quaternion.Euler(0, 0, 0);
                menu_unit.transform.localPosition = new Vector3(0f, -1.5f, 4.5f);
                float scale = Mathf.Clamp((Screen.width / (float)Screen.height) / 1.78f, 0.1f, 1.0f);  // adaptive scaling using 16:9 as default
                menu_unit.transform.localScale = new Vector3(scale, scale, scale);
                menu_unit.parentUnit = unitTemp;
                menu_unit.menuController = this;

                menuStatus = MenuStatus.UnitMenu;
            }  
        }
        /// <summary>
        /// Close Unit submenu
        /// </summary>
        public void ExitUnitMenu(Unit unitTemp)
        {
            if (unitTemp == null) return;

            unitTemp.OnExitMenu();
            CloseMenu();
        }
    }
}
