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

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.IO;

namespace RayTone
{
    public class Webcam : GraphicsUnit
    {
        [SerializeField] private MeshRenderer mesh;

        private GraphicsController graphicsController;

        private WebCamTexture webcamTexture;
        private RenderTexture renderTexture;
        private Material material;
        private float width;
        private float height;
        private float widthMultiplier = 1f;
        private float heightMultiplier = 1f;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            graphicsController = GraphicsController.Instance;
            material = mesh.material;

            // Initialize renderTexture and webcam
            renderTexture = new(1920, 1080, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            StartWebcam();
        }

        /////
        //UPDATE
        private void Update()
        {
            // Blit textures
            if (webcamTexture) Graphics.Blit(webcamTexture, renderTexture);

            width = renderTexture.width;
            height = renderTexture.height;

            widthMultiplier = 1f;
            heightMultiplier = 1f;

            // width
            if (GetInletStatus(0))
            {
                widthMultiplier = GetInletVal(0);
            }
            // height
            if (GetInletStatus(1))
            {
                heightMultiplier = GetInletVal(1);
            }

            // update resolution
            if ((webcamTexture != null) && (renderTexture != null))
            {
                graphicsController.SetTextureResolution(renderTexture, width * widthMultiplier, height * heightMultiplier);
            }
            mesh.transform.localScale = new Vector3(width * widthMultiplier * 0.0015f, 0.01f, height * heightMultiplier * 0.0015f); //sweet scale multiplier...

            // opacity
            if (GetInletStatus(2))
            {
                material.color = new Color(1, 1, 1, GetInletVal(2));
            }
            else
            {
                material.color = new Color(1, 1, 1, 1);
            }
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            graphicsController.RemoveTexture(renderTexture);

            // clean-up
            Destroy(material);
            material = null;
            webcamTexture.Stop();
            webcamTexture = null;
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        /// <summary>
        /// texture ID
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            int id = 0;
            
            if (renderTexture)
            {
                id = graphicsController.GetTextureID(renderTexture);
            }

            StoreValue(id);
            return id;
        }

        /// <summary>
        /// Start webcam
        /// </summary>
        /// <returns></returns>
        private void StartWebcam()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Console.Log("Webcam Unit: No webcam detected.", true);
                return;
            }
            
            // Start webcam
            webcamTexture = new(1920, 1080);
            webcamTexture.Play();

            // Set material
            renderTexture.wrapMode = TextureWrapMode.Repeat;
            material.mainTexture = renderTexture;
            material.SetTexture("_BaseMap", renderTexture);
            material.SetTexture("_EmissionMap", renderTexture);

            // Store texture - we Blit once before storing to "activate" the texture.
            // Otherwise, NativeTexturePtr returns 0 the first time.
            Graphics.Blit(webcamTexture, renderTexture);
            graphicsController.AddTexture(renderTexture);
        }
    }
}
