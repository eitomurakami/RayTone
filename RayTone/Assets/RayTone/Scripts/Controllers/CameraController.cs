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
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RayTone
{ 
    public class CameraController : Singleton<CameraController>
    {
        private bool visible = true;
        private bool movable = true;
        private float intensity = 1f;

        private Vector3 cameraClickedPosition;
        private Vector3 mouseClickedPosition;
        private Vector3 positionTemp;

        [SerializeField] private UnityEngine.Rendering.Volume volume;
        static private UnityEngine.Rendering.Volume volume_static;
        // dynamic? resolution
        [SerializeField] private float scale;
        private UniversalRenderPipelineAsset urp;
        public static float renderScaleDivider = 2f;

        // used only for in-game cursor
        [SerializeField] private Texture2D cursor;

        // controller reference
        private RayToneController raytoneController;
        private PlayerController playerController;
        private MenuController menuController;

        /////
        //START
        private void Start()
        {
            // Store as static
            volume_static = volume;

            urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            Application.targetFrameRate = 60;
            //Cursor.SetCursor(cursor, new Vector2(0.5f, 0.5f), CursorMode.ForceSoftware);

            // Get controller references
            raytoneController = RayToneController.Instance;
            playerController = PlayerController.Instance;
            menuController = MenuController.Instance;
        }

        /////
        //UPDATE
        void Update()
        {
            // Dynamically down-scale
            scale = Mathf.Min(3840f / Screen.width / renderScaleDivider, 1);
            urp.renderScale = scale;

            // MOVE CAMERA
            if (movable && !menuController.GetBrowserStatus() && menuController.GetMenuStatus() == MenuStatus.None)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    cameraClickedPosition = this.transform.position;
                    mouseClickedPosition = playerController.GetMousePosition();
                }
                if (Input.GetMouseButton(1))
                {
                    Vector3 mouseCurrentPosition = playerController.GetMousePosition() - (this.transform.position - cameraClickedPosition);
                    transform.position = new Vector3(cameraClickedPosition.x - (mouseCurrentPosition.x - mouseClickedPosition.x), transform.position.y, cameraClickedPosition.z - (mouseCurrentPosition.z - mouseClickedPosition.z));
                }

                if (Input.GetKey("left shift") || Input.GetKey("right shift"))
                {
                    intensity = 30f;
                }
                else
                {
                    intensity = 15f;
                }

                if (Input.GetKey("up"))
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + Time.deltaTime * intensity);
                }

                else if (Input.GetKey("down"))
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - Time.deltaTime * intensity);
                }

                if (Input.GetKey("left"))
                {
                    transform.position = new Vector3(transform.position.x - Time.deltaTime * intensity, transform.position.y, transform.position.z);
                }

                else if (Input.GetKey("right"))
                {
                    transform.position = new Vector3(transform.position.x + Time.deltaTime * intensity, transform.position.y, transform.position.z);
                }

                if (Input.GetKey("="))
                {
                    if(Input.GetKey("left ctrl") || Input.GetKey("right ctrl") || Input.GetKey("left cmd") || Input.GetKey("right cmd"))
                    {
                        transform.position = new Vector3(transform.position.x, Mathf.Max(transform.position.y - Time.deltaTime * intensity, 10f), transform.position.z);
                    }
                }

                else if (Input.GetKey("-"))
                {
                    if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl") || Input.GetKey("left cmd") || Input.GetKey("right cmd"))
                    {
                        transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime * intensity, transform.position.z);
                    }    
                }

                // iOS pinch zoom
                if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    float magnitude = (touchZero.position - touchOne.position).magnitude;
                    float magnitudePrev = ((touchZero.position - touchZero.deltaPosition) - (touchOne.position - touchOne.deltaPosition)).magnitude;

                    float delta = magnitude - magnitudePrev;
                    transform.position = new Vector3(transform.position.x, Mathf.Max(transform.position.y - delta * 0.005f * intensity, 10f), transform.position.z);
                }

                // Mouse scroll
                transform.position = new Vector3(transform.position.x, Mathf.Max(transform.position.y - Input.mouseScrollDelta.y * 0.1f * intensity, 10f), transform.position.z);
            }
        }

        /// <summary>
        /// Set camera mobility
        /// </summary>
        /// <param name="b"></param>
        public void SetMobility(bool moveable_arg)
        {
            movable = moveable_arg;
        }

        // ZoomIn
        public void ZoomIn(GameObject target)
        {
            // Disable movement
            movable = false;

            // Save current position
            positionTemp = transform.position;

            transform.position = target.transform.position + new Vector3(1f, 5f, -8f);
            transform.rotation = Quaternion.Euler(30f, 0, 0);
        }

        // ZoomOut
        public void ZoomOut()
        {
            // Enable movement
            movable = true;

            transform.position = positionTemp;
            transform.rotation = Quaternion.Euler(90f, 0, 0);
        }

        /// <summary>
        /// Get visibility
        /// </summary>
        /// <returns></returns>
        public bool GetVisibility()
        {
            return visible;
        }

        /// <summary>
        /// Toggle visibility
        /// </summary>
        /// <param name="arg"></param>
        public void ToggleVisibility(bool arg)
        {
            if (arg)
            {
                Cursor.visible = true;
                Camera.main.cullingMask |= (1 << LayerMask.NameToLayer("RayToneMain"));
                Camera.main.cullingMask |= (1 << LayerMask.NameToLayer("RayToneGraphics"));
                visible = true;
            }
            else
            {
                if (raytoneController.GetStoredUserConfig().fullScreen)
                {
                    Cursor.visible = false;
                } 
                Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("RayToneMain"));
                Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("RayToneGraphics"));
                visible = false;
            }
        }

        // Second display
        public static void ActivateSecondDisplay()
        {
            if(Display.displays.Length > 1)
            {
                Display.displays[1].Activate();
            }
        }

        // Graphics scalability
        public static void SetRenderScaleDivider(float arg)
        {
            renderScaleDivider = arg;
        }
        public static float GetRenderScaleDivider()
        {
            return renderScaleDivider;
        }
        public static void SetPostProcessingStatus(bool enabled)
        {
            volume_static.enabled = enabled;
        }
        public static bool GetPostProcessingStatus()
        {
            return volume_static.enabled;
        }
        public static void SetAntiAliasingFactor(int arg)
        {
            //QualitySettings.antiAliasing = arg;
        }
        public static int GetAntiAliasingFactor()
        {
            return QualitySettings.antiAliasing;
        }
    }
}
